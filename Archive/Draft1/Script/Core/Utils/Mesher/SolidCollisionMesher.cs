using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils.Mesher
{
	[Serializable]
	public class SolidCollisionMesher : Utils.Mesher.CollisionMesher
	{
		protected override string MeshName => "SolidCollisionMesh";

		protected override bool IsAcceptableDefinition(Core.Voxel.Model.Definition definition)
		{
			return definition != null && definition.Data.Collision == Core.Voxel.Model.Collision.Solid;
		}
	
		public static List<Mesh> Build(Core.Voxel.Model.Cell[,,] cells)
		{
			return new SolidCollisionMesher().BuildMeshes(cells);
		}
	}
}
