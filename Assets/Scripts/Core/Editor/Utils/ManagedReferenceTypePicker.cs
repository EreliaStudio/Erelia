using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

internal static class ManagedReferenceTypePicker
{
	private static readonly Dictionary<Type, Type[]> ConcreteTypesCache = new Dictionary<Type, Type[]>();

	public static Type[] GetConcreteTypes(Type baseType, Comparison<Type> comparison = null)
	{
		if (baseType == null)
		{
			return Array.Empty<Type>();
		}

		if (!ConcreteTypesCache.TryGetValue(baseType, out Type[] concreteTypes))
		{
			List<Type> discoveredTypes = new List<Type>();
			foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
			{
				if (!IsSelectableConcreteType(type))
				{
					continue;
				}

				discoveredTypes.Add(type);
			}

			concreteTypes = discoveredTypes.ToArray();
			Array.Sort(concreteTypes, (left, right) => string.CompareOrdinal(left.FullName, right.FullName));
			ConcreteTypesCache[baseType] = concreteTypes;
		}

		if (comparison == null)
		{
			return concreteTypes;
		}

		Type[] sortedTypes = (Type[])concreteTypes.Clone();
		Array.Sort(sortedTypes, comparison);
		return sortedTypes;
	}

	public static string[] BuildDisplayNames(Type[] concreteTypes, Func<Type, string> labelFormatter, bool includeNullOption, string nullLabel = "None")
	{
		int optionCount = concreteTypes != null ? concreteTypes.Length : 0;
		int offset = includeNullOption ? 1 : 0;
		string[] labels = new string[optionCount + offset];

		if (includeNullOption)
		{
			labels[0] = nullLabel;
		}

		if (concreteTypes == null)
		{
			return labels;
		}

		for (int i = 0; i < concreteTypes.Length; i++)
		{
			Type type = concreteTypes[i];
			labels[i + offset] = labelFormatter != null
				? labelFormatter(type)
				: ObjectNames.NicifyVariableName(type.Name);
		}

		return labels;
	}

	public static int GetSelectionIndex(Type currentType, Type[] concreteTypes, bool includeNullOption)
	{
		if (currentType == null)
		{
			return includeNullOption ? 0 : -1;
		}

		if (concreteTypes == null)
		{
			return -1;
		}

		for (int i = 0; i < concreteTypes.Length; i++)
		{
			if (concreteTypes[i] == currentType)
			{
				return includeNullOption ? i + 1 : i;
			}
		}

		return -1;
	}

	public static Type GetTypeForSelection(int selectedIndex, Type[] concreteTypes, bool includeNullOption)
	{
		if (includeNullOption)
		{
			if (selectedIndex <= 0)
			{
				return null;
			}

			selectedIndex--;
		}

		if (concreteTypes == null || selectedIndex < 0 || selectedIndex >= concreteTypes.Length)
		{
			return null;
		}

		return concreteTypes[selectedIndex];
	}

	public static object CreateInstance(Type concreteType)
	{
		return concreteType == null ? null : Activator.CreateInstance(concreteType, nonPublic: true);
	}

	public static string NicifyTypeName(Type type, string prefixToTrim = null, string suffixToTrim = null)
	{
		if (type == null)
		{
			return string.Empty;
		}

		string name = type.Name;

		if (!string.IsNullOrEmpty(prefixToTrim) && name.StartsWith(prefixToTrim, StringComparison.Ordinal))
		{
			name = name.Substring(prefixToTrim.Length);
		}

		if (!string.IsNullOrEmpty(suffixToTrim) && name.EndsWith(suffixToTrim, StringComparison.Ordinal))
		{
			name = name.Substring(0, name.Length - suffixToTrim.Length);
		}

		return ObjectNames.NicifyVariableName(name);
	}

	private static bool IsSelectableConcreteType(Type type)
	{
		if (type == null || type.IsAbstract || type.IsGenericType || type.ContainsGenericParameters)
		{
			return false;
		}

		ConstructorInfo constructor = type.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			types: Type.EmptyTypes,
			modifiers: null);

		return constructor != null;
	}
}
