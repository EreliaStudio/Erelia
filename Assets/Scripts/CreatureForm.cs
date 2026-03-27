using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class CreatureForm
{
	public Sprite Icon;
	public GameObject ModelPrefab;
	public List<string> Tags = new List<string>();
};