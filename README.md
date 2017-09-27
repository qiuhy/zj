# 资金分析
统一使用以下方法进行分析

```csharp
//return int[]
//0:inBills 匹配中的数量,1:outBills 匹配中的数量
IEnumerable<Bill> inBills;  //希望匹配的
IEnumerable<Bill> outBills; //可用来匹配的
double maxDeviation;        //最大误差，可为0
int maxDateRange;           //日期范围
int maxLevel;               //匹配数量范围
int[] Analyze.Match_MvM(inBills, outBills, maxDeviation, maxDateRange, maxLevel);
```
