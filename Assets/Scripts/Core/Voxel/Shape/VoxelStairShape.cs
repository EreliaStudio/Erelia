using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelStairShape : VoxelShape
{
	private const float StepHeight = 0.5f;
	private const float StepDepth = 0.5f;

	[SerializeField] private Sprite spriteFront;
	[SerializeField] private Sprite spriteBack;
	[SerializeField] private Sprite spriteBottom;
	[SerializeField] private Sprite spriteTop;
	[SerializeField] private Sprite spriteSideLeft;
	[SerializeField] private Sprite spriteSideRight;
	[SerializeField] private Sprite spriteStepTop;
	[SerializeField] private Sprite spriteStepRiser;

	protected override FaceSet ConstructRenderFaces()
	{
		var faceSet = new FaceSet();
		faceSet.InnerFaces.Add(CreateInnerStepTop());
		faceSet.InnerFaces.Add(CreateInnerStepRiser());
		faceSet.OuterShell.PosX = CreatePositiveXFace();
		faceSet.OuterShell.NegX = CreateNegativeXFace();
		faceSet.OuterShell.NegZ = CreateFrontFace();
		faceSet.OuterShell.PosZ = CreateBackFace();
		faceSet.OuterShell.PosY = CreateTopFace();
		faceSet.OuterShell.NegY = CreateBottomFace();
		return faceSet;
	}

	protected override MaskSet ConstructMask()
	{
		const float maskOffset = 0.01f;
		float lowerY = StepHeight + maskOffset;
		float upperY = 1f + maskOffset;
		float riserZ = StepDepth;
		float uvStep = StepDepth;
		float right = 1f;

		var positiveFaces = new List<Face>
		{
			CreateRectangle(
				new Vector3(0f, upperY, StepDepth), new Vector2(0f, uvStep * 2f),
				new Vector3(right, upperY, StepDepth), new Vector2(1f, uvStep * 2f),
				new Vector3(right, upperY, 1f), new Vector2(1f, 1f),
				new Vector3(0f, upperY, 1f), new Vector2(0f, 1f)),
			CreateRectangle(
				new Vector3(0f, lowerY, riserZ), new Vector2(0f, uvStep),
				new Vector3(right, lowerY, riserZ), new Vector2(1f, uvStep),
				new Vector3(right, upperY, riserZ), new Vector2(1f, uvStep * 2f),
				new Vector3(0f, upperY, riserZ), new Vector2(0f, uvStep * 2f)),
			CreateRectangle(
				new Vector3(0f, lowerY, 0f), new Vector2(0f, 0f),
				new Vector3(1f, lowerY, 0f), new Vector2(1f, 0f),
				new Vector3(1f, lowerY, StepDepth), new Vector2(1f, uvStep),
				new Vector3(0f, lowerY, StepDepth), new Vector2(0f, uvStep))
		};

		var negativeFaces = new List<Face>
		{
			CreateRectangle(
				new Vector3(0f, 1f + maskOffset, 0f), new Vector2(0f, 0f),
				new Vector3(1f, 1f + maskOffset, 0f), new Vector2(1f, 0f),
				new Vector3(1f, 1f + maskOffset, 1f), new Vector2(1f, 1f),
				new Vector3(0f, 1f + maskOffset, 1f), new Vector2(0f, 1f))
		};

		var mask = new MaskSet();
		mask.PositiveYFaces.AddRange(positiveFaces);
		mask.NegativeYFaces.AddRange(negativeFaces);
		return mask;
	}

	private Face CreateInnerStepTop()
	{
		GetSpriteUvRect(spriteStepTop, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		Vector2 halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
		return CreateRectangle(
			new Vector3(0f, StepHeight, 0f), halfUvAnchor,
			new Vector3(1f, StepHeight, 0f), halfUvAnchor + new Vector2(halfUvSize.x, 0f),
			new Vector3(1f, StepHeight, StepDepth), halfUvAnchor + halfUvSize,
			new Vector3(0f, StepHeight, StepDepth), halfUvAnchor + new Vector2(0f, halfUvSize.y));
	}

	private Face CreateInnerStepRiser()
	{
		GetSpriteUvRect(spriteStepRiser, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		return CreateRectangle(
			new Vector3(0f, StepHeight, StepDepth), uvAnchor,
			new Vector3(1f, StepHeight, StepDepth), uvAnchor + new Vector2(halfUvSize.x, 0f),
			new Vector3(1f, 1f, StepDepth), uvAnchor + halfUvSize,
			new Vector3(0f, 1f, StepDepth), uvAnchor + new Vector2(0f, halfUvSize.y));
	}

	private Face CreatePositiveXFace()
	{
		GetSpriteUvRect(spriteSideRight, out Vector2 uvAnchor, out Vector2 uvSize);
		float u0 = uvAnchor.x;
		float u1 = uvAnchor.x + uvSize.x * 0.5f;
		float u2 = uvAnchor.x + uvSize.x;
		float v0 = uvAnchor.y;
		float v1 = uvAnchor.y + uvSize.y * 0.5f;
		float v2 = uvAnchor.y + uvSize.y;

		var face = new Face();
		face.AddPolygon(new List<Vertex>
		{
			new Vertex { Position = new Vector3(1f, 0f, 0f), UV = new Vector2(u2, v0) },
			new Vertex { Position = new Vector3(1f, 0f, StepDepth), UV = new Vector2(u1, v0) },
			new Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = new Vector2(u1, v1) },
			new Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = new Vector2(u2, v1) }
		});
		face.AddPolygon(new List<Vertex>
		{
			new Vertex { Position = new Vector3(1f, 0f, StepDepth), UV = new Vector2(u1, v0) },
			new Vertex { Position = new Vector3(1f, 0f, 1f), UV = new Vector2(u0, v0) },
			new Vertex { Position = new Vector3(1f, 1f, 1f), UV = new Vector2(u0, v2) },
			new Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = new Vector2(u1, v2) }
		});
		return face;
	}

	private Face CreateNegativeXFace()
	{
		GetSpriteUvRect(spriteSideLeft, out Vector2 uvAnchor, out Vector2 uvSize);
		float u0 = uvAnchor.x;
		float u1 = uvAnchor.x + uvSize.x * 0.5f;
		float u2 = uvAnchor.x + uvSize.x;
		float v0 = uvAnchor.y;
		float v1 = uvAnchor.y + uvSize.y * 0.5f;
		float v2 = uvAnchor.y + uvSize.y;

		var face = new Face();
		face.AddPolygon(new List<Vertex>
		{
			new Vertex { Position = new Vector3(0f, 0f, 0f), UV = new Vector2(u2, v0) },
			new Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = new Vector2(u2, v1) },
			new Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = new Vector2(u1, v1) },
			new Vertex { Position = new Vector3(0f, 0f, StepDepth), UV = new Vector2(u1, v0) }
		});
		face.AddPolygon(new List<Vertex>
		{
			new Vertex { Position = new Vector3(0f, 0f, StepDepth), UV = new Vector2(u1, v0) },
			new Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = new Vector2(u1, v2) },
			new Vertex { Position = new Vector3(0f, 1f, 1f), UV = new Vector2(u0, v2) },
			new Vertex { Position = new Vector3(0f, 0f, 1f), UV = new Vector2(u0, v0) }
		});
		return face;
	}

	private Face CreateFrontFace()
	{
		GetSpriteUvRect(spriteFront, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		return CreateRectangle(
			new Vector3(0f, 0f, 0f), uvAnchor,
			new Vector3(1f, 0f, 0f), uvAnchor + new Vector2(halfUvSize.x, 0f),
			new Vector3(1f, StepHeight, 0f), uvAnchor + halfUvSize,
			new Vector3(0f, StepHeight, 0f), uvAnchor + new Vector2(0f, halfUvSize.y));
	}

	private Face CreateBackFace()
	{
		GetSpriteUvRect(spriteBack, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 1f), uvAnchor,
			new Vector3(0f, 1f, 1f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(1f, 0f, 1f), uvAnchor + new Vector2(0f, uvSize.y));
	}

	private Face CreateTopFace()
	{
		GetSpriteUvRect(spriteStepTop, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
		Vector2 halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
		return CreateRectangle(
			new Vector3(0f, 1f, StepDepth), halfUvAnchor,
			new Vector3(1f, 1f, StepDepth), halfUvAnchor + new Vector2(halfUvSize.x, 0f),
			new Vector3(1f, 1f, 1f), halfUvAnchor + halfUvSize,
			new Vector3(0f, 1f, 1f), halfUvAnchor + new Vector2(0f, halfUvSize.y));
	}

	private Face CreateBottomFace()
	{
		GetSpriteUvRect(spriteBottom, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 0f), uvAnchor,
			new Vector3(0f, 0f, 1f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, 0f, 1f), uvAnchor + uvSize,
			new Vector3(1f, 0f, 0f), uvAnchor + new Vector2(0f, uvSize.y));
	}
}
