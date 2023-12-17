#if NET5_0_OR_GREATER

using System.Numerics;
using System.Runtime.CompilerServices;

#endif

namespace K4os.CronEx.Internals;

/// <summary>
/// Polyfill for .NET 5+ BitOperations.
/// </summary>
internal class BitOps
{
	/// <summary>
	/// Shifts ulong right by count bits. Handles count being being greater than 63 by returning 0,
	/// instead of undefined behavior (doing nothing to be precise). 
	/// </summary>
	/// <param name="value">Value to be shifter.</param>
	/// <param name="count">Number os bits.</param>
	/// <returns>Shifted number.</returns>
	public static ulong Shr(ulong value, int count) =>
		count >= sizeof(ulong) * 8 ? 0 : value >> count;

	#if NET5_0_OR_GREATER

	/// <summary>Counts trailing zeros in ulong.</summary>
	/// <param name="value">Value.</param>
	/// <returns>Number of trailing zeroes.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Ctz(ulong value) =>
		BitOperations.TrailingZeroCount(value);

	#else

	/// <summary>Counts trailing zeros in ulong.</summary>
	/// <param name="value">Value.</param>
	/// <returns>Number of trailing zeroes.</returns>
	public static int Ctz(ulong value)
	{
		// This implementation is slow(ish) as it uses iterations,
		// but it's only used for .NET Framework
		
		if (value == 0) 
			return sizeof(ulong) * 8;

		value = (value ^ (value - 1)) >> 1;
		int ctz;
		for (ctz = 0; value != 0; ctz++) value >>= 1;
		return ctz;
	}
		
	#endif
}
