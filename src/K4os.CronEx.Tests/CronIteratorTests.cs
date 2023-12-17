using K4os.CronEx.Internals;
using Xunit;

namespace K4os.CronEx.Tests;

public class CronIteratorTests
{
	private static CronSpecIterator IteratorFor(CronSpec parse) => new(parse);

	[Fact]
	public void CanCreateCronIterator()
	{
		var i = IteratorFor(CronSpec.Parse("* * * * *"));
		i.Reset(new DateTime(2021, 1, 1, 1, 2, 0));
		Assert.Equal(new DateTime(2021, 1, 1, 1, 2, 0), i.Current);
	}

	[Theory]
	[InlineData("* * * * *", "2021-01-10 14:15:00")]
	[InlineData("*/5 */2 */3 * *", "2021-01-10 14:15:00")]
	[InlineData("*/5 */2 */3 */3 *", "2021-04-10 14:15:00")]
	public void ResettingToExactDate(string expression, string datetime)
	{
		var i = IteratorFor(CronSpec.Parse(expression));
		var expected = DateTime.Parse(datetime);
		i.Reset(expected);
		Assert.Equal(expected, i.Current);
	}

	[Theory]
	[InlineData("* * * * *", "2021-01-10 14:15:15", "2021-01-10 14:16:00")]
	[InlineData("*/5 */2 */3 * *", "2021-01-10 14:10:15", "2021-01-10 14:15:00")]
	[InlineData("*/5 */2 */3 * *", "2021-01-10 14:11:15", "2021-01-10 14:15:00")]
	public void ResettingToALittleBitEarlier(string expression, string earlier, string later)
	{
		var i = IteratorFor(CronSpec.Parse(expression));
		var provided = DateTime.Parse(earlier);
		var expected = DateTime.Parse(later);
		i.Reset(provided);
		Assert.Equal(expected, i.Current);
	}

	[Theory]
	[InlineData("0 0 * * 3")]
	public void FilteringByDayOfWeekWorks(string expression)
	{
		var i = IteratorFor(CronSpec.Parse(expression));
		i.Reset(new DateTime(2021, 1, 1, 0, 0, 0));
		for (var n = 0; n < 1000; n++)
		{
			Assert.Equal(DayOfWeek.Wednesday, i.Current!.Value.DayOfWeek);
			i.Next();
		}
	}

	[Fact]
	public void AllWednesdaysOn30th()
	{
		var all = CronSpec.Parse("0 0 30 * 3").EnumerateFrom(DateTime.Parse("2021-01-01"))
			.Take(1000)
			.All(x => x is { DayOfWeek: DayOfWeek.Wednesday, Day: 30 });
		Assert.True(all);
	}

	[Fact]
	public void All30OfFebruary()
	{
		Assert.Throws<ArgumentException>(
			() => _ = CronSpec
				.Parse("0 0 30 2 *")
				.EnumerateFrom(DateTime.Parse("2021-01-01"))
				.Take(1000)
				.First());
	}

	[Fact]
	public void FindCornerCases()
	{
		for (var dow = 0; dow < 7; dow++)
		{
			var spec = $"0 0 29 2 {dow}";
			var count = CronSpec
				.Parse(spec)
				.EnumerateFrom(DateTime.MinValue)
				.Count();
			Assert.True(count > 0);
		}
	}

	[Fact]
	public void AllWednesdays()
	{
		var spec = "0 0 * * 3";
		var match = CronSpec
			.Parse(spec)
			.EnumerateFrom(DateTime.MinValue)
			.All(d => d.DayOfWeek == DayOfWeek.Wednesday);
		Assert.True(match);
	}
	
	[Fact]
	public void All29OfFebruary()
	{
		var spec = "0 0 29 2 *";
		var match = CronSpec
			.Parse(spec)
			.EnumerateFrom(DateTime.MinValue)
			.All(d => d is { Month: 2, Day: 29 });
		Assert.True(match);
	}
	
	[Fact]
	public void FiveInTheEveningEvery5MinutesSecondOfEachMonth()
	{
		var spec = "*/5 17 2 * *";
		var match = CronSpec
			.Parse(spec)
			.EnumerateFrom(DateTime.MinValue)
			.All(d => d is { Day: 2, Hour: 17 } && d.Minute % 5 == 0);
		Assert.True(match);
	}
}
