using System.Collections;
using System.Reflection;
using UnityEditor;

public static class SerializedPropertyObjectResolver
{
	public static T GetTargetObjectOfProperty<T>(SerializedProperty p_property) where T : class
	{
		object currentObject = p_property.serializedObject.targetObject;
		string path = p_property.propertyPath.Replace(".Array.data[", "[");

		string[] elements = path.Split('.');

		for (int elementIndex = 0; elementIndex < elements.Length; elementIndex++)
		{
			string element = elements[elementIndex];

			if (element.Contains("["))
			{
				string fieldName = element.Substring(0, element.IndexOf("["));
				int itemIndex = int.Parse(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
				currentObject = GetValue(currentObject, fieldName, itemIndex);
			}
			else
			{
				currentObject = GetValue(currentObject, element);
			}
		}

		return currentObject as T;
	}

	private static object GetValue(object p_source, string p_name)
	{
		if (p_source == null)
		{
			return null;
		}

		System.Type type = p_source.GetType();
		while (type != null)
		{
			FieldInfo field = type.GetField(p_name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(p_source);
			}

			type = type.BaseType;
		}

		return null;
	}

	private static object GetValue(object p_source, string p_name, int p_index)
	{
		IEnumerable enumerable = GetValue(p_source, p_name) as IEnumerable;
		if (enumerable == null)
		{
			return null;
		}

		IEnumerator enumerator = enumerable.GetEnumerator();
		for (int index = 0; index <= p_index; index++)
		{
			if (!enumerator.MoveNext())
			{
				return null;
			}
		}

		return enumerator.Current;
	}
}