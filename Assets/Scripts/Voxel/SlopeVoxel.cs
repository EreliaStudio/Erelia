using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Slope")]
public class SlopeVoxel : Voxel
{
    [SerializeField] private Sprite spritePosX;
    [SerializeField] private Sprite spriteNegX;
    [SerializeField] private Sprite spriteSlope;
    [SerializeField] private Sprite spriteNegZ;
    [SerializeField] private Sprite spriteNegY;

	protected override List<VoxelFace> ConstructInnerFaces()
	{
		var faces = new List<VoxelFace>();

        SpriteUvUtils.GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
		VoxelFace slope = GeometryUtils.CreateFaceFromCorners(
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 1f, 1f),
			new Vector3(0f, 1f, 1f),
			uvAnchor,
			uvSize);
		faces.Add(slope);

		return faces;
	}

	protected override Dictionary<OuterShellPlane, VoxelFace> ConstructOuterShellFaces()
	{
		var faces = new Dictionary<OuterShellPlane, VoxelFace>();

        SpriteUvUtils.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
		VoxelFace posX = GeometryUtils.CreateFace(
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 1f, 1f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.PosX] = posX;

        SpriteUvUtils.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
		VoxelFace negX = GeometryUtils.CreateFace(
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 1f, 1f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.NegX] = negX;

        SpriteUvUtils.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
		VoxelFace negY = GeometryUtils.CreateFace(
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 1f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.NegY] = negY;

        SpriteUvUtils.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
		VoxelFace negZ = GeometryUtils.CreateFace(
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 1f, 0f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.NegZ] = negZ;

		return faces;
	}
}
