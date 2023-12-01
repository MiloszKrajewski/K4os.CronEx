using System;
using System.Linq;
using Xunit;

namespace K4os.CronEx.Tests;

public class FreqSpecTests
{
	private static readonly AllowedRange Everything = new(0, int.MaxValue);
	private static readonly AllowedRange OneToFive = new(1, 5);
	private static readonly AllowedRange ZeroToTen = new(0, 10);

	[Fact]
	public void ParsingFailsWhenExpressionIsNull()
	{
		Assert.Throws<ArgumentNullException>(() => FreqSpec.Parse(null!, Everything));
	}

	[Theory]
	[InlineData(""), InlineData("garbage"), InlineData("7a"), InlineData("a7")]
	[InlineData("3-8-10"), InlineData("3-8/5/5")]
	[InlineData("-3/5"), InlineData("3-/5"), InlineData("3/-5")]
	public void ParsingFailsWhenExpressionIsNotValid(string text)
	{
		Assert.Throws<ArgumentException>(() => FreqSpec.Parse(text, Everything));
	}

	[Theory]
	[InlineData("0-3"), InlineData("3-6"), InlineData("5-1")]
	public void ParsingFailsWhenExpressionIsNotInRange(string text)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => FreqSpec.Parse(text, OneToFive));
	}

	[Theory]
	[InlineData("*", 0, 10, 1)]
	[InlineData("*/2", 0, 10, 2)]
	[InlineData("3", 3, 3, 1)]
	[InlineData("3/2", 3, 10, 2)]
	[InlineData("3-9", 3, 9, 1)]
	[InlineData("3-9/3", 3, 9, 3)]
	public void ParsingReducesTextToRangeExpression(string text, int min, int max, int nth)
	{
		var spec = FreqSpec.Parse(text, ZeroToTen);
		Assert.Equal(min, spec.Min);
		Assert.Equal(max, spec.Max);
		Assert.Equal(nth, spec.Nth);
	}

	[Theory]
	[InlineData("*", "0-10/1")]
	[InlineData("*/2", "0-10/2")]
	[InlineData("3", "3-3/1")]
	[InlineData("3/2", "3-10/2")]
	[InlineData("3-9", "3-9/1")]
	[InlineData("3-9/3", "3-9/3")]
	public void EveryExpressionIsARange(string textA, string textB)
	{
		var specA = FreqSpec.Parse(textA, ZeroToTen);
		var specB = FreqSpec.Parse(textB, ZeroToTen);

		Assert.Equal(specA.Min, specB.Min);
		Assert.Equal(specA.Max, specB.Max);
		Assert.Equal(specA.Nth, specB.Nth);
	}

	[Theory]
	[InlineData("*", new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
	[InlineData("*/2", new[] { 0, 2, 4, 6, 8, 10 })]
	[InlineData("3", new[] { 3 })]
	[InlineData("3/2", new[] { 3, 5, 7, 9 })]
	[InlineData("3-9", new[] { 3, 4, 5, 6, 7, 8, 9 })]
	[InlineData("3-9/3", new[] { 3, 6, 9 })]
	public void WhenEnumeratedExpressionReturnSequenceOfValues(string text, int[] expected)
	{
		var spec = FreqSpec.Parse(text, ZeroToTen);
		Assert.Equal(expected, spec.Enumerate());
	}

	[Theory]
	[InlineData("7-9/1", 7)]
	[InlineData("7-7", 7)]
	[InlineData("7", 7)]
	public void LowerBoundIsInclusive(string text, int expected)
	{
		var spec = FreqSpec.Parse(text, ZeroToTen);
		Assert.Equal(expected, spec.Enumerate().First());
	}

	[Theory]
	[InlineData("3-7/1", 7)]
	[InlineData("2-7/5", 7)]
	[InlineData("7-7", 7)]
	[InlineData("7", 7)]
	public void UpperBoundIsInclusive(string text, int expected)
	{
		var spec = FreqSpec.Parse(text, ZeroToTen);
		Assert.Equal(expected, spec.Enumerate().Last());
	}

	[Fact]
	public void ParseManyParsesManyExpressions()
	{
		var specs = FreqSpec.ParseMany("1,2,3", Everything);
		Assert.Equal(3, specs.Length);
		Assert.True(specs[0].Min == 1);
		Assert.True(specs[1].Min == 2);
		Assert.True(specs[2].Min == 3);
	}

	[Theory]
	[InlineData("1,2,3,"), InlineData(",1,2,3"), InlineData("1,,3")]
	public void ParseManyFailsIfListIsMalformed(string text)
	{
		Assert.Throws<ArgumentException>(() => FreqSpec.ParseMany(text, Everything));
	}

	[Fact]
	public void ParseManyHandlesComplexExpressions()
	{
		var specs = FreqSpec.ParseMany("1,2-8/7,3/1337,*/5", ZeroToTen);

		Assert.Equal(1, specs[0].Min);
		Assert.Equal(1, specs[0].Max);
		Assert.Equal(1, specs[0].Nth);

		Assert.Equal(2, specs[1].Min);
		Assert.Equal(8, specs[1].Max);
		Assert.Equal(7, specs[1].Nth);

		Assert.Equal(3, specs[2].Min);
		Assert.Equal(10, specs[2].Max);
		Assert.Equal(1337, specs[2].Nth);

		Assert.Equal(0, specs[3].Min);
		Assert.Equal(10, specs[3].Max);
		Assert.Equal(5, specs[3].Nth);
	}

	[Theory]
	[InlineData("1,1,1", new[] { 1 })]
	[InlineData("5,4,3,2,1", new[] { 1, 2, 3, 4, 5 })]
	[InlineData("8-9,1-5,3-7", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
	[InlineData("5,*/5", new[] { 0, 5, 10 })]
	public void EnumerateManyHandlesDuplicates(string text, int[] expected)
	{
		var specs = FreqSpec.ParseMany(text, ZeroToTen);
		Assert.Equal(expected, specs.Enumerate());
	}
}