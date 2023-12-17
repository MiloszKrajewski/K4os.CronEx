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

	/// <summary>
	/// Enumerates all moments in time matching this specification from specified start.
	/// It may or may not include start time, depending on <paramref name="inclusive"/>.
	/// </summary>
	/// <param name="start">Start time.</param>
	/// <param name="inclusive">if <c>false</c> then it is guaranteed that start date wont be included.</param>
	/// <returns>A stream timestamp.</returns>
	public IEnumerable<DateTime> EnumerateFrom(DateTime start, bool inclusive = true) => 
		CronSpecIterator.EnumerateFrom(this, start);
	
	/// <summary>
	/// Enumerates all moments in time matching this specification from specified start.
	/// It never includes start time.
	/// </summary>
	/// <param name="start">Start time.</param>
	public IEnumerable<DateTime> EnumerateAfter(DateTime start) => 
		EnumerateFrom(start, false);

	/// <summary>
	/// Gets first event time matching this specification from specified start.
	/// It may or may not return start time, depending on <paramref name="inclusive"/>.
	/// </summary>
	/// <param name="start">Start time.</param>
	/// <param name="inclusive">if <c>false</c> then it is guaranteed that start date wont be returned.</param>
	/// <returns>First matching time.</returns>
	public DateTime? NextFrom(DateTime start, bool inclusive = true) =>
		CronSpecIterator.NextFrom(this, start, inclusive);
	
	/// <summary>
	/// Gets first event time matching this specification from specified start.
	/// It will not return start time, it will be first event after.
	/// </summary>
	/// <param name="start">Start time.</param>
	/// <returns>First matching time.</returns>
	public IEnumerable<DateTime> NextAfter(DateTime start) => 
		EnumerateFrom(start, false);
}