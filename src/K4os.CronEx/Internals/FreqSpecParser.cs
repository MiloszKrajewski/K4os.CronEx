using System;
using System.Text.RegularExpressions;

namespace K4os.CronEx.Internals;

/// <summary>
/// Parser for frequency expression, like: * or 1-30/15. Please note: it does not
/// process lists (separated by ","), they should be handled on level above.
/// </summary>
public static class FreqSpecParser
{
	// I used https://cron.help/ to investigate actual behaviour and there seems to be
	// some grey area around invalid data like: x/0, 0-9999
	// Also numbers which are out of range seems to be accepted, for example 123-156/3 
	// (as days of month) happily prints 123rd, 126th, 129th, 132nd, 135th, 138th,
	// 141st, 144th, 147th, 150th, 153rd, and 156th
	// The question is: should it fail validation, not work at all, work "kind of"?
	// Then I've found https://crontab.guru/ which behaves much better and does not
	// accept invalid data. I guess telling "something is wrong" is better than
	// just silently accepting garbage.

	// Regular expression handles: *, */n, a, a-b, a/n, a-b/n 
	// static, compiled and explicit capture - performance
	private static readonly Regex Pattern = new(
		@"^\s*((?<all>\*)|((?<min>\d+)(-(?<max>\d+))?))(/(?<nth>\d+))?\s*$",
		RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		
	/// <summary>Parses single frequency expression in a form of:
	/// *, */n, a, a-b, a/n, or a-b/n</summary>
	/// <param name="text">Specification to parse.</param>
	/// <param name="range">Allowed range.</param>
	/// <returns>Parsed specification or throw exception.</returns>
	/// <exception cref="ArgumentNullException">Expression is null.</exception>
	/// <exception cref="ArgumentException">When expression is not valid.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Expression is is syntactically correct
	/// but not not within allowed range.</exception>
	public static FreqSpec Parse(string text, AllowedRange range)
	{
		var match = Pattern.Match(text);
		if (!match.Success)
			throw new ArgumentException($"Expression '{text}' is not valid");

		var min = match.TryParse("min", int.Parse);
		var max = match.TryParse("max", int.Parse);
		var nth = match.TryParse("nth", int.Parse);

		// slightly complex but this logic needs to be somewhere
		// we COULD construct different child classes of FreqDef (ie: SingleValueSpec,
		// RangeSpec, FullSpec) and let polymorphism do the job.
		// But at the same time they can be all modelled as "a-b/n" (yes, all of them)
		// for example: * = a-b/1, a = a-a/1, a/n = a-max/n, etc.
		// this could be reduce to 2 cases only, but this way (with explicit '*')
		// is cleaner and easier to reason about
		var spec =
			min is null ? StarSpec(range, nth) : // * or */n
			max is null && nth is null ? SingleSpec(range, min.Value) : // a
			RangeSpec(range, min.Value, max, nth); // a/n, a-b, or a-b/n

		if (!spec.IsValid(range))
			throw new ArgumentOutOfRangeException(
				$"Expression {text} is not valid for range {range}");

		return spec;
	}

	private static FreqSpec StarSpec(AllowedRange range, int? nth) =>
		new(range.Min, range.Max, nth ?? 1, range); // *

	private static FreqSpec SingleSpec(AllowedRange range, int value) =>
		new(value, value, 1, range); // a

	private static FreqSpec RangeSpec(AllowedRange range, int min, int? max, int? nth) =>
		new(min, max ?? range.Max, nth ?? 1, range); // a-b/n
}