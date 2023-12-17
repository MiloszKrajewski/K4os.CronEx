namespace K4os.CronEx.Internals;

internal class IntRotor: Rotor
{
	private int _value;
	public IntRotor(int value) { _value = value; }
	public override int Value => _value;

	public override bool Inc()
	{
		_value++;
		return false;
	}

	public override bool Reset(int value)
	{
		_value = value;
		return false;
	}

	public override bool Check(int value) => true;
}
