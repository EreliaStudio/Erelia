using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunk.Controller
{
	[Serializable]
	public class SolidCollisionMesher : World.Chunk.Controller.CollisionMesher
	{
		protected override string MeshName => "SolidCollisionMesh";

		protected override bool IsAcceptableDefinition(Voxel.Model.Definition definition)
		{
			return definition != null && definition.Data.Collision == Voxel.Model.Collision.Solid;
		}
	
		public static List<Mesh> Build(Voxel.Model.Cell[,,] cells)
		{
			return new SolidCollisionMesher().BuildMeshes(cells);
		}
	}
}
