using System;
using System.Collections.Generic;

[Serializable]
public class VoxelData
{
	public VoxelTraversal Traversal = VoxelTraversal.Walkable;
	public List<string> Tags = new List<string>();

	public bool HasTag(string p_tag)
	{
		if (string.IsNullOrWhiteSpace(p_tag) || Tags == null)
		{
			return false;
		}

		for (int index = 0; index < Tags.Count; index++)
		{
			string currentTag = Tags[index];
			if (string.IsNullOrWhiteSpace(currentTag))
			{
				continue;
			}

			if (string.Equals(currentTag.Trim(), p_tag.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}
}
