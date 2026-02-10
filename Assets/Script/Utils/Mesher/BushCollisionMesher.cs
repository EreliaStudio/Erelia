using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.Mesher
{
	[Serializable]
	public class BushCollisionMesher : Utils.Mesher.CollisionMesher
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
