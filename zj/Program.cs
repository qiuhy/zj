using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using util;

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
        private static Bill str2Bill5(String str)
        {
            try
            {
                /*
                账号	币种	卡号	交易时间戳	工作日期	借贷标志	发生额	余额	注释	对方帐户    ....
                 */

                Bill b = new Bill();
                string[] data = str.Substring(2).Split("\"\t\"\t");
                b.date = DateTime.ParseExact(data[3], "yyyy-MM-dd-HH.mm.ss.ffffff"
                        , System.Globalization.CultureInfo.CurrentCulture);
                b.acct = data[2];
                b.name = "";
                b.to_acct = data[9].Trim();
                b.to_name = "";
                b.isOut = data[5].StartsWith('借');
                b.amount = Double.Parse(data[6]);
                b.balance = Double.Parse(data[7]);
                b.comment = data[8];
                return b;
            }
            catch (System.Exception)
            {
                // Console.WriteLine($"{e} {str}");
                return null;
            }
        }
        private static Bill str2Bill6(String str)
        {
            try
            {
                /*
                序号	任务流水号	查询反馈结果	查询反馈结果原因	查询账号	查询卡号
                交易类型	借贷标志7	币种	交易金额9	交易余额10	交易时间11	交易流水号
                交易对方名称13	交易对方账号14	交易对方卡号	交易对方证件号码	交易对方余额	交易对方账号开户行
                交易摘要19	交易网点名称	交易网点代码	日志号	传票号	凭证种类	凭证号	现金标志
                终端号	交易是否成功	交易发生地	商户名称	商户号	IP地址	MAC地址	交易柜员号	备注35

                 */

                Bill b = new Bill();
                string[] data = str.Split("\t");
                b.date = DateTime.ParseExact(data[11], "yyyy-MM-dd HH:mm:ss"
                        , System.Globalization.CultureInfo.CurrentCulture);
                b.acct = "";
                b.name = "";
                b.to_acct = data[14].Trim();
                b.to_name = data[13].Trim();
                b.isOut = data[7].StartsWith('出');
                b.amount = Double.Parse(data[9]);
                b.balance = Double.Parse(data[10]);
                b.comment = data[6] + " " + data[19] + " " + data[35];
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
            Encoding encode = CommUtil.GetFileEncoding(fn);
            using (FileStream fs = new FileStream(fn, FileMode.Open))
            {
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
            }
            billList.Sort();
            return billList;
        }
        public static void saveResult(string fn, List<Bill> billList)
        {
            using (FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                foreach (var g in billList.GroupBy(x => x.matchid).OrderBy(x => x.Key))
                {
                    if (g.Key == 0)
                    {
                        sw.WriteLine($"nomatch:{g.Count()}");
                        foreach (Bill b in g.OrderBy(x => x.date).ThenBy(x => x.id))
                        {
                            sw.WriteLine(b);
                        }
                        sw.WriteLine();
                    }
                    else
                    {
                        Match m = new Match(g.Where(x => !x.isOut), g.Where(x => x.isOut));
                        sw.WriteLine(m);
                    }
                }
                sw.Close();
            }
        }
        public static int zjAnalyze(IEnumerable<Bill> billList)
        {
            int billListCount = billList.Count();
            if (billListCount == 0)
            {
                Console.WriteLine("无数据，退出");
                return 0;
            }
            Logger logger = new Logger(logLevel: System.Diagnostics.SourceLevels.Information);

            IEnumerable<Bill> inBills, outBills;
            UsedTime ut = new UsedTime();

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
            void doMatch(string name, double deviation, int dateRange, int inLevel, int outLevel)
            {
                curMatchCount = 0;
                ut.Add(name);
                int[] matched;
                // logger.info($"{name} maxDeviation:{maxDeviation} maxDateRange:{maxDateRange}");
                if ("Day".Equals(name))
                    matched = a.Match_Day(inBills, outBills, deviation, dateRange, inLevel, outLevel);
                else
                    matched = a.Match_MvM(inBills, outBills, deviation, dateRange, inLevel, outLevel);

                totalInMatched += matched[0];
                totalOutMatched += matched[1];
                totalMatchCount += curMatchCount;
                logger.info(strFormat, name
                        , matched[0] + matched[1]
                        , matched[0]
                        , matched[1]
                        , ut.GetElapse(name).TotalSeconds
                        , StringUtil.Number2Str(curMatchCount)
                        , StringUtil.Number2Str(curMatchCount / ut.GetElapse(name).TotalSeconds, 2));
            }

            logger.info("Start 开始");

            inBills = billList.Where(x => x.isOut == false)
                                .OrderBy(x => x.date).ThenBy(x => x.id)
                                .ToList();
            outBills = billList.Where(x => x.isOut == true)
                                .OrderBy(x => x.date).ThenBy(x => x.id)
                                .ToList();

            int inBillsCount = inBills.Count();
            int outBillsCount = outBills.Count();
            logger.info($"Bills:{billListCount}  ({inBillsCount} ,{outBillsCount})");

            double maxDeviation = 0.001;
            int maxDateRange = 2;
            // doMatch("5v5", 0.001, 1, 5, 5);
            doMatch("1v1", maxDeviation, maxDateRange, 1, 1);
            doMatch($"Day", maxDeviation, maxDateRange, 0, 0);
            for (int i = 2; i <= 5; i++)
            {
                doMatch($"1v{i}", maxDeviation, maxDateRange, 1, i);
                doMatch($"{i}v1", maxDeviation, maxDateRange, i, 1);
            }
            for (int i = 2; i <= 5; i++)
                for (int j = 2; j <= 5; j++)
                    doMatch($"{i}v{j}", maxDeviation, maxDateRange, i, j);

            logger.info(new String('-', 80));
            logger.info($"SUM match(%) {(double)(totalInMatched + totalOutMatched) / (inBillsCount + outBillsCount),6:p2}"
                        + $"({(double)totalInMatched / inBillsCount:p2}, {(double)totalOutMatched / outBillsCount:p2})");
            logger.info(strFormat, "SUM"
                        , totalInMatched + totalOutMatched
                        , totalInMatched
                        , totalOutMatched
                        , ut.GetElapse().TotalSeconds
                        , StringUtil.Number2Str(totalMatchCount)
                        , StringUtil.Number2Str(totalMatchCount / ut.GetElapse().TotalSeconds, 2));


            return totalInMatched + totalOutMatched;
        }

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Default;
            // List<Bill> billList = readFile("data\\对公账户-新银基.txt", 2, str2Bill1);
            // List<Bill> billList = readFile("data\\对公账户-达隆.txt", 2, str2Bill1);
            // List<Bill> billList = readFile("data\\对公账户-驰诚.txt", 2, str2Bill1);
            // List<Bill> billList = readFile("data\\对公账户-银锐.txt", 2, str2Bill2);
            // List<Bill> billList = readFile("data\\对公账户-中和锐.txt", 2, str2Bill2);
            // List<Bill> billList = readFile("data\\富中宝（建设银行）.txt", 6, str2Bill3);
            // List<Bill> billList = readFile("data\\富中宝贵客户对帐单（江苏银行）.txt", 1, str2Bill4);
            // List<Bill> billList = readFile("data\\工行-直属8027.txt", 1, str2Bill5);
            List<Bill> billList = readFile("data\\无锡金富汇金属制品有限公司.txt", 1, str2Bill6);
            zjAnalyze(billList.Where(x => x.matchid == 0 && x.amount >= 100));
            zjAnalyze(Analyze.Merge_Bill(billList.Where(x => x.matchid == 0 && x.amount >= 100)));

            saveResult($"data\\result_{DateTime.Now.Ticks}.txt", billList);
        }
    }
}
