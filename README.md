# 资金分析

```csharp
//统一使用以下方法进行分析
int[] Analyze.Match_MvM(inBills, outBills, maxDeviation, maxDateRange, inLevel, outLevel);
int[] Analyze.Match_Day(inBills, outBills, maxDeviation, maxDateRange, inLevel, outLevel);

```
## 备注

添加System.Text.Encoding.CodePages的引用，来支持 Windows-1252, Shift-JIS, and GB2312 等 .Net Core 中不支持的编码

dotnet add package System.Text.Encoding.CodePages --version 4.4.0