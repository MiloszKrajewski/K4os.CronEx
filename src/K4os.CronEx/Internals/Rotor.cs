namespace K4os.CronEx.Internals;

internal abstract class Rotor
{
	public abstract int Value { get; }

	public abstract bool Inc();
	public abstract bool Reset(int value);
	public abstract bool Check(int value);
}
