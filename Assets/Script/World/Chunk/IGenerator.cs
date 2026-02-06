using System;
using UnityEngine;

namespace World.Chunk
{
	public abstract class IGenerator : ScriptableObject
	{
		public abstract World.Chunk.Data Generate(World.Chunk.Coordinates coordinate);
	}
}