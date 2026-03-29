using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FeatBoard
{
	[SerializeReference]
	public List<FeatNode> Nodes = new List<FeatNode>();
}
