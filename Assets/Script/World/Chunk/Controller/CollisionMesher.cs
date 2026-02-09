using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Chunk.Controller
{
	[Serializable]
	public abstract class CollisionMesher : World.Chunk.Core.Mesher
	{
		protected abstract bool IsAcceptableDefinition(Voxel.Model.Definition definition);

		protected virtual string MeshName => "CollisionMesh";

		public List<Mesh> BuildMeshes(World.Chunk.Model.Cell[,,] cells)
		{
			return new List<Mesh>();
		}
	}
}
