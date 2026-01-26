using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Stair")]
public class StairVoxel : Voxel
{
	[SerializeField] private Sprite spriteFront;
	[SerializeField] private Sprite spriteBack;
	[SerializeField] private Sprite spriteBottom;
	[SerializeField] private Sprite spriteTop;
	[SerializeField] private Sprite spriteSideLeft;
	[SerializeField] private Sprite spriteSideRight;
	[SerializeField] private Sprite spriteStepTop;
	[SerializeField] private Sprite spriteStepRiser;

	private const float StepHeight = 0.5f;
	private const float StepDepth = 0.5f;

	protected override List<VoxelFace> ConstructInnerFaces()
	{
		var faces = new List<VoxelFace>();

		SpriteUvUtils.GetSpriteUvRect(spriteStepTop, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		Vector2 halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
		Vector2 uvA = halfUvAnchor;
		Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
		Vector2 uvC = halfUvAnchor + halfUvSize;
		Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
		VoxelFace stepTop = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvD });
		faces.Add(stepTop);

		SpriteUvUtils.GetSpriteUvRect(spriteStepRiser, out uvAnchor, out uvSize);
		halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		halfUvAnchor = uvAnchor;
		uvA = halfUvAnchor;
		uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
		uvC = halfUvAnchor + halfUvSize;
		uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
		VoxelFace stepRiser = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvD });
		faces.Add(stepRiser);

		return faces;
	}

	protected override Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces()
	{
		var faces = new Dictionary<OuterShellPlane, VoxelFace>();
		Vector2 uvAnchor = Vector2.zero;
		Vector2 uvSize = Vector2.zero;
		Vector2 halfUvSize = Vector2.zero;
		Vector2 halfUvAnchor = Vector2.zero;
		Vector2 uvA = Vector2.zero;
		Vector2 uvB = Vector2.zero;
		Vector2 uvC = Vector2.zero;
		Vector2 uvD = Vector2.zero;

		SpriteUvUtils.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
		float u0 = uvAnchor.x;
		float u1 = uvAnchor.x + uvSize.x * 0.5f;
		float u2 = uvAnchor.x + uvSize.x;
		float v0 = uvAnchor.y;
		float v1 = uvAnchor.y + uvSize.y * 0.5f;
		float v2 = uvAnchor.y + uvSize.y;
		var posX = new VoxelFace();
		posX.AddPolygon(new List<FaceVertex>
		{
			new FaceVertex { Position = new Vector3(1f, 0f, 0f), TileUV = new Vector2(u2, v0) },
			new FaceVertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
			new FaceVertex { Position = new Vector3(1f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
			new FaceVertex { Position = new Vector3(1f, StepHeight, 0f), TileUV = new Vector2(u2, v1) }
		});
		posX.AddPolygon(new List<FaceVertex>
		{
			new FaceVertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
			new FaceVertex { Position = new Vector3(1f, 0f, 1f), TileUV = new Vector2(u0, v0) },
			new FaceVertex { Position = new Vector3(1f, 1f, 1f), TileUV = new Vector2(u0, v2) },
			new FaceVertex { Position = new Vector3(1f, 1f, StepDepth), TileUV = new Vector2(u1, v2) }
		});
		faces[OuterShellPlane.PosX] = posX;

		SpriteUvUtils.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
		u0 = uvAnchor.x;
		u1 = uvAnchor.x + uvSize.x * 0.5f;
		u2 = uvAnchor.x + uvSize.x;
		v0 = uvAnchor.y;
		v1 = uvAnchor.y + uvSize.y * 0.5f;
		v2 = uvAnchor.y + uvSize.y;
		var negX = new VoxelFace();
		negX.AddPolygon(new List<FaceVertex>
		{
			new FaceVertex { Position = new Vector3(0f, 0f, 0f), TileUV = new Vector2(u2, v0) },
			new FaceVertex { Position = new Vector3(0f, StepHeight, 0f), TileUV = new Vector2(u2, v1) },
			new FaceVertex { Position = new Vector3(0f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
			new FaceVertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) }
		});
		negX.AddPolygon(new List<FaceVertex>
		{
			new FaceVertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
			new FaceVertex { Position = new Vector3(0f, 1f, StepDepth), TileUV = new Vector2(u1, v2) },
			new FaceVertex { Position = new Vector3(0f, 1f, 1f), TileUV = new Vector2(u0, v2) },
			new FaceVertex { Position = new Vector3(0f, 0f, 1f), TileUV = new Vector2(u0, v0) }
		});
		faces[OuterShellPlane.NegX] = negX;

		SpriteUvUtils.GetSpriteUvRect(spriteFront, out uvAnchor, out uvSize);
		halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		halfUvAnchor = uvAnchor;
		uvA = halfUvAnchor;
		uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
		uvC = halfUvAnchor + halfUvSize;
		uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
		VoxelFace negZ = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvD });
		faces[OuterShellPlane.NegZ] = negZ;

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

		SpriteUvUtils.GetSpriteUvRect(spriteStepTop, out uvAnchor, out uvSize);
		halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
		uvA = halfUvAnchor;
		uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
		uvC = halfUvAnchor + halfUvSize;
		uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
		VoxelFace posY = GeometryUtils.CreateRectangle(
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvA },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvB },
			new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
			new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
		faces[OuterShellPlane.PosY] = posY;

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

		return faces;
	}
}
