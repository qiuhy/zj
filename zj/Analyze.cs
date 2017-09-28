using System;
using System.Collections.Generic;
using System.Linq;
using static util.util;
namespace zj
{
    class Analyze
    {
        public delegate void onAfterMatch(List<Match> match, long matchCount, double progress);
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
                {
                    long matchCount = util.Math.CombinationCount(matchBills.Count, maxLevel, false);
                    afterMatch(r, matchCount, (double)doneCount / inCount);
                }
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
                while (oneDayBills.Count > inLevel)
                {
                    foreach (int[] idxs in new util.Combination(oneDayBills.Count, inLevel, false))
                    {
                        match.Clear();
                        foreach (int i in idxs)
                        {
                            match.Add1(oneDayBills[i]);
                        }
                        DateTime toMatchDate = match.toMatchBills[0].date;

                        List<Bill> curMatchBills = new List<Bill>(
                            matchBills.Where(x => x.date > toMatchDate
                                        && x.date < toMatchDate.AddDays(maxDateRange)));

                        curResult = match.GetMatchResult(curMatchBills, outLevel);
                        if (curResult.Count > 0)
                        {
                            Match curMatch = curResult[0];
                            curMatch.UpdateMatchID();
                            toMatchCount += curMatch.toMatchBills.Count;
                            matchedCount += curMatch.matchedBills.Count;
                            oneDayBills = new List<Bill>(item.Where(x => x.matchid == 0));
                        }
                        if (afterMatch != null)
                        {
                            long matchCount = util.Math.CombinationCount(curMatchBills.Count, outLevel, false)
                                            * util.Math.CombinationCount(oneDayBills.Count, inLevel, false);
                            afterMatch(curResult, matchCount, (double)doneCount / inCount);
                        }
                        if (curResult.Count > 0) break;
                    }
                    if (curResult.Count == 0) break;
                }
            }
            return new int[] { toMatchCount, matchedCount };
        }

        /* 在日期范围内 n对m 匹配
            IEnumerable<Bill> inBills;  //希望匹配的
            IEnumerable<Bill> outBills; //可用来匹配的
            double maxDeviation;        //最大误差，可为0
            int maxDateRange;           //日期范围
            int maxLevel;               //匹配数量范围
            int[] return[0]:inBills 匹配中的数量,return[1]:outBills 匹配中的数量
        */
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

        /* 在日期范围内顺序匹配
            IEnumerable<Bill> inBills;  //希望匹配的
            IEnumerable<Bill> outBills; //可用来匹配的
            double maxDeviation;        //最大误差，可为0
            int maxDateRange;           //日期范围
            int inLevel;                 //未使用
            int outLevel;               //未使用
            int[] return[0]:inBills 匹配中的数量,return[1]:outBills 匹配中的数量
        */
        public int[] Match_Day(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
            , double maxDeviation, int maxDateRange, int inLevel, int outLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int doneCount = 0;
            List<DateTime> inDates = new List<DateTime>(inBills.Select(x => x.date.Date).Distinct());
            int inCount = inDates.Count;
            Match match = new Match(maxDeviation);
            foreach (DateTime theDate in inDates)
            {
                doneCount++;
                bool matched = false;

                IEnumerable<Bill> oneDayBills = inBills.Union(outBills)
                                .Where(x => x.date >= theDate && x.date < theDate.AddDays(maxDateRange))
                                .OrderBy(x => x.date).ThenBy(x => x.id);
                do
                {
                    matched = false;
                    match.Clear();
                    int matchCount = oneDayBills.Count();
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
                                afterMatch(result, matchCount, (double)doneCount / inCount);
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
