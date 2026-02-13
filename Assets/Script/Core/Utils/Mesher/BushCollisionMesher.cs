using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils.Mesher
{
	[Serializable]
	public class BushCollisionMesher : Utils.Mesher.CollisionMesher
	{
		protected override string MeshName => "BushCollisionMesh";

		protected override bool IsAcceptableDefinition(Core.Voxel.Model.Definition definition)
		{
			return definition != null && definition.Data.Collision == Core.Voxel.Model.Collision.Bush;
		}

		public static List<Mesh> Build(Core.Voxel.Model.Cell[,,] cells)
		{
			return new BushCollisionMesher().BuildMeshes(cells);
		}
	}
}
