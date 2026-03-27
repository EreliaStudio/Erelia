using System;
using UnityEngine;

[Serializable]
public class StatValue
{
	[HideInInspector]
	public int Value = 10;

	public int Maximum = 10;

	public float Ratio => Maximum > 0 ? (float)Value / Maximum : 0f;

	public StatValue(int p_value)
	{
		Value = p_value;
		Maximum = p_value;
	}
};