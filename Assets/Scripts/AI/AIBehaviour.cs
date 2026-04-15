using System;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

[Serializable]
public class AIBehaviour
{
	public string ActiveMode = "";

	[SerializedDictionary("Mode", "AI Rules")]
	public SerializedDictionary<string, List<AIRule>> RulesByModes = new SerializedDictionary<string, List<AIRule>>();
};