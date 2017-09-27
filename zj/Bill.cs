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
        public int matchid = 0;
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
}
