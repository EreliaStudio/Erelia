using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class CreatureForm
{
	public string DisplayName = "New Form";
	public int Tier = 0;
	public Sprite Icon;
	public GameObject ModelPrefab;
	public List<string> Tags = new List<string>();
};