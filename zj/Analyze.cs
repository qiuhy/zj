using System;
using System.Collections.Generic;
using System.Linq;
using static util.util;
namespace zj
{
    class Analyze
    {
        public delegate void onAfterMatch(List<Match> result, int listCount, int maxLevel, double progress);
        private onAfterMatch afterMatch = null;

        public Analyze(onAfterMatch callback = null)
        {
            afterMatch = callback;
        }

        private List<Bill> getBillListByIndex(List<Bill> BillList, int[] idxs)
        {
            List<Bill> bills = new List<Bill>();
            foreach (int i in idxs)
            {
                bills.Add(BillList[i]);
            }
            return bills;
        }

        private int[] match_1vM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double maxDeviation, int maxDateRange, int maxLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int inCount = inBills.Count();
            int doneCount = 0;

            foreach (Bill b in inBills.OrderBy(x => x.date).ThenBy(x => x.id))
            {
                doneCount++;
                DateTime minDate = (maxDateRange > 0) ? b.date : b.date.AddDays(maxDateRange);
                DateTime maxDate = (maxDateRange < 0) ? b.date : b.date.AddDays(maxDateRange);

                List<Bill> matchBills = new List<Bill>(
                       outBills.Where(x => x.date > minDate && x.date < maxDate)
                               .OrderBy(x => x.date).ThenBy(x => x.id)
                               );
                if (matchBills.Count == 0) continue;
                Match match = new Match(maxDeviation, b);
                List<Match> r = match.GetMatchResult(matchBills, maxLevel, maxDateRange < 0);
                if (r.Count > 0)
                {
                    r[0].UpdateMatchID();
                    toMatchCount += r[0].toMatchBills.Count;
                    matchedCount += r[0].matchedBills.Count;
                }
                if (afterMatch != null)
                    afterMatch(r, matchBills.Count, maxLevel, (double)doneCount / inCount);
            }
            return new int[] { toMatchCount, matchedCount };
        }

        private int[] match_Mv1(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                , double maxDeviation, int maxDateRange, int maxLevel)
        {
            //就是反过来1vM
            return match_1vM(outBills, inBills, maxDeviation, -maxDateRange, maxLevel);
        }


        private int[] match_MvM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double maxDeviation, int maxDateRange, int inLevel, int outLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int doneCount = 0;
            Match match = new Match(maxDeviation);
            List<Match> curResult = null;
            IEnumerable<IGrouping<DateTime, Bill>> groupBills = inBills.GroupBy(x => x.date.Date);
            int inCount = groupBills.Count();

            foreach (var item in groupBills)
            {
                doneCount++;
                if (item.Count() < inLevel) continue;

                IEnumerable<Bill> matchBills = outBills.Where(x => x.date > item.Key
                                            && x.date < item.Key.AddDays(maxDateRange + 1)
                                            );
                if (matchBills.Count() == 0) continue;

                List<Bill> oneDayBills = new List<Bill>(item);
                while (oneDayBills.Count > 0)
                {
                    foreach (int[] idxs in new util.Combination(oneDayBills.Count, inLevel, false))
                    {
                        List<Bill> toMatchBills = getBillListByIndex(oneDayBills, idxs);
                        match.toMatchBills = getBillListByIndex(oneDayBills, idxs);
                        DateTime toMatchDate = toMatchBills[0].date;
                        List<Bill> curMatchBills = new List<Bill>(
                            matchBills.Where(x => x.date > toMatchDate
                                        && x.date < toMatchDate.AddDays(maxDateRange)));
                        curResult = match.GetMatchResult(curMatchBills, outLevel);
                        if (afterMatch != null)
                            afterMatch(curResult, curMatchBills.Count, outLevel, (double)doneCount / inCount);
                        if (curResult.Count > 0)
                        {
                            Match curMatch = curResult[0];
                            curMatch.UpdateMatchID();
                            toMatchCount += curMatch.toMatchBills.Count;
                            matchedCount += curMatch.matchedBills.Count;
                            oneDayBills = new List<Bill>(item.Where(x => x.matchid == 0));
                            break;
                        }
                    }
                    if (curResult.Count == 0) break;
                }
            }
            return new int[] { toMatchCount, matchedCount };
        }

        public int[] Match_MvM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                , double maxDeviation, int maxDateRange, int n, int m)
        {
            if (n == 1)
            {
                return match_1vM(inBills, outBills, maxDeviation, maxDateRange, m);
            }
            else if (m == 1)
            {
                return match_Mv1(inBills, outBills, maxDeviation, maxDateRange, n);
            }
            else
            {
                return match_MvM(inBills, outBills, maxDeviation, maxDateRange, n, m);
            }
        }

        public int[] Match_Day(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
            , double maxDeviation, int maxDateRange, int inLevel, int outLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int doneCount = 0;
            List<DateTime> inDates = new List<DateTime>(inBills.Select(x => x.date.Date).Distinct());
            int inCount = inDates.Count;
            // List<Match> result = new List<Match>();
            Match match = new Match(maxDeviation);
            foreach (DateTime theDate in inDates)
            {
                doneCount++;

                bool matched = false;
                do
                {
                    IEnumerable<Bill> oneDayBills = inBills.Where(x => x.date >= theDate && x.date < theDate.AddDays(maxDateRange))
                                            .Union(outBills.Where(x => x.date >= theDate && x.date < theDate.AddDays(maxDateRange)))
                                            .OrderBy(x => x.date).ThenBy(x => x.id);
                    matched = false;
                    match.Clear();
                    foreach (Bill b in oneDayBills)
                    {
                        if (b.isOut) match.Add2(b); else match.Add1(b);

                        if (match.isMatch)
                        {
                            matched = true;
                            toMatchCount += match.toMatchBills.Count;
                            matchedCount += match.matchedBills.Count;
                            match.UpdateMatchID();
                            if (afterMatch != null)
                            {
                                List<Match> result = new List<Match>();
                                result.Add(new Match(match));
                                afterMatch(result, match.toMatchBills.Count, match.matchedBills.Count, (double)doneCount / inCount);
                            }
                            break;
                        }
                    }
                } while (matched);
            }
            return new int[] { toMatchCount, matchedCount };
        }
    }
}
