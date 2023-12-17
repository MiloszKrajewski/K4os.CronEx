namespace K4os.CronEx.Internals;

internal class FreqRotor: Rotor
{
	private readonly ulong _mask;
	private int _index;

	public override int Value => _index;

	public FreqRotor(FreqSpec[] specs)
	{
		_mask = ToFreqMask(specs);
		if (_mask == 0)
			throw new ArgumentException("No valid rotor values", nameof(specs));

		_index = Next(_mask, -1);
	}

	public static ulong ToFreqMask(FreqSpec[] specs) =>
		specs.Aggregate(0UL, (current, f) => current | f.ToUInt64());

	private static int Next(ulong value, int index) =>
		value == 0 ? -1 : BitOps.Shr(value, index + 1) switch {
			0 => BitOps.Ctz(value),
			var v => BitOps.Ctz(v) + index + 1,
		};

	public override bool Inc()
	{
		var before = _index;
		var after = _index = Next(_mask, before);
		return after <= before;
	}

	public override bool Reset(int value)
	{
		var after = _index = Next(_mask, value - 1);
		return after > value;
	}

	public override bool Check(int value) =>
		(_mask & (1UL << value)) != 0;
}
