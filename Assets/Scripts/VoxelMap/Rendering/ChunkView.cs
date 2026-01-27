using UnityEngine;

public class ChunkView : MonoBehaviour
{
	public ChunkCoord Coord { get; private set; }
	public Chunk Chunk { get; private set; }

    private ChunkRenderMeshBuilder renderMesher;
    private ChunkSolidCollisionMeshBuilder solidCollisionMesher;
    private ChunkBushTriggerMeshBuilder bushTriggerMesher;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider solidCollider;
    private MeshFilter solidFilter;
    private Transform solidRoot;
    private readonly System.Collections.Generic.List<MeshCollider> solidColliders = new System.Collections.Generic.List<MeshCollider>();
    private readonly System.Collections.Generic.List<MeshFilter> solidFilters = new System.Collections.Generic.List<MeshFilter>();
    private readonly System.Collections.Generic.List<Transform> solidRoots = new System.Collections.Generic.List<Transform>();
    private readonly System.Collections.Generic.List<MeshCollider> bushColliders = new System.Collections.Generic.List<MeshCollider>();
    private readonly System.Collections.Generic.List<MeshFilter> bushFilters = new System.Collections.Generic.List<MeshFilter>();
    private readonly System.Collections.Generic.List<Transform> bushRoots = new System.Collections.Generic.List<Transform>();

	public void Initialize(ChunkCoord coord, Chunk chunk, ChunkRenderMeshBuilder renderMesherInstance, ChunkSolidCollisionMeshBuilder solidCollisionMesherInstance, ChunkBushTriggerMeshBuilder bushTriggerMesherInstance, Material material)
	{
		Coord = coord;
		Chunk = chunk;
		renderMesher = renderMesherInstance;
		solidCollisionMesher = solidCollisionMesherInstance;
		bushTriggerMesher = bushTriggerMesherInstance;

		EnsureComponents();
		if (material != null)
		{
			meshRenderer.sharedMaterial = material;
		}

		RebuildMesh();
	}

    public void RebuildMesh()
    {
        if (Chunk == null || (renderMesher == null && solidCollisionMesher == null && bushTriggerMesher == null))
        {
            return;
        }

        if (renderMesher != null)
        {
            meshFilter.sharedMesh = renderMesher.BuildMesh(Chunk);
        }

        if (solidCollisionMesher != null)
        {
            var solidMeshes = solidCollisionMesher.BuildSolidMeshes(Chunk);
            ApplySolidMeshes(solidMeshes);
        }
        else
        {
            ApplySolidMeshes(null);
        }

        if (bushTriggerMesher != null)
        {
            var bushMeshes = bushTriggerMesher.BuildBushTriggerMeshes(Chunk);
            ApplyBushMeshes(bushMeshes);
        }
        else
        {
            ApplyBushMeshes(null);
        }
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

        MeshCollider[] rootColliders = GetComponents<MeshCollider>();
        for (int i = 0; i < rootColliders.Length; i++)
        {
            rootColliders[i].enabled = false;
        }

        // Solid colliders are created per-island on demand.

        Transform legacyBush = transform.Find("bushCollider");
        if (legacyBush != null)
        {
            Destroy(legacyBush.gameObject);
        }

        Transform legacySolid = transform.Find("SolidCollider");
        if (legacySolid != null)
        {
            Destroy(legacySolid.gameObject);
        }

        // Bush colliders are created per-island on demand.
    }

    private static void ApplyCollider(MeshCollider collider, MeshFilter filter, Mesh mesh, bool isbush)
    {
        if (collider == null)
        {
            return;
        }

        if (isbush)
        {
            collider.convex = true;
            collider.isTrigger = true;
        }
        else
        {
            collider.isTrigger = false;
            collider.convex = false;
        }

        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }

        if (mesh == null || mesh.vertexCount == 0)
        {
            collider.sharedMesh = null;
            return;
        }

