using System;
using UnityEngine;

[Serializable]
public class VoxelSlopeShape : VoxelShape
{
	[SerializeField] private Sprite spriteBack;
	[SerializeField] private Sprite spriteBottom;
	[SerializeField] private Sprite spriteSlope;
	[SerializeField] private Sprite spriteSideLeft;
	[SerializeField] private Sprite spriteSideRight;

	protected override FaceSet ConstructRenderFaces()
	{
		var faceSet = new FaceSet();
		faceSet.InnerFaces.Add(CreateSlopeFace());
		faceSet.OuterShell.PosX = CreateSideTriangle(spriteSideRight, true);
		faceSet.OuterShell.NegX = CreateSideTriangle(spriteSideLeft, false);
		faceSet.OuterShell.NegY = CreateBottomFace();
		faceSet.OuterShell.PosZ = CreateBackFace();
		return faceSet;
	}

	protected override MaskSet ConstructMask()
	{
		const float maskOffset = 0.01f;
		var mask = new MaskSet();

		mask.PositiveYFaces.Add(CreateRectangle(
			new Vector3(0f, 0f + maskOffset, 0f), new Vector2(0f, 0f),
			new Vector3(1f, 0f + maskOffset, 0f), new Vector2(1f, 0f),
			new Vector3(1f, 1f + maskOffset, 1f), new Vector2(1f, 1f),
			new Vector3(0f, 1f + maskOffset, 1f), new Vector2(0f, 1f)));

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

	private Face CreateSlopeFace()
	{
		GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 0f), uvAnchor,
			new Vector3(1f, 0f, 0f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(0f, 1f, 1f), uvAnchor + new Vector2(0f, uvSize.y));
	}

	private Face CreateSideTriangle(Sprite sprite, bool positiveX)
	{
		GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
		Vector2 uvA = uvAnchor;
		Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
		Vector2 uvC = uvAnchor + new Vector2(0f, uvSize.y);

		return positiveX
			? CreateTriangle(new Vector3(1f, 0f, 0f), uvA, new Vector3(1f, 0f, 1f), uvB, new Vector3(1f, 1f, 1f), uvC)
			: CreateTriangle(new Vector3(0f, 0f, 0f), uvA, new Vector3(0f, 1f, 1f), uvB, new Vector3(0f, 0f, 1f), uvC);
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

	private Face CreateBackFace()
	{
		GetSpriteUvRect(spriteBack, out Vector2 uvAnchor, out Vector2 uvSize);
		return CreateRectangle(
			new Vector3(0f, 0f, 1f), uvAnchor,
			new Vector3(0f, 1f, 1f), uvAnchor + new Vector2(uvSize.x, 0f),
			new Vector3(1f, 1f, 1f), uvAnchor + uvSize,
			new Vector3(1f, 0f, 1f), uvAnchor + new Vector2(0f, uvSize.y));
	}
}
