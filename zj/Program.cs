using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace zj
{
    class Program
    {
        public delegate int MatchHandel(IEnumerable<Bill> inBills, IEnumerable<Bill> outBills
                                , double maxDeviation, int maxDateRange, int maxLevel);

        public delegate Bill Str2Bill(String str);

        // List<Bill> billList = createBillList(6000, DateTime.Parse("2017-01-01"), 365, 5);
        static List<Bill> createTestBills(int size, DateTime begdate, int dataRange = 5
            , int toRange = 3, double maxAmount = 10000, double minAmount = 1000)
        {
            Random rd = new Random();
            int intervalSeconds = dataRange * 3600 * 24 / size;
            List<Bill> billList = new List<Bill>();
            for (int i = 0; i < size; i++)
            {
                Bill b = new Bill();
                b.id = i + 1;
                int no = 1 + rd.Next(toRange);
                b.isOut = (rd.NextDouble() > 0.1);
                b.name = b.isOut ? "me" : "from_na" + no;
                b.acct = b.isOut ? "my_zh" : "from_zh" + no;
                b.to_name = b.isOut ? "to_na" + no : "me";
                b.to_acct = b.isOut ? "to_zh" + no : "my_zh";

                b.date = begdate.AddSeconds(intervalSeconds * i + rd.Next(intervalSeconds));
                b.amount = minAmount + Math.Round(rd.NextDouble() * (maxAmount - minAmount), 2);
                b.comment = "";
                billList.Add(b);
            }
            return billList;
        }

        private static Bill str2Bill1(String str)
        {
            try
            {
                Bill b = new Bill();
                string[] data = str.Split('\t');
                string sdate = String.Format("{0} {1:d6}", data[1], int.Parse(data[3]));
                b.date = DateTime.ParseExact(sdate, "yyyyMMdd HHmmss"
                        , System.Globalization.CultureInfo.CurrentCulture);
                b.acct = data[0].Trim();
                b.name = "";
                b.isOut = (data[6].Trim() == "0");
                b.to_acct = data[29].Trim();
                b.to_name = data[30].Trim();
                b.amount = double.Parse(data[7].Trim());
                b.balance = double.Parse(data[8].Trim());
                b.comment = data[21].Trim();
                return b;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        private static Bill str2Bill2(String str)
        {
            try
            {
                /*
                帐号	交易日期	交易时间	交易金额	帐户余额	对方帐号	对方户名
                交易码	凭证代号	借贷标记	余额性质	余额方向	营业机构号	交易柜员
                授权柜员	柜员流水号	原柜员流水号	基本帐户标记	现转标志	摘要代码
                传票子序号	客户帐号	客户帐号类型	客户帐号子序号	冲补标志	存折/支票户标志
                打印标志	序号	时间戳	记录状态
                 */

                Bill b = new Bill();
                string[] data = str.Split('\t');
                string sdate = String.Format("{0} {1:d6}", data[1], int.Parse(data[2]));
                b.date = DateTime.ParseExact(sdate, "yyyyMMdd HHmmss"
                        , System.Globalization.CultureInfo.CurrentCulture);
                b.acct = data[0].Trim();
                b.name = "";
                b.isOut = (data[9].Trim() == "0");
                b.to_acct = data[5].Trim();
                b.to_name = data[6].Trim();
                b.amount = double.Parse(data[3].Trim());
                b.balance = double.Parse(data[4].Trim());
                b.comment = data[19].Trim();
                return b;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static List<Bill> readFile(string fn, int skipRows, Str2Bill toBill)
        {
            List<Bill> billList = new List<Bill>();
            FileStream fs = new FileStream(fn, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            int irow = 0;
            while (!sr.EndOfStream)
            {
                string sLine = sr.ReadLine();
                irow++;

                if (irow > skipRows)
                {
                    Bill b = toBill(sLine);
                    if (b != null)
                    {
                        b.id = irow - skipRows;
                        billList.Add(b);

                    }
                }
            }
            sr.Close();
            fs.Close();
            return billList;
        }

        private static bool test()
        {
            util.Combination c = new util.Combination(5, 4, true, true);
            for (int i = 0; i < c.Count; i++)
            // int i = 0;
            // foreach (int[] a in c)
            {
                // i++;
                Console.WriteLine($"{i}:{util.StringUtil.Array2Str(c[i])}");
            }
            // if (c.Count > 0)
            return true;

        }



        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            util.Logger logger = new util.Logger(logLevel: System.Diagnostics.SourceLevels.Information, log2File: true);

            IEnumerable<Bill> inBills, outBills;
            util.UsedTime ut = new util.UsedTime();

            int totalMatched = 0;
            long totalMatchCount = 0;
            void showMatchResult(List<Match> result, int matchCount, int listCount, int iLevel)
            {
                totalMatchCount += matchCount;

                if (result.Count > 0)
                {
                    totalMatched += result[0].toMatchBills.Count;
                    logger.verbose(result[0]);
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{DateTime.Now:HH:mm:ss} {totalMatched,6}/{totalMatchCount,-10} "
                            + $" c({listCount,-3},{iLevel}) = {matchCount,-8}");
                Console.SetCursorPosition(0, Console.CursorTop);
            };

            void doMatch(string name, MatchHandel doit, double maxDeviation, int maxDateRange, int maxLevel)
            {
                ut.Add(name);
                logger.info($"{name} maxDeviation:{maxDeviation} maxDateRange:{maxDateRange} maxLevel:{maxLevel}");
                int matched = doit(inBills, outBills, maxDeviation, maxDateRange, maxLevel);
                logger.info($"{name} used time:{ut.GetElapse(name).TotalSeconds:f3} matched:{matched}");
            }

            Match.afterMatch = showMatchResult;
            logger.info($"read file");
            // List<Bill> billList = readFile("data\\对公账户-新银基.txt", 2, str2Bill1);
            // List<Bill> billList = readFile("data\\对公账户-驰诚.txt", 2, str2Bill1);
            List<Bill> billList = readFile("data\\对公账户-中和锐.txt", 2, str2Bill2);
            inBills = billList.Where(x => x.isOut == false && x.matchid == null && x.amount > 5000)
                                .OrderBy(x => x.date).ThenBy(x => x.id);
            outBills = billList.Where(x => x.isOut == true && x.matchid == null)
                                .OrderBy(x => x.date).ThenBy(x => x.id);
            logger.info($"inBills:{inBills.Count()} outBills:{outBills.Count()}");

            doMatch("1vM", Analyze.Match_1vM, 0, 3, 5);
            doMatch("Mv1", Analyze.Match_Mv1, 0, 3, 5);
            doMatch("MvM", Analyze.Match_MvM, 0.001, 3, 5);
            logger.info($"total matched:{totalMatched} used time:{ut.GetElapse().TotalSeconds:f3} match count:{totalMatchCount} speed:{totalMatchCount / ut.GetElapse().TotalSeconds:f2}/s");
        }
    }
}
