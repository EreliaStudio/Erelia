using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Cross Plane")]
public class CrossPlaneVoxel : Voxel
{
	[SerializeField] private Sprite sprite;

	protected override List<VoxelFace> ConstructInnerFaces()
	{
		var faces = new List<VoxelFace>();

		SpriteUvUtils.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		Vector2 uvC = uvAnchor + uvSize;
		Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

		VoxelFace planeA = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
		faces.Add(planeA);
		VoxelFace planeABack = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB });
		faces.Add(planeABack);

		VoxelFace planeB = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
		faces.Add(planeB);
		VoxelFace planeBBack = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB });
		faces.Add(planeBBack);

		return faces;
	}

	protected override Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces()
	{
		return new Dictionary<OuterShellPlane, VoxelFace>();
	}
}
