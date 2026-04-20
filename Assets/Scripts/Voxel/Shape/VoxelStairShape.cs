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
	const float riserOutset = -0.01f;

	float lowerY = StepHeight + maskOffset;
	float upperY = 1f + maskOffset;
	float riserZ = StepDepth + riserOutset;
	float right = 1f;

	float v0 = 0f;
	float v1 = 1f / 3f;
	float v2 = 2f / 3f;
	float v3 = 1f;

	var mask = new MaskSet();

	// Top platform -> top third
	mask.PositiveYFaces.Add(CreateRectangle(
		new Vector3(0f, upperY, StepDepth), new Vector2(0f, v2),
		new Vector3(right, upperY, StepDepth), new Vector2(1f, v2),
		new Vector3(right, upperY, 1f), new Vector2(1f, v3),
		new Vector3(0f, upperY, 1f), new Vector2(0f, v3)));

	// Vertical riser -> middle third
	mask.PositiveYFaces.Add(CreateRectangle(
		new Vector3(0f, lowerY, riserZ), new Vector2(0f, v1),
		new Vector3(right, lowerY, riserZ), new Vector2(1f, v1),
		new Vector3(right, upperY, riserZ), new Vector2(1f, v2),
		new Vector3(0f, upperY, riserZ), new Vector2(0f, v2)));

	// Lower step top -> bottom third
	mask.PositiveYFaces.Add(CreateRectangle(
		new Vector3(0f, lowerY, 0f), new Vector2(0f, v0),
		new Vector3(1f, lowerY, 0f), new Vector2(1f, v0),
		new Vector3(1f, lowerY, StepDepth), new Vector2(1f, v1),
		new Vector3(0f, lowerY, StepDepth), new Vector2(0f, v1)));

	mask.NegativeYFaces.Add(CreateRectangle(
		new Vector3(0f, 1f + maskOffset, 0f), new Vector2(0f, 0f),
		new Vector3(1f, 1f + maskOffset, 0f), new Vector2(1f, 0f),
		new Vector3(1f, 1f + maskOffset, 1f), new Vector2(1f, 1f),
		new Vector3(0f, 1f + maskOffset, 1f), new Vector2(0f, 1f)));

	return mask;
}

	protected override CardinalHeightSet ConstructPositiveYCardinalHeights()
	{
		return new CardinalHeightSet(
			positiveX: 0.5f,
			negativeX: 0.5f,
			positiveZ: 1f,
			negativeZ: 0f,
			stationary: 0.5f);
	}

	protected override CardinalHeightSet ConstructNegativeYCardinalHeights()
	{
		return new CardinalHeightSet(
			positiveX: 1f,
			negativeX: 1f,
			positiveZ: 1f,
			negativeZ: 1f,
			stationary: 1f);
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
