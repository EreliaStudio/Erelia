using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class BattleBoardView : MonoBehaviour
{
    [SerializeField] private Material renderMaterial = null;
    [SerializeField] private bool buildSolidColliders = true;

    private readonly VoxelRenderMeshBuilder renderMesher = new VoxelRenderMeshBuilder();
    private readonly VoxelSolidCollisionMeshBuilder solidMesher = new VoxelSolidCollisionMeshBuilder();
    private readonly List<MeshCollider> solidColliders = new List<MeshCollider>();
    private readonly List<MeshFilter> solidFilters = new List<MeshFilter>();
    private readonly List<Transform> solidRoots = new List<Transform>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        EnsureComponents();
        if (renderMaterial != null)
        {
            meshRenderer.sharedMaterial = renderMaterial;
        }
    }

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null || request.BattleBoard == null)
        {
            return;
        }

        Build(request.BattleBoard, request.Registry);
    }

    public void Build(BattleBoard board, VoxelRegistry registry)
    {
        if (board == null)
        {
            return;
        }

        if (registry == null)
        {
            Debug.LogWarning("BattleBoardView: Missing VoxelRegistry, cannot build mesh.");
            return;
        }

        renderMesher.SetRegistry(registry);
        solidMesher.SetRegistry(registry);

        transform.position = board.OriginCell;
        Mesh mesh = renderMesher.BuildMesh(board.Voxels, board.SizeX, board.SizeY, board.SizeZ);
        mesh.name = "BattleBoardRenderMesh";
        meshFilter.sharedMesh = mesh;

        if (buildSolidColliders)
        {
            List<Mesh> solidMeshes = solidMesher.BuildSolidMeshes(board.Voxels, board.SizeX, board.SizeY, board.SizeZ);
            ApplySolidMeshes(solidMeshes);
        }
        else
        {
            ApplySolidMeshes(null);
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
    }

    private void ApplySolidMeshes(List<Mesh> meshes)
    {
        int desiredCount = meshes == null ? 0 : meshes.Count;
        if (desiredCount == 0)
        {
            CleanupSolidChildren();
            return;
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
            Mesh mesh = meshes[i];
            solidFilters[i].sharedMesh = mesh;
            solidColliders[i].sharedMesh = mesh;
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

            MeshCollider collider = root.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = root.gameObject.AddComponent<MeshCollider>();
            }
            collider.isTrigger = false;
            collider.convex = false;

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
}
