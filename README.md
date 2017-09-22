# 资金分析
使用以下三个方法进行分析,参数都相同

```csharp
//return 匹配中的数量
IEnumerable<Bill> inBills;  //希望匹配的
IEnumerable<Bill> outBills; //可用来匹配的
double maxDeviation;        //最大误差，可为0
int maxDateRange;           //日期范围
int maxLevel;               //匹配数量范围
int Analyze.Match_Mv1(inBills, outBills, maxDeviation,maxDateRange, maxLevel);
int Analyze.Match_1vM(inBills, outBills, maxDeviation,maxDateRange, maxLevel);
int Analyze.Match_MvM(inBills, outBills, maxDeviation,maxDateRange, maxLevel);
```
