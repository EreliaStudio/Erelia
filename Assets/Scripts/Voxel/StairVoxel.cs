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
        Vector2 uvA = uvAnchor;
        Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
        Vector2 uvC = uvAnchor + uvSize;
        Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
        VoxelFace stepTop = GeometryUtils.CreateRectangle(
            new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvA },
            new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvB },
            new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvC },
            new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvD });
        faces.Add(stepTop);

        SpriteUvUtils.GetSpriteUvRect(spriteStepRiser, out uvAnchor, out uvSize);
        uvA = uvAnchor;
        uvB = uvAnchor + new Vector2(uvSize.x, 0f);
        uvC = uvAnchor + uvSize;
        uvD = uvAnchor + new Vector2(0f, uvSize.y);
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
        Vector2 uvA = Vector2.zero;
        Vector2 uvB = Vector2.zero;
        Vector2 uvC = Vector2.zero;
        Vector2 uvD = Vector2.zero;

        SpriteUvUtils.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
        uvA = uvAnchor;
        uvB = uvAnchor + new Vector2(uvSize.x, 0f);
        uvC = uvAnchor + uvSize;
        uvD = uvAnchor + new Vector2(0f, uvSize.y);
        var posX = new VoxelFace();
        posX.AddPolygon(GeometryUtils.CreateRectanglePolygon(
            new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
            new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, StepDepth), UV = uvB },
            new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvC },
            new GeometryUtils.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvD }));
        posX.AddPolygon(GeometryUtils.CreateRectanglePolygon(
            new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, StepDepth), UV = uvA },
            new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
            new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
            new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvD }));
        faces[OuterShellPlane.PosX] = posX;

        SpriteUvUtils.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
        uvA = uvAnchor;
        uvB = uvAnchor + new Vector2(uvSize.x, 0f);
        uvC = uvAnchor + uvSize;
        uvD = uvAnchor + new Vector2(0f, uvSize.y);
        var negX = new VoxelFace();
        negX.AddPolygon(GeometryUtils.CreateRectanglePolygon(
            new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
            new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvB },
            new GeometryUtils.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvC },
            new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, StepDepth), UV = uvD }));
        negX.AddPolygon(GeometryUtils.CreateRectanglePolygon(
            new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, StepDepth), UV = uvA },
            new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvB },
            new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
            new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD }));
        faces[OuterShellPlane.NegX] = negX;

        SpriteUvUtils.GetSpriteUvRect(spriteFront, out uvAnchor, out uvSize);
        uvA = uvAnchor;
        uvB = uvAnchor + new Vector2(uvSize.x, 0f);
        uvC = uvAnchor + uvSize;
        uvD = uvAnchor + new Vector2(0f, uvSize.y);
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

        SpriteUvUtils.GetSpriteUvRect(spriteTop, out uvAnchor, out uvSize);
        uvA = uvAnchor;
        uvB = uvAnchor + new Vector2(uvSize.x, 0f);
        uvC = uvAnchor + uvSize;
        uvD = uvAnchor + new Vector2(0f, uvSize.y);
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
