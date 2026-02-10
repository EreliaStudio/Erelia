using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunk.Controller
{
	[Serializable]
	public class BushCollisionMesher : World.Chunk.Controller.CollisionMesher
	{
		protected override string MeshName => "BushCollisionMesh";

		protected override bool IsAcceptableDefinition(Voxel.Model.Definition definition)
		{
			return definition != null && definition.Data.Collision == Voxel.Model.Collision.Bush;
		}

		public static List<Mesh> Build(Voxel.Model.Cell[,,] cells)
		{
			return new BushCollisionMesher().BuildMeshes(cells);
		}
	}
}
