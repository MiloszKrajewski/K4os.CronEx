# K4os.CronEx

[![NuGet Stats](https://img.shields.io/nuget/v/K4os.CronEx.svg)](https://www.nuget.org/packages/K4os.CronEx)

# Usage

This library consists of two components, cron expression parser and cron expression iterator.
Most likely you'll be interested mostly in the iterator, which provides methods to find next
occurrence(s) of events described by cron expression.

```csharp
var cron = CronSpec.Parse("* * * * *");
var next1 = cron.NextAfter(DateTime.Now);
var next9 = cron.EnumerateFrom(DateTime.Now).Take(9);
```

# Build

```shell
build
```
