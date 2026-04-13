using System;
using System.Collections;
using System.Collections.Generic;

public sealed class ObservableList<T> : IReadOnlyList<ObservableValue<T>>
{
	private readonly List<ObservableValue<T>> items = new List<ObservableValue<T>>();

	public event Action<ObservableList<T>> Changed;

	public int Count => items.Count;

	public ObservableValue<T> this[int p_index] => items[p_index];

	public ObservableValue<T> Add(T p_value)
	{
		ObservableValue<T> entry = new ObservableValue<T>(p_value);
		items.Add(entry);
		NotifyChanged();
		return entry;
	}

	public void SetItems(IEnumerable<T> p_values)
	{
		items.Clear();

		if (p_values != null)
		{
			foreach (T value in p_values)
			{
				items.Add(new ObservableValue<T>(value));
			}
		}

		NotifyChanged();
	}

	public void Clear()
	{
		if (items.Count == 0)
		{
			return;
		}

		items.Clear();
		NotifyChanged();
	}

	public void RemoveAt(int p_index)
	{
		items.RemoveAt(p_index);
		NotifyChanged();
	}

	public int RemoveAll(Predicate<T> p_match)
	{
		int removedCount = 0;

		for (int index = items.Count - 1; index >= 0; index--)
		{
			if (!p_match(items[index].Value))
			{
				continue;
			}

			items.RemoveAt(index);
			removedCount++;
		}

		if (removedCount > 0)
		{
			NotifyChanged();
		}

		return removedCount;
	}

	public bool NotifyItemChanged(T p_value)
	{
		int itemIndex = FindIndex(p_value);
		if (itemIndex < 0)
		{
			return false;
		}

		NotifyItemChangedAt(itemIndex);
		return true;
	}

	public void NotifyItemChangedAt(int p_index)
	{
		items[p_index].Set(items[p_index].Value, true);
	}

	public int FindIndex(T p_value)
	{
		for (int index = 0; index < items.Count; index++)
		{
			if (AreSameItem(items[index].Value, p_value))
			{
				return index;
			}
		}

		return -1;
	}

	public IEnumerator<ObservableValue<T>> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void NotifyChanged()
	{
		Changed?.Invoke(this);
	}

	private static bool AreSameItem(T p_left, T p_right)
	{
		if (!typeof(T).IsValueType)
		{
			return ReferenceEquals(p_left, p_right);
		}

		return EqualityComparer<T>.Default.Equals(p_left, p_right);
	}
}
