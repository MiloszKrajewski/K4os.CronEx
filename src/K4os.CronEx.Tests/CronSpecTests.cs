using System;
using Xunit;

namespace K4os.CronEx.Tests
{
	public class CronSpecTests
	{
		[Fact]
		public void CronParseParsesAllParts()
		{
			var spec = CronSpec.Parse("1 2 3 4 5 command");
			Assert.Equal(new[] { 1 }, spec.Minutes.Enumerate());
			Assert.Equal(new[] { 2 }, spec.Hours.Enumerate());
			Assert.Equal(new[] { 3 }, spec.DaysOfMonth.Enumerate());
			Assert.Equal(new[] { 4 }, spec.Months.Enumerate());
			Assert.Equal(new[] { 5 }, spec.DaysOfWeek.Enumerate());
			Assert.Equal("command", spec.Context);
		}
		
		[Fact]
		public void CommandCanContainsSpaces()
		{
			var spec = CronSpec.Parse("1 2 3 4 5 command can contain spaces");
			Assert.Equal("command can contain spaces", spec.Context);
		}
		
		[Fact]
		public void MultipleSpacesAndTagsAreAllowedAsSeparators()
		{
			var spec = CronSpec.Parse("1\t2 \t \t 3\t\t\t4 \t5\tcommand");
			Assert.Equal(new[] { 1 }, spec.Minutes.Enumerate());
			Assert.Equal(new[] { 2 }, spec.Hours.Enumerate());
			Assert.Equal(new[] { 3 }, spec.DaysOfMonth.Enumerate());
			Assert.Equal(new[] { 4 }, spec.Months.Enumerate());
			Assert.Equal(new[] { 5 }, spec.DaysOfWeek.Enumerate());
			Assert.Equal("command", spec.Context);
		}
		
		[Fact]
		public void ParsingRequiresAllParts()
		{
			Assert.Throws<ArgumentException>(() => CronSpec.Parse("1\t2 \t \t 3\t\t\t4 \t"));
		}
		
		[Fact]
		public void AllowedRangesAreValid()
		{
			var spec = CronSpec.Parse("* * * * * command");

			void AssertRange(FreqSpec[] specs, int min, int max, int nth)
			{
				Assert.Single(specs);
				Assert.Equal(min, specs[0].Min);
				Assert.Equal(max, specs[0].Max);
				Assert.Equal(nth, specs[0].Nth);
			}
			
			AssertRange(spec.Minutes, 0, 59, 1);
			AssertRange(spec.Hours, 0, 23, 1);
			AssertRange(spec.DaysOfMonth, 1, 31, 1);
			AssertRange(spec.Months, 1, 12, 1);
			AssertRange(spec.DaysOfWeek, 0, 6, 1);
		}
		
		[Theory]
		[InlineData("! 2 3 4 5")]
		[InlineData("1 2 3 4 !")]
		public void AllPartsNeedToBeValid(string invalid)
		{
			Assert.Throws<ArgumentException>(() => CronSpec.Parse($"{invalid} command"));
		}
		
//		[Fact]
//		public void RenderingCronSpecDoesNotCrash()
//		{
//			var spec = CronSpec.Parse("1 2 3 4 5 command");
//			var text = spec.Render();
//			Assert.False(string.IsNullOrWhiteSpace(text));
//		}
	}
}
