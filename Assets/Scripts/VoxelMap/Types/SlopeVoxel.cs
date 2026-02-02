using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Slope")]
public class SlopeVoxel : Voxel
{
	[SerializeField] private Sprite spriteBack;
	[SerializeField] private Sprite spriteBottom;
	[SerializeField] private Sprite spriteSlope;
	[SerializeField] private Sprite spriteSideLeft;
	[SerializeField] private Sprite spriteSideRight;

	protected override List<VoxelFace> ConstructInnerFaces()
	{
		var faces = new List<VoxelFace>();

		SpriteUvUtils.GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		Vector2 uvC = uvAnchor + uvSize;
		Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace slope = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
		faces.Add(slope);

		return faces;
	}

	protected override List<VoxelFace> ConstructMaskFaces()
	{
		const float maskOffset = 0.01f;
		var faces = new List<VoxelFace>();
		VoxelFace slope = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
		faces.Add(slope);
		return faces;
	}

	protected override List<VoxelFace> ConstructFlippedMaskFaces()
	{
		const float maskOffset = 0.01f;
		var faces = new List<VoxelFace>();
		VoxelFace top = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
		faces.Add(top);
		return faces;
	}

	protected override Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces()
	{
		var faces = new Dictionary<OuterShellPlane, VoxelFace>();
		Vector2 uvAnchor = Vector2.zero;
		Vector2 uvSize = Vector2.zero;
		Vector2 uvA = Vector2.zero;
		Vector2 uvB = Vector2.zero;
		Vector2 uvC = Vector2.zero;
		Vector2 uvD = Vector2.zero;
		Vector2 uvTriA = Vector2.zero;
		Vector2 uvTriB = Vector2.zero;
		Vector2 uvTriC = Vector2.zero;

		SpriteUvUtils.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
		uvTriA = uvAnchor;
		uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvTriC = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace posX = GeometryUtils.CreateTriangle(
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvTriA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvTriB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvTriC });
		faces[OuterShellPlane.PosX] = posX;

		SpriteUvUtils.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
		uvTriA = uvAnchor;
		uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvTriC = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace negX = GeometryUtils.CreateTriangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvTriA },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvTriB },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvTriC });
		faces[OuterShellPlane.NegX] = negX;

		SpriteUvUtils.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
		uvA = uvAnchor;
		uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvC = uvAnchor + uvSize;
		uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace negY = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });
		faces[OuterShellPlane.NegY] = negY;

		SpriteUvUtils.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
		uvA = uvAnchor;
		uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvC = uvAnchor + uvSize;
		uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace posZ = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
		faces[OuterShellPlane.PosZ] = posZ;

		return faces;
	}

}
