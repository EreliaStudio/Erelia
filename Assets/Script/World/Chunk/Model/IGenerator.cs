using System;
using UnityEngine;

namespace World.Chunk.Model
{
	public abstract class IGenerator : ScriptableObject
	{
		public abstract World.Chunk.Model.Data Generate(World.Chunk.Model.Coordinates coordinate);
	}
}