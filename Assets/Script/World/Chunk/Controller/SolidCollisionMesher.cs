using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunk.Controller
{
	[Serializable]
	public class SolidCollisionMesher : World.Chunk.CollisionMesher
	{
		protected override string MeshName => "SolidCollisionMesh";

		public List<Mesh> BuildSolidMeshes(World.Chunk.Cell[,,] cells)
		{
			return BuildMeshes(cells);
		}

		public static List<Mesh> Build(World.Chunk.Cell[,,] cells)
		{
			return new SolidCollisionMesher().BuildMeshes(cells);
		}

		protected override bool IsAcceptableDefinition(Voxel.Model.Definition definition)
		{
			return definition != null && definition.Data.Collision == Voxel.Collision.Solid;
		}
	}
}
