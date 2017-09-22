using System;
using System.Collections.Generic;
using System.Linq;
namespace zj
{
    static class Analyze
    {
        private static void updateMatchID(Match m)
        {
            int[] toMatchID = m.toMatchBills.Select(b => b.id).ToArray();
            m.matchedBills.ForEach(b => b.matchid = toMatchID);

            int[] matchedID = m.matchedBills.Select(b => b.id).ToArray();
            m.toMatchBills.ForEach(b => b.matchid = matchedID);
        }

        private static List<Bill> getBillListByIndex(List<Bill> BillList, int[] idx)
        {
            List<Bill> bills = new List<Bill>();
            for (int i = 0; i < idx.Length; i++)
            {
                bills.Add(BillList[idx[i]]);
            }
            return bills;
        }

        public static int[] Match_1vM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double maxDeviation, int maxDateRange, int maxLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;

            foreach (Bill b in inBills.OrderBy(x => x.date).ThenBy(x => x.id))
            {
                DateTime minDate = (maxDateRange > 0) ? b.date : b.date.AddDays(maxDateRange);
                DateTime maxDate = (maxDateRange < 0) ? b.date : b.date.AddDays(maxDateRange);

                List<Bill> matchBills = new List<Bill>(
                       outBills.Where(x => x.date > minDate && x.date < maxDate)
                               .OrderBy(x => x.date)
                               );
                Match match = new Match(b, maxDeviation);
                List<Match> r = match.GetMatchResult(matchBills, maxLevel);
                if (r.Count > 0)
                {
                    updateMatchID(r[0]);
                    toMatchCount += r[0].toMatchBills.Count;
                    matchedCount += r[0].matchedBills.Count;
                }
            }
            return new int[] { toMatchCount, matchedCount };
        }

        public static int[] Match_Mv1(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                , double maxDeviation, int maxDateRange, int maxLevel)
        {
            //就是反过来1vM
            int[] result = Match_1vM(outBills, inBills, maxDeviation, -maxDateRange, maxLevel);
            int i;
            i = result[0];
            result[0] = result[1];
            result[1] = i;
            return result;
        }

        public static int[] Match_MvM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double maxDeviation, int maxDateRange, int maxLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;

            int minLevel = 2; //最少匹配数
            // List<Match> result = new List<Match>();
            foreach (var item in inBills.GroupBy(x => x.date.Date))
            {
                int count = item.Count();
                if (count < minLevel) continue;

                IEnumerable<Bill> matchBills = outBills.Where(x => x.date > item.Key
                                            && x.date < item.Key.AddDays(maxDateRange + 1)
                                            && x.matchid == null);

                List<Bill> oneDayBills = new List<Bill>(item);
                List<Match> curResult = null;
                int curLevel = minLevel;
                while (curLevel <= count)
                {
                    int curResultCount = 0;
                    foreach (int[] idx in new util.Combination(count, curLevel, false))
                    {
                        List<Bill> toMatchBills = getBillListByIndex(oneDayBills, idx);
                        Match m = new Match(toMatchBills, maxDeviation);
                        DateTime toMatchDate = toMatchBills.Select(x => x.date).Max();
                        List<Bill> curMatchBills = new List<Bill>(
                            matchBills.Where(x => x.date > toMatchDate
                                        && x.date < toMatchDate.AddDays(maxDateRange)));
                        curResult = m.GetMatchResult(curMatchBills, maxLevel);
                        curResultCount = curResult.Count;
                        if (curResultCount > 0) break;
                    }
                    if (curResultCount == 0)
                    {
                        curLevel++;
                    }
                    else
                    {
                        Match curMatch = curResult[0];
                        updateMatchID(curMatch);
                        toMatchCount += curMatch.toMatchBills.Count;
                        matchedCount += curMatch.matchedBills.Count;
                        curLevel = minLevel;
                        oneDayBills = new List<Bill>(item.Where(x => x.matchid == null));
                        count = oneDayBills.Count();
                    }
                }
            }
            return new int[] { toMatchCount, matchedCount };
        }
    }
}
