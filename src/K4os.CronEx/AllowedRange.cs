using System;

namespace K4os.CronEx;

/// <summary>
/// Range expression, used mainly to concisely pass a pair of numbers: [min,max].
/// Note: both bound are inclusive.
/// </summary>
/// <param name="Min">Minimum value (inclusive)</param>
/// <param name="Max">Maximum value (inclusive)</param>
public record AllowedRange(int Min, int Max)
{
	/// <summary>Checks if range contains given value.</summary>
	/// <param name="value">Tested value.</param>
	/// <returns><c>true</c> if given value is within range, <c>false</c> otherwise.</returns>
	public bool Contains(int value) => value >= Min && value <= Max;

	/// <inheritdoc />
	public override string ToString() => $"[{Min},{Max}]";
}