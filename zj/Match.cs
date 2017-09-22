using System;
using System.Collections.Generic;
using System.Text;
namespace zj
{
    class Match : IComparable<Match>
    {
        public delegate void onAfterMatch(List<Match> result, int matchCount, int listCount, int maxLevel);
        public static onAfterMatch afterMatch = null;

        public static readonly int matchMothed_In2Out = 0;
        public static readonly int matchMothed_Out2In = 1;

        private List<Bill> _toMatchBills;
        private List<Bill> _matchedBills = new List<Bill>();
        private double _amount = 0;
        private double _sum = 0;
        private double _allowDeviation = 0;
        private int _matchMothed = matchMothed_In2Out;

        public Match(Bill toMatch, double allowDeviation)
        {
            _toMatchBills = new List<Bill>();
            _toMatchBills.Add(toMatch);
            _amount = toMatch.amount;
            _allowDeviation = allowDeviation;
        }
        public Match(List<Bill> toMatchBills, double allowDeviation)
        {
            _toMatchBills = toMatchBills;
            foreach (Bill b in _toMatchBills)
            {
                _amount += b.amount;
            }
            _allowDeviation = allowDeviation;
        }

        public double sum { get => _sum; }
        public double diff { get => _amount - sum; }

        //匹配度
        public double rate { get => sum / _amount; }

        //偏离度
        public double deviation { get => Math.Abs(1 - rate); }

        public bool isMatch { get => _allowDeviation == 0 ? diff == 0 : deviation < _allowDeviation; }

        public List<Bill> toMatchBills { get => _toMatchBills; }

        public List<Bill> matchedBills { get => _matchedBills; }

        public List<Match> GetMatchResult(List<Bill> billList, int maxLevel)
        {
            List<Match> result = new List<Match>();
            int matchCount = 0;
            foreach (int[] idxs in new util.Combination(billList.Count, maxLevel))
            {
                _matchedBills.Clear();
                _sum = 0;
                foreach (int i in idxs)
                {
                    _matchedBills.Add(billList[i]);
                    _sum += billList[i].amount;
                }
                matchCount++;
                if (isMatch)
                    result.Add(this.Copy());
            }
            result.Sort();
            if (afterMatch != null && matchCount > 0)
                afterMatch(result, matchCount, billList.Count, maxLevel);
            return result;
        }

        public Match Copy()
        {
            Match m = new Match(this._toMatchBills, this._allowDeviation);
            m._matchedBills = new List<Bill>(this._matchedBills.ToArray());
            m._sum = this._sum;
            return m;
        }

        public int CompareTo(Match m) => deviation.CompareTo(m.deviation);
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"matched:{rate:p2} {deviation:p2}  diff: {_amount:#,##0.00} - {_sum:#,##0.00} = {diff:f2}");
            foreach (Bill b in _toMatchBills)
                sb.AppendLine(b.ToString());
            sb.AppendLine(new String('-', 80));
            foreach (Bill b in _matchedBills)
                sb.AppendLine(b.ToString());
            return sb.ToString();
        }
    }
}
