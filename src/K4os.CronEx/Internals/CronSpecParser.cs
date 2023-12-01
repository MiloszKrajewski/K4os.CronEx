using System;

namespace K4os.CronEx.Internals;

/// <summary>Cron expression parser. Handles whole like: "1/15 0 1,15 * 1-5 /usr/bin/find".</summary>
public static class CronSpecParser
{
	// separators, possibly tabs and spaces
	private static readonly char[] Separators = { ' ', '\t' };
		
	private static readonly AllowedRange AllowedMinutes = new(0, 59);
	private static readonly AllowedRange AllowedHours = new(0, 23);
	private static readonly AllowedRange AllowedDaysOfMonth = new(1, 31);
	private static readonly AllowedRange AllowedMonths = new(1, 12);
	private static readonly AllowedRange AllowedDaysOfWeek = new(0, 6);

	#if NET5_0_OR_GREATER
	
	private const StringSplitOptions SplitOptions = 
		StringSplitOptions.RemoveEmptyEntries | 
		StringSplitOptions.TrimEntries;

	#else
	
	private const StringSplitOptions SplitOptions = 
		StringSplitOptions.RemoveEmptyEntries;
	
	#endif
	
	/// <summary>
	/// Parses crontab entry expression. Returns parsed expression or fails with
	/// <see cref="ArgumentException"/> if expression is invalid.
	/// </summary>
	/// <param name="text">Entry to be parsed.</param>
	/// <returns>Parsed expression.</returns>
	/// <exception cref="ArgumentException">Thrown when expression is not correct.</exception>
	public static CronSpec Parse(string text)
	{
		var parts = text.Split(Separators, 6, SplitOptions);
		if (parts.Length < 5)
			throw new ArgumentException("Given expression is not valid");

		return new CronSpec {
			Minutes = FreqSpec.ParseMany(parts[0], AllowedMinutes),
			Hours = FreqSpec.ParseMany(parts[1], AllowedHours),
			DaysOfMonth = FreqSpec.ParseMany(parts[2], AllowedDaysOfMonth),
			Months = FreqSpec.ParseMany(parts[3], AllowedMonths),
			DaysOfWeek = FreqSpec.ParseMany(parts[4], AllowedDaysOfWeek),
			Context = parts.Length < 6 ? string.Empty : parts[5],
		};
	}
}