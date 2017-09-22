using System;
using System.Collections.Generic;
using System.Linq;
namespace zj
{
    static class Analyze
    {
        private static void updateMatchID(Match m)
        {
            int[] toMatchID = new int[m.toMatchBills.Count];
            int[] matchedID = new int[m.matchedBills.Count];
            int i = 0;

            for (i = 0; i < m.toMatchBills.Count; i++)
            {
                toMatchID[i] = m.toMatchBills[i].id;
            }

            i = 0;
            foreach (Bill b in m.matchedBills)
            {
                b.matchid = toMatchID;
                matchedID[i] = b.id;
            }

            foreach (Bill b in m.toMatchBills)
            {
                b.matchid = matchedID;
            }
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

        public static int Match_1vM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double maxDeviation, int maxDateRange, int maxLevel)
        {
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
                    matchedCount++;
                }
            }
            return matchedCount;
        }

        public static int Match_Mv1(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                , double maxDeviation, int maxDateRange, int maxLevel)
        {
            //就是反过来1vM
            return Match_1vM(outBills, inBills, maxDeviation, -maxDateRange, maxLevel);
        }

        public static int Match_MvM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double maxDeviation, int maxDateRange, int maxLevel)
        {
            int matchedCount = 0;
            int minLevel = 2; //最少匹配数
            List<Match> result = new List<Match>();
            foreach (var item in inBills.GroupBy(x => x.date.Date))
            {
                int count = item.Count();
                if (count < minLevel) continue;

                int curLevel = minLevel;
                int curResultCount = 0;
                IEnumerable<Bill> matchBills = outBills.Where(x => x.date > item.Key
                                            && x.date < item.Key.AddDays(maxDateRange + 1)
                                            && x.matchid == null);

                List<Bill> oneDayBills = new List<Bill>(item);
                List<Match> curResult = null;
                while (curLevel <= count)
                {
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
                        result.Add(curMatch);
                        matchedCount += curLevel;
                        curLevel = minLevel;
                        oneDayBills = new List<Bill>(item.Where(x => x.matchid == null));
                        count = oneDayBills.Count();
                    }
                }
            }
            return matchedCount;
        }
    }
}
