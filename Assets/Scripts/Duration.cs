using System;

[Serializable]
public class Duration
{
	public enum Kind
	{
		TurnBased,
		Seconds,
		Infinite
	}

	public Kind Type = Kind.Infinite;
	public int Turns = 1;
	public float Seconds = 1f;
};
