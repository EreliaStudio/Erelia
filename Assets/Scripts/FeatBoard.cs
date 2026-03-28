using AYellowpaper.SerializedCollections;
using System;

[Serializable]
public class FeatBoard
{
	[SerializedDictionary("Node Guid", "Node")]
	public SerializedDictionary<string, FeatNode> NodesByGuid = new SerializedDictionary<string, FeatNode>();
}
