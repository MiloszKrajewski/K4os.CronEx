using System;
using K4os.CronEx.Internals;

namespace K4os.CronEx;

/// <summary>
/// Specification of whole crontab entry: "* * * * * command".
/// See https://en.wikipedia.org/wiki/Cron for detailed specification.
/// </summary>
public class CronSpec
{
	/// <summary>Parses crontab entry specification.</summary>
	/// <param name="text">Specification.</param>
	/// <returns>Parsed entry.</returns>
	/// <exception cref="ArgumentException">When entry is not valid.</exception>
	public static CronSpec Parse(string text) => 
		CronSpecParser.Parse(text);

	/// <summary>Specifications of minutes.</summary>
	public FreqSpec[] Minutes { get; init; } = Array.Empty<FreqSpec>();

	/// <summary>Specifications for hours.</summary>
	public FreqSpec[] Hours { get; init; } = Array.Empty<FreqSpec>();

	/// <summary>Specifications for days of month.</summary>
	public FreqSpec[] DaysOfMonth { get; init; } = Array.Empty<FreqSpec>();

	/// <summary>Specifications for months..</summary>
	public FreqSpec[] Months { get; init; } = Array.Empty<FreqSpec>();

	/// <summary>Specifications for days of week.</summary>
	public FreqSpec[] DaysOfWeek { get; init; } = Array.Empty<FreqSpec>();

	/// <summary>Command to be executed.</summary>
	public string Context { get; init; } = string.Empty;
}