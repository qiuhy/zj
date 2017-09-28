using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace zj
{
    class Program
    {
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
        private static Bill str2Bill3(String str)
        {
            double toDouble(string s)
            {
                return Double.Parse(s.Replace("\"", String.Empty).Replace(",", String.Empty));
            }
            try
            {
                /*
                交易日期	交易时间	活存账户明细号	摘要描述	借方发生额	贷方发生额	账户余额
                交易机构号	对方账号	交易机构名称	对方户名	对方行名	柜员号	交易流水号	交易渠道	扩充备注
                 */

                Bill b = new Bill();
                string[] data = str.Split('\t');
                if ("".Equals(data[1]))
                    data[1] = "00:00:00";
                string sdate = $"{data[0]} {data[1]}";
                b.date = DateTime.ParseExact(sdate, "yyyy-MM-dd HH:mm:ss"
                        , System.Globalization.CultureInfo.CurrentCulture);
                b.acct = "";
                b.name = "";
                b.to_acct = data[8].Trim();
                b.to_name = data[10].Trim();
                double amount1 = toDouble(data[4]);
                double amount2 = toDouble(data[5]);
                b.isOut = (amount1 != 0);
                b.amount = amount1 + amount2;
                b.balance = toDouble(data[6]);
                b.comment = data[14].Trim() + " " + data[15].Trim();
                return b;
            }
            catch (System.Exception)
            {
                // Console.WriteLine($"{e} {str}");
                return null;
            }
        }
        private static Bill str2Bill4(String str)
        {
            double toDouble(string s)
            {
                return Double.Parse(s.Replace("\"", String.Empty).Replace(",", String.Empty));
            }
            try
            {
                /*
                交易日期	借贷标记	交易金额	帐户余额	交易时间	对方帐号	对方名称
                 */

                Bill b = new Bill();
                string[] data = str.Split('\t');
                string sdate = $"{data[0]} {data[4]:D6}";
                b.date = DateTime.ParseExact(sdate, "yyyyMMdd HHmmss"
                        , System.Globalization.CultureInfo.CurrentCulture);
                b.acct = "";
                b.name = "";
                b.to_acct = data[5].Trim();
                b.to_name = data[6].Trim();
                b.isOut = data[1].StartsWith('0');
                b.amount = toDouble(data[2]);
                b.balance = toDouble(data[3]);
                b.comment = "";
                return b;
            }
            catch (System.Exception)
            {
                // Console.WriteLine($"{e} {str}");
                return null;
            }
        }

        public static List<Bill> readFile(string fn, int skipRows, Str2Bill toBill)
        {
            List<Bill> billList = new List<Bill>();
            Encoding encode = util.util.GetFileEncoding(fn);
            FileStream fs = new FileStream(fn, FileMode.Open);
            StreamReader sr = new StreamReader(fs, encode);
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
                        if (b.id == 0)
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
            Console.OutputEncoding = Encoding.Default;
            util.Logger logger = new util.Logger(logLevel: System.Diagnostics.SourceLevels.Information, log2File: true);

            IEnumerable<Bill> inBills, outBills;
            util.UsedTime ut = new util.UsedTime();

            int totalInMatched = 0;
            int totalOutMatched = 0;
            long totalMatchCount = 0;
            long curMatchCount = 0;

            void showMatchResult(List<Match> result, long matchCount, double progress)
            {
                if (matchCount == 0)
                    return;
                curMatchCount += matchCount;
                if (result.Count > 0)
                {
                    logger.verbose(result[0]);
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{DateTime.Now:HH:mm:ss} {matchCount,10} {curMatchCount,12} {progress,7:p2}");
                Console.SetCursorPosition(0, Console.CursorTop);
            };

            Analyze a = new Analyze(showMatchResult);
            string strFormat = "{0} matched: {1,5} ({2,3},{3,3}) used time:{4,-7:f3} count:{5,-10} speed:{6,10:f2}/s";
            void doMatch(string name, double maxDeviation, int maxDateRange, int inLevel, int outLevel)
            {
                curMatchCount = 0;
                ut.Add(name);
                int[] matched;
                logger.info($"{name} maxDeviation:{maxDeviation} maxDateRange:{maxDateRange}");
                if ("Day".Equals(name))
                    matched = a.Match_Day(inBills, outBills, maxDeviation, maxDateRange, inLevel, outLevel);
                else
                    matched = a.Match_MvM(inBills, outBills, maxDeviation, maxDateRange, inLevel, outLevel);

                totalInMatched += matched[0];
                totalOutMatched += matched[1];
                totalMatchCount += curMatchCount;
                logger.info(strFormat, name
                        , matched[0] + matched[1]
                        , matched[0]
                        , matched[1]
                        , ut.GetElapse(name).TotalSeconds
                        , curMatchCount
                        , curMatchCount / ut.GetElapse(name).TotalSeconds);
            }


            logger.info("Start 开始");
            // List<Bill> billList = readFile("data\\对公账户-新银基.txt", 2, str2Bill1);
            // List<Bill> billList = readFile("data\\对公账户-驰诚.txt", 2, str2Bill1);
            // List<Bill> billList = readFile("data\\对公账户-中和锐.txt", 2, str2Bill2);
            // List<Bill> billList = readFile("data\\富中宝（建设银行）.txt", 6, str2Bill3);
            List<Bill> billList = readFile("data\\富中宝贵客户对帐单（江苏银行）.txt", 1, str2Bill4);

            inBills = billList.Where(x => x.isOut == false && x.matchid == 0 && x.amount >= 100)
                                .OrderBy(x => x.date).ThenBy(x => x.id);
            outBills = billList.Where(x => x.isOut == true && x.matchid == 0 && x.amount >= 100)
                                .OrderBy(x => x.date).ThenBy(x => x.id);

            int inBillsCount = inBills.Count();
            int outBillsCount = outBills.Count();

            logger.info($"Bills:{billList.Count}  ({inBillsCount} ,{outBillsCount})");

            doMatch("1v1", 0.001, 1, 1, 1);
            doMatch($"Day", 0.001, 1, 0, 0);
            for (int i = 2; i <= 5; i++)
            {
                doMatch($"1v{i}", 0.001, 1, 1, i);
                doMatch($"{i}v1", 0.001, 1, i, 1);
            }
            for (int i = 2; i <= 5; i++)
                for (int j = 2; j <= 5; j++)
                    doMatch($"{i}v{j}", 0.001, 1, i, j);

            logger.info(new String('-', 80));
            logger.info($"SUM match(%) {(double)(totalInMatched + totalOutMatched) / (inBillsCount + outBillsCount),6:p2}"
                        + $"({(double)totalInMatched / inBillsCount:p2}, {(double)totalOutMatched / outBillsCount:p2})");
            logger.info(strFormat, "SUM"
                        , totalInMatched + totalOutMatched
                        , totalInMatched
                        , totalOutMatched
                        , ut.GetElapse().TotalSeconds
                        , totalMatchCount
                        , totalMatchCount / ut.GetElapse().TotalSeconds);
        }
    }
}
