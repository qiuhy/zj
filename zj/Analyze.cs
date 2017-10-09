using System;
using System.Collections.Generic;
using System.Linq;
using util;
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

        private int[] match_1vM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double deviation, int dateRange, int level)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int inCount = inBills.Count();
            int doneCount = 0;

            foreach (Bill b in inBills.Where(x => x.matchid == 0).OrderBy(x => x.date).ThenBy(x => x.id))
            {
                doneCount++;
                Match match = new Match(b);

                DateTime minDate = (dateRange > 0) ? b.date : b.date.AddDays(dateRange);
                DateTime maxDate = (dateRange < 0) ? b.date : b.date.AddDays(dateRange);

                List<Bill> matchBills = outBills.Where(x => x.date > minDate && x.date < maxDate && x.matchid == 0)
                                                .OrderBy(x => x.date).ThenBy(x => x.id)
                                                .ToList();
                if (matchBills.Count == 0) continue;

                List<Match> r = match.GetMatchResult(matchBills, deviation, level);
                if (r.Count > 0)
                {
                    r[0].UpdateMatchID();
                    toMatchCount += r[0].toMatchBills.Count;
                    matchedCount += r[0].matchedBills.Count;
                }
                if (afterMatch != null)
                {
                    long matchCount = MathUtil.CombinationCount(matchBills.Count, level);
                    afterMatch(r, matchCount, (double)doneCount / inCount);
                }
            }
            return new int[] { toMatchCount, matchedCount };
        }

        private int[] match_MvM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                    , double deviation, int dateRange, int inLevel, int outLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int doneCount = 0;
            Match match = new Match();
            List<Match> curResult = null;
            IEnumerable<IGrouping<DateTime, Bill>> groupBills = inBills.Where(x => x.matchid == 0)
                                                                        .GroupBy(x => x.date.Date);
            int inCount = groupBills.Count();

            foreach (var item in groupBills)
            {
                doneCount++;
                if (item.Count() < inLevel) continue;
                List<Bill> oneDayBills = item.ToList();

                List<Bill> matchBills = outBills.Where(x => x.date > item.Key
                                            && x.date < item.Key.AddDays(dateRange + 1)
                                            && x.matchid == 0
                                            ).ToList();
                if (matchBills.Count < outLevel) continue;

                while (oneDayBills.Count > inLevel)
                {
                    foreach (int[] idxs in new Combination(oneDayBills.Count, inLevel))
                    {
                        match.Clear();
                        foreach (int i in idxs)
                        {
                            match.Add1(oneDayBills[i]);
                        }
                        DateTime toMatchDate = oneDayBills[idxs[0]].date;

                        List<Bill> curMatchBills = matchBills.Where(x => x.date > toMatchDate
                                                                    && x.date < toMatchDate.AddDays(dateRange)
                                                                    && x.matchid == 0)
                                                            .ToList();

                        curResult = match.GetMatchResult(curMatchBills, deviation, outLevel);
                        if (curResult.Count > 0)
                        {
                            Match curMatch = curResult[0];
                            curMatch.UpdateMatchID();
                            toMatchCount += curMatch.toMatchBills.Count;
                            matchedCount += curMatch.matchedBills.Count;
                            oneDayBills = item.Where(x => x.matchid == 0).ToList();
                        }
                        if (afterMatch != null)
                        {
                            long matchCount = MathUtil.CombinationCount(curMatchBills.Count, outLevel)
                                            * MathUtil.CombinationCount(oneDayBills.Count, inLevel);
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
            double deviation;        //最大误差，可为0
            int dateRange;           //日期范围
            int inLevel;               //inBills匹配数量范围
            int outLevel;               //outBills匹配数量范围
            int[] return[0]:inBills 匹配中的数量,return[1]:outBills 匹配中的数量
        */
        public int[] Match_MvM(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                , double deviation, int dateRange, int inLevel, int outLevel)
        {
            if (inLevel == 1)
            {
                return match_1vM(inBills, outBills, deviation, dateRange, outLevel);
            }
            else if (outLevel == 1)
            {
                //就是反过来1vM
                return match_1vM(inBills, outBills, deviation, -dateRange, inLevel);
            }
            else
            {
                return match_MvM(inBills, outBills, deviation, dateRange, inLevel, outLevel);
            }
        }

        /* 在日期范围内顺序匹配
            IEnumerable<Bill> inBills;  //希望匹配的
            IEnumerable<Bill> outBills; //可用来匹配的
            double deviation;        //最大误差，可为0
            int dateRange;           //日期范围
            int inLevel;                 //未使用
            int outLevel;               //未使用
            int[] return[0]:inBills 匹配中的数量,return[1]:outBills 匹配中的数量
        */
        public int[] Match_Day(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
            , double deviation, int dateRange, int inLevel, int outLevel)
        {
            int toMatchCount = 0;
            int matchedCount = 0;
            int doneCount = 0;
            List<DateTime> inDates = inBills.Where(x => x.matchid == 0)
                                            .Select(x => x.date.Date)
                                            .Distinct()
                                            .ToList();
            int inCount = inDates.Count;
            Match match = new Match();
            foreach (DateTime theDate in inDates)
            {
                doneCount++;
                bool matched = false;
                IEnumerable<Bill> oneDayinBills = inBills.Where(x => x.date >= theDate
                                                && x.date < theDate.AddDays(dateRange)
                                                && x.matchid == 0);
                IEnumerable<Bill> oneDayoutBills = outBills.Where(x => x.date >= theDate
                                                && x.date < theDate.AddDays(dateRange)
                                                && x.matchid == 0);

                IEnumerable<Bill> oneDayBills = oneDayinBills.Concat(oneDayoutBills)
                                                .OrderBy(x => x.date).ThenBy(x => x.id);
                do
                {
                    matched = false;
                    match.Clear();
                    int matchCount = oneDayBills.Count();
                    foreach (Bill b in oneDayBills)
                    {
                        if (b.isOut) match.Add2(b); else match.Add1(b);

                        if (match.isMatch(deviation))
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

        //单据匹配 同一天同一帐号按收付合并
        static public IEnumerable<MergedBill> Merge_Bill(IEnumerable<Bill> bills)
        {
            return bills.GroupBy(x => new
            {
                date = x.date.Date,
                to_acct = x.to_acct,
                isOut = x.isOut
            }).Where(g => g.Count() > 1)
            .Select(g => new MergedBill
            {
                id = g.Min(x => x.id),
                isOut = g.Key.isOut,
                date = g.Min(x => x.date),
                to_name = g.Max(x => x.to_name),
                to_acct = g.Key.to_acct,
                amount = g.Sum(x => x.amount),
                comment = String.Join(',', g.Select(x => x.comment).Distinct()),
                sourceBill = g.ToArray()
            });
        }
    }
}
