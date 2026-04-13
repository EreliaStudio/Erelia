using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObservableValue<T>
{
	private T value;

	public ObservableValue()
	{
	}

	public ObservableValue(T p_value)
	{
		value = p_value;
	}

	public T Value => value;

	public event Action<T> Changed;

	public bool Set(T p_value, bool p_forceNotify = false)
	{
		if (!p_forceNotify && EqualityComparer<T>.Default.Equals(value, p_value))
		{
			return false;
		}

		value = p_value;
		Changed?.Invoke(value);
		return true;
	}

	public bool Increase(T p_delta)
	{
		if (!IsPositive(p_delta))
		{
			return false;
		}

		if (!TryAdd(value, p_delta, out T result))
		{
			throw new InvalidOperationException($"ObservableValue<{typeof(T).Name}> only supports Increase for numeric values.");
		}

		return Set(result);
	}

	public bool Decrease(T p_delta)
	{
		if (!IsPositive(p_delta))
		{
			return false;
		}

		if (!TryNegate(p_delta, out T negativeDelta) || !TryAdd(value, negativeDelta, out T result))
		{
			throw new InvalidOperationException($"ObservableValue<{typeof(T).Name}> only supports Decrease for numeric values.");
		}

		return Set(result);
	}

	public void Notify()
	{
		Changed?.Invoke(value);
	}

	private static bool TryAdd(T p_left, T p_right, out T p_result)
	{
		if (typeof(T) == typeof(int))
		{
			p_result = (T)(object)((int)(object)p_left + (int)(object)p_right);
			return true;
		}

		if (typeof(T) == typeof(float))
		{
			p_result = (T)(object)((float)(object)p_left + (float)(object)p_right);
			return true;
		}

		if (typeof(T) == typeof(double))
		{
			p_result = (T)(object)((double)(object)p_left + (double)(object)p_right);
			return true;
		}

		if (typeof(T) == typeof(long))
		{
			p_result = (T)(object)((long)(object)p_left + (long)(object)p_right);
			return true;
		}

		if (typeof(T) == typeof(decimal))
		{
			p_result = (T)(object)((decimal)(object)p_left + (decimal)(object)p_right);
			return true;
		}

		p_result = default;
		return false;
	}

	private static bool TryNegate(T p_value, out T p_result)
	{
		if (typeof(T) == typeof(int))
		{
			p_result = (T)(object)(-(int)(object)p_value);
			return true;
		}

		if (typeof(T) == typeof(float))
		{
			p_result = (T)(object)(-(float)(object)p_value);
			return true;
		}

		if (typeof(T) == typeof(double))
		{
			p_result = (T)(object)(-(double)(object)p_value);
			return true;
		}

		if (typeof(T) == typeof(long))
		{
			p_result = (T)(object)(-(long)(object)p_value);
			return true;
		}

		if (typeof(T) == typeof(decimal))
		{
			p_result = (T)(object)(-(decimal)(object)p_value);
			return true;
		}

		p_result = default;
		return false;
	}

	private static bool IsPositive(T p_value)
	{
		if (typeof(T) == typeof(int))
		{
			return (int)(object)p_value > 0;
		}

		if (typeof(T) == typeof(float))
		{
			return (float)(object)p_value > 0f;
		}

		if (typeof(T) == typeof(double))
		{
			return (double)(object)p_value > 0d;
		}

		if (typeof(T) == typeof(long))
		{
			return (long)(object)p_value > 0L;
		}

		if (typeof(T) == typeof(decimal))
		{
			return (decimal)(object)p_value > 0m;
		}

		return false;
	}

	public abstract class Listener : MonoBehaviour
	{
		private ObservableValue<T> source;

		public void SubscribeTo(ObservableValue<T> p_source)
		{
			if (ReferenceEquals(source, p_source))
			{
				if (source == null)
				{
					ClearRenderedValue();
					return;
				}

				ReactToEdition(source.Value);
				return;
			}

			if (source != null)
			{
				source.Changed -= ReactToEdition;
			}

			source = p_source;

			if (source == null)
			{
				ClearRenderedValue();
				return;
			}

			source.Changed += ReactToEdition;
			ReactToEdition(source.Value);
		}

		public void ClearBinding()
		{
			SubscribeTo(null);
		}

		protected virtual void OnDestroy()
		{
			if (source != null)
			{
				source.Changed -= ReactToEdition;
				source = null;
			}
		}

		protected virtual void ClearRenderedValue()
		{
		}

		protected abstract void ReactToEdition(T p_value);
	}
}