        collider.sharedMesh = mesh;
    }

    private Transform EnsureChild(string name, Transform existing)
    {
        if (existing != null)
        {
            return existing;
        }

        Transform child = transform.Find(name);
        if (child != null)
        {
            return child;
        }

        var childObject = new GameObject(name);
        childObject.transform.SetParent(transform, false);
        return childObject.transform;
    }

    private static MeshCollider EnsureChildCollider(Transform root)
    {
        if (root == null)
        {
            return null;
        }

        MeshCollider collider = root.GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = root.gameObject.AddComponent<MeshCollider>();
        }

        return collider;
    }

    private static MeshFilter EnsureChildFilter(Transform root)
    {
        if (root == null)
        {
            return null;
        }

        MeshFilter filter = root.GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = root.gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer renderer = root.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = root.gameObject.AddComponent<MeshRenderer>();
        }
        renderer.enabled = false;

        return filter;
    }

    private void ApplyBushMeshes(System.Collections.Generic.List<Mesh> meshes)
    {
        int desiredCount = meshes == null ? 0 : meshes.Count;
        if (desiredCount == 0)
        {
            CleanupBushChildren();
        }
        while (bushRoots.Count > desiredCount)
        {
            int last = bushRoots.Count - 1;
            Transform root = bushRoots[last];
            if (root != null)
            {
                Destroy(root.gameObject);
            }
            bushRoots.RemoveAt(last);
            bushColliders.RemoveAt(last);
            bushFilters.RemoveAt(last);
        }

        for (int i = 0; i < desiredCount; i++)
        {
            EnsureBushSlot(i);
            ApplyCollider(bushColliders[i], bushFilters[i], meshes[i], true);
        }
    }

    private void ApplySolidMeshes(System.Collections.Generic.List<Mesh> meshes)
    {
        int desiredCount = meshes == null ? 0 : meshes.Count;
        if (desiredCount == 0)
        {
            CleanupSolidChildren();
        }

        while (solidRoots.Count > desiredCount)
        {
            int last = solidRoots.Count - 1;
            Transform root = solidRoots[last];
            if (root != null)
            {
                Destroy(root.gameObject);
            }
            solidRoots.RemoveAt(last);
            solidColliders.RemoveAt(last);
            solidFilters.RemoveAt(last);
        }

        for (int i = 0; i < desiredCount; i++)
        {
            EnsureSolidSlot(i);
            ApplyCollider(solidColliders[i], solidFilters[i], meshes[i], false);
        }
    }

    private void EnsureSolidSlot(int index)
    {
        while (solidRoots.Count <= index)
        {
            int slot = solidRoots.Count;
            string name = "SolidCollider " + slot;
            Transform existing = transform.Find(name);
            Transform root = existing != null ? existing : new GameObject(name).transform;
            root.SetParent(transform, false);

            MeshCollider collider = EnsureChildCollider(root);
            MeshFilter filter = EnsureChildFilter(root);

            solidRoots.Add(root);
            solidColliders.Add(collider);
            solidFilters.Add(filter);
        }
    }

    private void CleanupSolidChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name.StartsWith("SolidCollider "))
            {
                Destroy(child.gameObject);
            }
        }
        solidRoots.Clear();
        solidColliders.Clear();
        solidFilters.Clear();
    }

    private void CleanupBushChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name.StartsWith("BushCollider "))
            {
                Destroy(child.gameObject);
            }
        }
        bushRoots.Clear();
        bushColliders.Clear();
        bushFilters.Clear();
    }

    private void EnsureBushSlot(int index)
    {
        while (bushRoots.Count <= index)
        {
            int slot = bushRoots.Count;
            string name = "BushCollider " + slot;
            Transform existing = transform.Find(name);
            Transform root = existing != null ? existing : new GameObject(name).transform;
            root.SetParent(transform, false);

            MeshCollider collider = EnsureChildCollider(root);
            MeshFilter filter = EnsureChildFilter(root);

            bushRoots.Add(root);
            bushColliders.Add(collider);
            bushFilters.Add(filter);
        }
    }
}
