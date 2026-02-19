using System;
using UnityEngine;

namespace Exploration.World.Chunk.Model
{
	public abstract class AbstractGenerator : ScriptableObject
	{
		public abstract Exploration.World.Chunk.Model.Data Generate(Exploration.World.Chunk.Model.Coordinates coordinate);
	}
}
