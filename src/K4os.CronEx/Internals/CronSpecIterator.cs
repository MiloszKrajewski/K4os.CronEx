using System.Diagnostics.CodeAnalysis;

namespace K4os.CronEx.Internals;

/// <summary>Cron events iterator.</summary>
public class CronSpecIterator
{
	// seems that 40 is enough when requesting all wednesdays on 29th of February from beginning of time
	// so 100 gives a little bit of safety margin
	private const int MAX_ERROR_COUNT = 100;

	private readonly Rotor[] _rotors;
	private readonly ulong _dow;
	private DateTime? _current;

	private int Year => _rotors[4].Value;
	private int Month => _rotors[3].Value;
	private int DayOfMonth => _rotors[2].Value;
	private int Hour => _rotors[1].Value;
	private int Minute => _rotors[0].Value;

	/// <summary>Current event time.</summary>
	public DateTime? Current => _current;

	/// <summary>
	/// Creates new instance of <see cref="CronSpecIterator"/>.
	/// </summary>
	/// <param name="spec">Cron expression specification.</param>
	public CronSpecIterator(CronSpec spec)
	{
		_rotors = new Rotor[] {
			new FreqRotor(spec.Minutes), // 0
			new FreqRotor(spec.Hours), // 1
			new FreqRotor(spec.DaysOfMonth), // 2
			new FreqRotor(spec.Months), // 3
			new IntRotor(0), // 4
		};
		_dow = FreqRotor.ToFreqMask(spec.DaysOfWeek);
		_current = null;
	}

	/// <summary>Resets iterator to specified time, or greater.</summary>
	/// <param name="time">Initial time.</param>
	public void Reset(DateTime time)
	{
		var reset = false;
		reset = _rotors[4].Reset(reset ? 0 : time.Year) | reset;
		reset = _rotors[3].Reset(reset ? 0 : time.Month) | reset;
		reset = _rotors[2].Reset(reset ? 0 : time.Day) | reset;
		reset = _rotors[1].Reset(reset ? 0 : time.Hour) | reset;
		reset = _rotors[0].Reset(reset ? 0 : time.Minute) | reset;
		_ = reset;

		SkipInvalidDates();
		UpdateCurrentDate();
		UpdateAfterReset(time);
	}

	/// <summary>Moves to next event.</summary>
	public void Next()
	{
		Inc(0);
		SkipInvalidDates();
		UpdateCurrentDate();
	}

	/// <summary>Enumerates all dates matching this specification from specified start.</summary>
	/// <param name="spec">Cron expression.</param>
	/// <param name="start">Start date.</param>
	/// <param name="inclusive">If <c>false</c> then it explicitly considers dates only after <paramref name="start"/>.</param>
	/// <returns>A stream of dates.</returns>
	[SuppressMessage("ReSharper", "IteratorNeverReturns")]
	public static IEnumerable<DateTime> EnumerateFrom(
		CronSpec spec, DateTime start, bool inclusive = true)
	{
		var iterator = new CronSpecIterator(spec);
		iterator.Reset(start);
		while (true)
		{
			var current = iterator.Current;
			if (current is null)
				yield break;

			if (inclusive || current > start)
				yield return current.Value;

			iterator.Next();
		}
	}

	/// <summary>Enumerates next date matching this specification from specified start.</summary>
	/// <param name="spec">Cron expression.</param>
	/// <param name="start">Start date.</param>
	/// <param name="inclusive">If <c>false</c> then it explicitly considers dates only after <paramref name="start"/>.</param>
	/// <returns>Next date.</returns>
	public static DateTime? NextFrom(
		CronSpec spec, DateTime start, bool inclusive = true)
	{
		var iterator = new CronSpecIterator(spec);
		iterator.Reset(start);

		while (true)
		{
			var current = iterator.Current;
			if (inclusive || current > start) return current;

			iterator.Next();
		}
	}

	private void SkipInvalidDates()
	{
		var failed = 0;

		while (true)
		{
			if (Year is < 1 or > 9999) break;
			if (IsValidDate()) break;

			if (++failed > MAX_ERROR_COUNT)
				throw new ArgumentException(
					"Cron spec generates too many invalid dates");

			Inc(2);
		}
	}

	private void UpdateCurrentDate()
	{
		var year = Year;
		_current = year is < 1 or > 9999
			? null
			: new DateTime(year, Month, DayOfMonth, Hour, Minute, 0);
	}
	
	private void UpdateAfterReset(DateTime time)
	{
		while (_current < time) Next();
	}

	private void Inc(int index)
	{
		var length = _rotors.Length;
		for (var i = 0; i < length; i++)
		{
			if (i < index)
			{
				_rotors[i].Reset(0);
			}
			else
			{
				if (!_rotors[i].Inc())
					return;
			}
		}
	}

	private bool IsValidDate() =>
		IsValidDayOfMonth() &&
		IsValidDayOfWeek();

	private bool IsValidDayOfMonth() =>
		DayOfMonth is > 0 and <= 31 &&
		DayOfMonth <= DateTime.DaysInMonth(Year, Month);

	private bool IsValidDayOfWeek()
	{
		var date = new DateTime(Year, Month, DayOfMonth);
		return (_dow & (1uL << (int)date.DayOfWeek)) != 0;
	}
}
