using System;

[Serializable]
public sealed class ObservableResource : ObservableValue<ObservableResource>
{
	private int current;
	private int max;

	public ObservableResource()
	{
		Set(this, true);
	}

	public int Current => current;
	public int Max => max;
	public float Ratio => max > 0 ? (float) current / max : 0f;

	public bool Set(int p_current, int p_max, bool p_forceNotify = false)
	{
		int targetMax = Math.Max(0, p_max);
		int targetCurrent = Math.Clamp(p_current, 0, targetMax);

		if (!p_forceNotify && current == targetCurrent && max == targetMax)
		{
			return false;
		}

		current = targetCurrent;
		max = targetMax;
		Notify();
		return true;
	}

	public bool SetCurrent(int p_current, bool p_forceNotify = false)
	{
		return Set(p_current, max, p_forceNotify);
	}

	public bool SetMax(int p_max, bool p_resetCurrent = false, bool p_forceNotify = false)
	{
		int targetCurrent = p_resetCurrent ? Math.Max(0, p_max) : current;
		return Set(targetCurrent, p_max, p_forceNotify);
	}

	public bool Change(int p_delta)
	{
		if (p_delta == 0)
		{
			return false;
		}

		return Set(current + p_delta, max);
	}

	public bool Increase(int p_delta)
	{
		if (p_delta <= 0)
		{
			return false;
		}

		return Change(p_delta);
	}

	public bool Decrease(int p_delta)
	{
		if (p_delta <= 0)
		{
			return false;
		}

		return Change(-p_delta);
	}

	public bool Reset()
	{
		return Set(max, max);
	}
}
