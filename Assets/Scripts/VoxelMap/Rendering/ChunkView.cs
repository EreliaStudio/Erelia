using UnityEngine;

public class ChunkView : MonoBehaviour
{
	public ChunkCoord Coord { get; private set; }
	public Chunk Chunk { get; private set; }

    private ChunkMesher mesher;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider solidCollider;
    private MeshCollider triggerCollider;

	public void Initialize(ChunkCoord coord, Chunk chunk, ChunkMesher mesherInstance, Material material)
	{
		Coord = coord;
		Chunk = chunk;
		mesher = mesherInstance;

		EnsureComponents();
		if (material != null)
		{
			meshRenderer.sharedMaterial = material;
		}

		RebuildMesh();
	}

    public void RebuildMesh()
    {
        if (Chunk == null || mesher == null)
        {
            return;
        }

        meshFilter.sharedMesh = mesher.BuildMesh(Chunk);
        mesher.BuildCollisionMeshes(Chunk, out Mesh solidMesh, out Mesh triggerMesh);
        ApplyCollider(solidCollider, solidMesh, false);
        ApplyCollider(triggerCollider, triggerMesh, true);
    }

    private void EnsureComponents()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
		}

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }

        if (solidCollider == null)
        {
            solidCollider = GetComponent<MeshCollider>();
            if (solidCollider == null)
            {
                solidCollider = gameObject.AddComponent<MeshCollider>();
            }
        }

        if (triggerCollider == null)
        {
            MeshCollider[] colliders = GetComponents<MeshCollider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != solidCollider && colliders[i].isTrigger)
                {
                    triggerCollider = colliders[i];
                    break;
                }
            }

            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<MeshCollider>();
                triggerCollider.isTrigger = true;
            }
        }
    }

    private static void ApplyCollider(MeshCollider collider, Mesh mesh, bool isTrigger)
    {
        if (collider == null)
        {
            return;
        }

        collider.isTrigger = isTrigger;
        if (mesh == null || mesh.vertexCount == 0)
        {
            collider.sharedMesh = null;
            return;
        }

        collider.sharedMesh = mesh;
    }
}
