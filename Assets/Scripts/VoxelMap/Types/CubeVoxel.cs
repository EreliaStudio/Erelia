using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Cube")]
public class CubeVoxel : Voxel
{
	[SerializeField] private Sprite spritePosX;
	[SerializeField] private Sprite spriteNegX;
	[SerializeField] private Sprite spritePosY;
	[SerializeField] private Sprite spriteNegY;
	[SerializeField] private Sprite spritePosZ;
	[SerializeField] private Sprite spriteNegZ;

	protected override List<VoxelFace> ConstructInnerFaces()
	{
		return new List<VoxelFace>();
	}

	protected override Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces()
	{
		var faces = new Dictionary<OuterShellPlane, VoxelFace>();

		SpriteUvUtils.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		Vector2 uvC = uvAnchor + uvSize;
		Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace posX = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
		faces[OuterShellPlane.PosX] = posX;

		SpriteUvUtils.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
		uvA = uvAnchor;
		uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvC = uvAnchor + uvSize;
		uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace negX = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });
		faces[OuterShellPlane.NegX] = negX;

		SpriteUvUtils.GetSpriteUvRect(spritePosY, out uvAnchor, out uvSize);
		uvA = uvAnchor;
		uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvC = uvAnchor + uvSize;
		uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace posY = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
		faces[OuterShellPlane.PosY] = posY;

		SpriteUvUtils.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
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

		SpriteUvUtils.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
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

		SpriteUvUtils.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
		uvA = uvAnchor;
		uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		uvC = uvAnchor + uvSize;
		uvD = uvAnchor + new Vector2(0f, uvSize.y);
		VoxelFace negZ = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
		faces[OuterShellPlane.NegZ] = negZ;

		return faces;
	}
}
