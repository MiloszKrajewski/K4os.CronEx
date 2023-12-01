using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace K4os.CronEx;

public static class Extensions
{
	/// <summary>Tries to parse named group of matched regular expression.</summary>
	/// <param name="match">Regular expression match.</param>
	/// <param name="name">Group name.</param>
	/// <param name="parse">Parse function.</param>
	/// <typeparam name="T">Parse-to type.</typeparam>
	/// <returns>Parsed value or <c>null</c> if group was not found.</returns>
	public static T? TryParse<T>(
		this Match match, string name, Func<string, T> parse) where T: struct =>
		match.Groups[name] switch {
			{ Success: true } g => parse(g.Value),
			_ => null,
		};
		
	/// <summary>
	/// Joins multiple strings into one using given separator.
	/// Uses library function <see cref="string.Join(string,IEnumerable{string})"/> but
	/// but uses extension method syntax rather than static method.
	/// </summary>
	/// <param name="strings">Sequence of strings.</param>
	/// <param name="separator">Separator.</param>
	/// <returns>Joined strings, separated by given separator.</returns>
	public static string Join(this IEnumerable<string> strings, string separator = "") =>
		string.Join(separator, strings);

	/// <summary>
	/// Enumerates all values expressed by specifications.
	/// All duplicates are removed and returned values are sorted.
	/// </summary>
	/// <param name="specs">Sequence of specifications.</param>
	/// <returns>Sequence of values.</returns>
	public static IEnumerable<int> Enumerate(this IEnumerable<FreqSpec> specs) =>
		specs.SelectMany(d => d.Enumerate()).Distinct().OrderBy(x => x);
}