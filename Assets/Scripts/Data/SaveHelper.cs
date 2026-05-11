using Newtonsoft.Json.Linq;
using UnityEngine;

public static class SaveHelper
{
	public static JObject ToJson(Vector3 p_vector)
	{
		return new JObject
		{
			["x"] = p_vector.x,
			["y"] = p_vector.y,
			["z"] = p_vector.z
		};
	}

	public static Vector3 ToVector3(JObject p_json)
	{
		if (p_json == null)
		{
			return Vector3.zero;
		}

		return new Vector3(
			p_json["x"]?.Value<float>() ?? 0f,
			p_json["y"]?.Value<float>() ?? 0f,
			p_json["z"]?.Value<float>() ?? 0f
		);
	}

	public static JObject ToJson(Vector3Int p_vector)
	{
		return new JObject
		{
			["x"] = p_vector.x,
			["y"] = p_vector.y,
			["z"] = p_vector.z
		};
	}

	public static Vector3Int ToVector3Int(JObject p_json)
	{
		if (p_json == null)
		{
			return Vector3Int.zero;
		}

		return new Vector3Int(
			p_json["x"]?.Value<int>() ?? 0,
			p_json["y"]?.Value<int>() ?? 0,
			p_json["z"]?.Value<int>() ?? 0
		);
	}
}
