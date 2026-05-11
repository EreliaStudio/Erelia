using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ReferenceRegistry
{
	private readonly Dictionary<string, ReferenceableScriptableObject> byGuid =
		new(StringComparer.Ordinal);

	public void Bind(IReadOnlyList<ReferenceableScriptableObject> p_entries)
	{
		if (p_entries == null)
		{
			return;
		}

		for (int index = 0; index < p_entries.Count; index++)
		{
			Register(p_entries[index]);
		}
	}

	public bool TryResolve<T>(string p_guid, out T p_result) where T : ReferenceableScriptableObject
	{
		p_result = null;

		if (string.IsNullOrEmpty(p_guid))
		{
			return false;
		}

		if (!byGuid.TryGetValue(p_guid, out ReferenceableScriptableObject entry))
		{
			return false;
		}

		if (entry is not T typed)
		{
			Debug.LogWarning(
				$"[ReferenceRegistry] GUID [{p_guid}] resolved to [{entry.GetType().Name}], expected [{typeof(T).Name}].");
			return false;
		}

		p_result = typed;
		return true;
	}

	public T Resolve<T>(string p_guid) where T : ReferenceableScriptableObject
	{
		if (TryResolve<T>(p_guid, out T result))
		{
			return result;
		}

		Debug.LogError($"[ReferenceRegistry] Could not resolve GUID [{p_guid}] as [{typeof(T).Name}].");
		return null;
	}

	private void Register(ReferenceableScriptableObject p_entry)
	{
		if (p_entry == null)
		{
			return;
		}

		if (string.IsNullOrEmpty(p_entry.Guid))
		{
			Debug.LogWarning(
				$"[ReferenceRegistry] Asset [{p_entry.name}] has no GUID — skipped. Run Tools/Erelia/Rebuild Reference Database.");
			return;
		}

		if (byGuid.TryGetValue(p_entry.Guid, out ReferenceableScriptableObject existing))
		{
			Debug.LogError(
				$"[ReferenceRegistry] GUID collision between [{p_entry.name}] and [{existing.name}] (GUID: {p_entry.Guid}). Database may be stale — rebuild it.");
			return;
		}

		byGuid[p_entry.Guid] = p_entry;
	}
}
