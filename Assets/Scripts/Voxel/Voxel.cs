using System;

[Serializable]
public class Voxel
{
    public int DataId;

    public Voxel()
    {
    }

    public Voxel(int dataId)
    {
        DataId = dataId;
    }
}
