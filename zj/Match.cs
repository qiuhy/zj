using System;
using System.Collections.Generic;
using System.Text;

namespace zj
{
    class Match : IComparable<Match>
    {
        private List<Bill> _matchBills1 = new List<Bill>();
        private List<Bill> _matchBills2 = new List<Bill>();
        private double _sum1 = 0;
        private double _sum2 = 0;

        public Match()
        {
        }
        public Match(Bill b)
        {
            Add1(b);
        }
        public Match(IEnumerable<Bill> billList)
        {
            foreach (Bill b in billList)
                Add1(b);
        }
        public Match(IEnumerable<Bill> billList1, IEnumerable<Bill> billList2)
            : this(billList1)
        {
            foreach (Bill b in billList2)
                Add2(b);
        }
        public Match(Match m)
            : this(m._matchBills1, m._matchBills2) { }

        public double sum1 { get => _sum1; }
        public double sum2 { get => _sum2; }
        public double diff { get => _sum1 - _sum2; }

        //匹配度
        public double rate { get => (_sum1 == 0) ? 0 : _sum2 / _sum1; }

        //偏离度
        public double deviation { get => Math.Abs(1 - rate); }

        public bool isMatch(double maxDeviation)
        {
            if (_matchBills1.Count == 0 || _matchBills2.Count == 0)
                return false;
            return maxDeviation == 0 ? diff == 0 : deviation < maxDeviation;
        }

        public List<Bill> toMatchBills { get => _matchBills1; }
        public List<Bill> matchedBills { get => _matchBills2; }

        public void Add1(Bill b) { _matchBills1.Add(b); _sum1 += b.amount; }
        public void Add2(Bill b) { _matchBills2.Add(b); _sum2 += b.amount; }

        public void Clear() { Clear1(); Clear2(); }
        private void Clear1() { _matchBills1.Clear(); _sum1 = 0; }
        private void Clear2() { _matchBills2.Clear(); _sum2 = 0; }

        public List<Match> GetMatchResult(List<Bill> billList, double deviation, int level)
        {
            List<Match> result = new List<Match>();
            int matchCount = 0;
            foreach (int[] idxs in new util.Combination(billList.Count, level))
            {
                Clear2();
                foreach (int i in idxs)
                {
                    Add2(billList[i]);
                }
                matchCount++;
                if (isMatch(deviation))
                {
                    result.Add(new Match(this));
                    if (deviation == 0)
                        break;
                }
            }
            result.Sort();
            return result;
        }

        public void UpdateMatchID()
        {
            int id = util.CommUtil.GetNextID();
            _matchBills1.ForEach(b => b.matchid = id);
            _matchBills2.ForEach(b => b.matchid = id);
        }

        public int CompareTo(Match m) => deviation.CompareTo(m.deviation);
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{_matchBills1.Count}v{_matchBills2.Count} deviation:{rate:p2} {deviation:p2}  diff: {_sum1:#,##0.00} - {_sum2:#,##0.00} = {diff:f2}");
            foreach (Bill b in _matchBills1)
                sb.AppendLine(b.ToString());
            foreach (Bill b in _matchBills2)
                sb.AppendLine(b.ToString());
            return sb.ToString();
        }
    }
}
