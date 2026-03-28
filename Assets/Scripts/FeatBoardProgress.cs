using System;
using System.Collections.Generic;

[Serializable]
public class FeatBoardProgress
{
	public HashSet<string> ExhaustedNodeGuids = new();
	public Dictionary<string, FeatNodeProgress> ActiveNodeProgressByGuid = new();
}