using UnityEngine;

public static class OuterShellPlaneUtil
{
    public const float NormalEpsilon = 0.001f;

    public static bool TryFromNormal(Vector3 normal, out OuterShellPlane plane)
    {
        if (normal.sqrMagnitude < NormalEpsilon)
        {
            plane = OuterShellPlane.PosX;
            return false;
        }

        Vector3 n = normal.normalized;
        float ax = Mathf.Abs(n.x);
        float ay = Mathf.Abs(n.y);
        float az = Mathf.Abs(n.z);

        if (ax >= 1f - NormalEpsilon && ay <= NormalEpsilon && az <= NormalEpsilon)
        {
            plane = n.x >= 0f ? OuterShellPlane.PosX : OuterShellPlane.NegX;
            return true;
        }
        if (ay >= 1f - NormalEpsilon && ax <= NormalEpsilon && az <= NormalEpsilon)
        {
            plane = n.y >= 0f ? OuterShellPlane.PosY : OuterShellPlane.NegY;
            return true;
        }
        if (az >= 1f - NormalEpsilon && ax <= NormalEpsilon && ay <= NormalEpsilon)
        {
            plane = n.z >= 0f ? OuterShellPlane.PosZ : OuterShellPlane.NegZ;
            return true;
        }

        plane = OuterShellPlane.PosX;
        return false;
    }

    public static Vector3 PlaneToNormal(OuterShellPlane plane)
    {
        switch (plane)
        {
            case OuterShellPlane.PosX:
                return Vector3.right;
            case OuterShellPlane.NegX:
                return Vector3.left;
            case OuterShellPlane.PosY:
                return Vector3.up;
            case OuterShellPlane.NegY:
                return Vector3.down;
            case OuterShellPlane.PosZ:
                return Vector3.forward;
            case OuterShellPlane.NegZ:
                return Vector3.back;
            default:
                return Vector3.zero;
        }
    }

    public static Vector3Int PlaneToOffset(OuterShellPlane plane)
    {
        switch (plane)
        {
            case OuterShellPlane.PosX:
                return new Vector3Int(1, 0, 0);
            case OuterShellPlane.NegX:
                return new Vector3Int(-1, 0, 0);
            case OuterShellPlane.PosY:
                return new Vector3Int(0, 1, 0);
            case OuterShellPlane.NegY:
                return new Vector3Int(0, -1, 0);
            case OuterShellPlane.PosZ:
                return new Vector3Int(0, 0, 1);
            case OuterShellPlane.NegZ:
                return new Vector3Int(0, 0, -1);
            default:
                return Vector3Int.zero;
        }
    }

    public static OuterShellPlane GetOppositePlane(OuterShellPlane plane)
    {
        switch (plane)
        {
            case OuterShellPlane.PosX:
                return OuterShellPlane.NegX;
            case OuterShellPlane.NegX:
                return OuterShellPlane.PosX;
            case OuterShellPlane.PosY:
                return OuterShellPlane.NegY;
            case OuterShellPlane.NegY:
                return OuterShellPlane.PosY;
            case OuterShellPlane.PosZ:
                return OuterShellPlane.NegZ;
            case OuterShellPlane.NegZ:
                return OuterShellPlane.PosZ;
            default:
                return plane;
        }
    }
}
