using System;
using System.Linq;
using K4os.CronEx.Internals;

namespace K4os.CronEx;

/// <summary>
/// Responsible for encapsulating frequency specification.
/// All cron specs can be represented as range with step, a-b/n
/// for example: 7 = 7-7/1, * = min-max/1, 5/5 = 5-max/5, etc.
/// </summary>
public class FreqSpec
{
	/// <summary>Parses list of expressions separated by comma (",").</summary>
	/// <param name="specs">List (or 1) of expressions.</param>
	/// <param name="parser">Parser for single expression.</param>
	/// <returns>Array of parsed expressions.</returns>
	public static FreqSpec[] ParseMany(string specs, Func<string, FreqSpec> parser) =>
		specs.Split(',').Select(parser).ToArray();

	/// <summary>Parses list of expressions separated by comma (",").</summary>
	/// <param name="specs">List (or 1) of expressions.</param>
	/// <param name="range">Allowed range for given expression.
	/// Range is used to expand '*' and validate expression's correctness.</param>
	/// <returns>Array of parsed expressions.</returns>
	public static FreqSpec[] ParseMany(string specs, AllowedRange range) =>
		ParseMany(specs, s => Parse(s, range));
		
	/// <summary>Parses single expression frequency expression.</summary>
	/// <param name="spec">Expression to parse.</param>
	/// <param name="range">Allowed range for given expression.
	/// Range is used to expand '*' and validate expression's correctness.</param>
	/// <returns>Array of parsed expressions.</returns>
	public static FreqSpec Parse(string spec, AllowedRange range) =>
		FreqSpecParser.Parse(spec, range);
		
	/// <summary>Range's lower bound. "a" in "a-b/n".</summary>
	public int Min { get; }

	/// <summary>Range's upper bound. "b" in "a-b/n".</summary>
	public int Max { get; }

	/// <summary>Range's step. "n" in "a-b/n".</summary>
	public int Nth { get; }
		
	/// <summary>
	/// Range of allowed values. It is used to expand '*' and validate expression's correctness.
	/// </summary>
	public AllowedRange Range { get; }

	/// <summary>Constructor. Note: values are not validated. Use <see cref="IsValid"/>
	/// if you need to validate specification.</summary>
	public FreqSpec(int min, int max, int nth, AllowedRange range) =>
		(Min, Max, Nth, Range) = (min, max, nth, range);

	/// <summary>Validates frequency specification against allowed range.</summary>
	/// <param name="range">Allowed range (as in: 0-59 for minutes).</param>
	/// <returns><c>true</c> is specification is valid; <c>false</c> otherwise.</returns>
	public bool IsValid(AllowedRange range) =>
		Min >= 0 && Max >= Min && range.Contains(Min) && range.Contains(Max) && Nth > 0;

	/// <summary>Enumerates all values produced by specification a-b/n which can be read as
	/// "a to b (inclusive) step n", for example: 1-5/2 will produce 1, 3, 5.</summary>
	/// <returns>Sequence of produced values.</returns>
	public IEnumerable<int> Enumerate()
	{
		for (var i = Min; i <= Max; i += Nth)
			yield return i;
	}
	
	/// <summary>Enumerates all values produced by specification a-b/n which can be read as
	/// "a to b (inclusive) step n", for example: 1-5/2 will produce 1, 3, 5.</summary>
	/// <returns>Sequence of produced values.</returns>
	public ulong ToUInt64()
	{
		if (Max > 63)
			throw new ArgumentOutOfRangeException(nameof(Max), "Max value is too big");

		var mask = 0UL;
		
		for (var i = Min; i <= Max; i += Nth)
			mask |= 1UL << i;

		return mask;
	}
}