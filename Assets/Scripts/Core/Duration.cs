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

	public static Duration Clone(Duration p_source)
	{
		if (p_source == null)
		{
			return new Duration();
		}

		return new Duration
		{
			Type = p_source.Type,
			Turns = p_source.Turns,
			Seconds = p_source.Seconds
		};
	}
}
