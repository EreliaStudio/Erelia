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

        SpriteUvUtils.GetSpriteUvRect(spritePosY, out uvAnchor, out uvSize);
		VoxelFace posY = GeometryUtils.CreateFace(
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 0f, 1f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.PosY] = posY;

        SpriteUvUtils.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
		VoxelFace negY = GeometryUtils.CreateFace(
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 1f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.NegY] = negY;

        SpriteUvUtils.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
		VoxelFace posZ = GeometryUtils.CreateFace(
			new Vector3(0f, 0f, 1f),
			new Vector3(1f, 1f, 0f),
			uvAnchor,
			uvSize);
		faces[OuterShellPlane.PosZ] = posZ;

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
