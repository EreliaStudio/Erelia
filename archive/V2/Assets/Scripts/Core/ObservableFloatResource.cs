using System;
using UnityEngine;

[Serializable]
public sealed class ObservableFloatResource : ObservableValue<ObservableFloatResource>
{
	private float current;
	private float max;

	public ObservableFloatResource()
	{
		Set(this, true);
	}

	public float Current => current;
	public float Max => max;
	public float Ratio => max > 0f ? current / max : 0f;

	public bool Set(float p_current, float p_max, bool p_forceNotify = false)
	{
		float targetMax = Mathf.Max(0f, p_max);
		float targetCurrent = Mathf.Clamp(p_current, 0f, targetMax);

		if (!p_forceNotify &&
			Mathf.Approximately(current, targetCurrent) &&
			Mathf.Approximately(max, targetMax))
		{
			return false;
		}

		current = targetCurrent;
		max = targetMax;
		Notify();
		return true;
	}

	public bool SetCurrent(float p_current, bool p_forceNotify = false)
	{
		return Set(p_current, max, p_forceNotify);
	}

	public bool SetMax(float p_max, bool p_resetCurrent = false, bool p_forceNotify = false)
	{
		float targetCurrent = p_resetCurrent ? Mathf.Max(0f, p_max) : current;
		return Set(targetCurrent, p_max, p_forceNotify);
	}

	public bool Change(float p_delta)
	{
		if (Mathf.Approximately(p_delta, 0f))
		{
			return false;
		}

		return Set(current + p_delta, max);
	}

	public bool Increase(float p_delta)
	{
		if (p_delta <= 0f || Mathf.Approximately(p_delta, 0f))
		{
			return false;
		}

		return Change(p_delta);
	}

	public bool Decrease(float p_delta)
	{
		if (p_delta <= 0f || Mathf.Approximately(p_delta, 0f))
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
