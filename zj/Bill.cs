using System;
using System.Text;
namespace zj
{
    class Bill : IComparable<Bill>
    {
        public int id = 0;
        public string name;
        public string acct;
        public string to_name;
        public string to_acct;
        public bool isOut;
        public DateTime date;
        public double amount;
        public double balance;
        public string comment;
        private int _matchid = 0;

        public virtual int matchid { get => _matchid; set => _matchid = value; }

        public int CompareTo(Bill b) => date.CompareTo(b.date);

        public override string ToString()
        {
            int chns = (Encoding.Default.GetByteCount(comment) - comment.Length) / 2;
            int adds = 24 - (comment.Length + chns);
            adds = adds < 0 ? 0 : adds;
            return $"{matchid,5} {id,5} {date:yyyy-MM-dd HH:mm:ss} {(isOut ? "-" : "+")}{amount,15:#,##0.00}"
                    + $" {comment + new string(' ', adds)} {(to_name + " " + to_acct).Trim()}";
        }
    }

    class MergedBill : Bill
    {
        public Bill[] sourceBill;

        // public override matchid
        public override int matchid
        {
            get => base.matchid; set
            {
                base.matchid = value;
                foreach (Bill b in sourceBill)
                    b.matchid = value;
            }
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder(base.ToString());
            foreach (Bill b in sourceBill)
            {
                str.Append($"\n\t{b.ToString()}");
            }
            return str.ToString();
        }
    }
}
