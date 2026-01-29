using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class BattleBoardView
{
    [SerializeField] private Material renderMaterial = null;
    [SerializeField] private Material maskMaterial = null;

    private readonly VoxelRenderMeshBuilder renderMesher = new VoxelRenderMeshBuilder();
    private readonly VoxelSolidCollisionMeshBuilder solidMesher = new VoxelSolidCollisionMeshBuilder();
    [SerializeField] private BattleCellRenderMeshBuilder maskRenderMesher;
    private BattleCellCollisionMeshBuilder maskCollisionMesher;
    private readonly List<MeshCollider> solidColliders = new List<MeshCollider>();
    private readonly List<MeshFilter> solidFilters = new List<MeshFilter>();
    private readonly List<Transform> solidRoots = new List<Transform>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Transform maskRoot;
    private MeshFilter maskFilter;
    private MeshRenderer maskRenderer;
    private MeshCollider maskCollider;
    private Rigidbody maskRigidbody;
    private Material maskMaterialInstance;
    private Mesh maskRenderMesh;
    private Mesh maskCollisionMesh;
    private BattleBoardData currentBoard;
    private Transform owner;

    public void Initialize(BattleBoardData board, VoxelRegistry registry, Transform ownerTransform)
    {
        owner = ownerTransform;
        if (maskRenderMesher == null)
        {
            maskRenderMesher = new BattleCellRenderMeshBuilder();
        }

        if (maskCollisionMesher == null)
        {
            maskCollisionMesher = new BattleCellCollisionMeshBuilder();
        }

        EnsureComponents();
        if (renderMaterial != null)
        {
            meshRenderer.sharedMaterial = renderMaterial;
        }
    }

    public void Cleanup()
    {
        DestroyRuntimeAssets();
    }

    public void Build(BattleBoardData board, VoxelRegistry registry)
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

        if (owner == null)
        {
            Debug.LogWarning("BattleBoardView: Missing owner transform, cannot build mesh.");
            return;
        }

        EnsureComponents();
        currentBoard = board;
        renderMesher.SetRegistry(registry);
        solidMesher.SetRegistry(registry);

        owner.position = board.OriginCell;
        Mesh mesh = renderMesher.BuildMesh(board.Voxels, board.SizeX, board.SizeY, board.SizeZ);
        mesh.name = "BattleBoardRenderMesh";
        meshFilter.sharedMesh = mesh;

        List<Mesh> solidMeshes = solidMesher.BuildSolidMeshes(board.Voxels, board.SizeX, board.SizeY, board.SizeZ);
        ApplySolidMeshes(solidMeshes);

        BuildMaskOverlay(board);
    }

    public void RebuildMask()
    {
        if (currentBoard == null)
        {
            return;
        }

        BuildMaskOverlay(currentBoard);
    }

    public void ConfigureMaskBuilders(
        BattleCellRenderMeshBuilder renderBuilder,
        BattleCellCollisionMeshBuilder collisionBuilder)
    {
        if (renderBuilder != null)
        {
            maskRenderMesher = renderBuilder;
        }

        if (collisionBuilder != null)
        {
            maskCollisionMesher = collisionBuilder;
        }

    }

    private void EnsureComponents()
    {
        if (owner == null)
        {
            return;
        }

        GameObject ownerObject = owner.gameObject;
        if (meshFilter == null)
        {
            meshFilter = ownerObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = ownerObject.AddComponent<MeshFilter>();
                meshFilter.name = "BattleBoard MeshFilter";
            }
        }

        if (meshRenderer == null)
        {
            meshRenderer = ownerObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = ownerObject.AddComponent<MeshRenderer>();
                meshRenderer.name = "BattleBoard MeshRenderer";
            }
        }
    }

    private void BuildMaskOverlay(BattleBoardData board)
    {
        if (board == null || maskRenderMesher == null)
        {
            return;
        }

        if (owner == null)
        {
            return;
        }

        EnsureMaskComponents();
        if (maskFilter == null || maskRenderer == null)
        {
            return;
        }

        if (maskRenderMesh != null)
        {
            UnityEngine.Object.Destroy(maskRenderMesh);
        }
        if (maskCollisionMesh != null)
        {
            UnityEngine.Object.Destroy(maskCollisionMesh);
        }

        maskRenderMesh = maskRenderMesher.BuildMesh(board);
        maskFilter.sharedMesh = maskRenderMesh;

        if (maskCollider != null)
        {
            maskCollisionMesh = maskCollisionMesher.BuildMesh(board, maskRenderMesher.Mappings);
            maskCollider.sharedMesh = maskCollisionMesh;
            maskCollider.isTrigger = true;
            maskCollider.convex = false;
        }
    }

    private void EnsureMaskComponents()
    {
        if (maskRoot == null)
        {
            Transform existing = owner.Find("MaskOverlay");
            maskRoot = existing != null ? existing : new GameObject("MaskOverlay").transform;
            maskRoot.SetParent(owner, false);
        }

        if (maskFilter == null)
        {
            maskFilter = maskRoot.GetComponent<MeshFilter>();
            if (maskFilter == null)
            {
                maskFilter = maskRoot.gameObject.AddComponent<MeshFilter>();
            }
        }

        if (maskRenderer == null)
        {
            maskRenderer = maskRoot.GetComponent<MeshRenderer>();
            if (maskRenderer == null)
            {
                maskRenderer = maskRoot.gameObject.AddComponent<MeshRenderer>();
            }
        }

        if (maskMaterial != null)
        {
            if (maskMaterialInstance == null)
            {
                maskMaterialInstance = new Material(maskMaterial);
                maskMaterialInstance.name = $"{maskMaterial.name} (MaskOverlay)";
            }
            maskRenderer.sharedMaterial = maskMaterialInstance;
        }

        if (maskRenderer.sharedMaterial != null)
        {
            Texture2D texture = ResolveMaskTexture();
            if (texture != null)
            {
                maskRenderer.sharedMaterial.mainTexture = texture;
            }
        }

        if (maskCollider == null)
        {
            maskCollider = maskRoot.GetComponent<MeshCollider>();
            if (maskCollider == null)
            {
                maskCollider = maskRoot.gameObject.AddComponent<MeshCollider>();
            }
        }

        if (maskRigidbody == null)
        {
            maskRigidbody = maskRoot.GetComponent<Rigidbody>();
            if (maskRigidbody == null)
            {
                maskRigidbody = maskRoot.gameObject.AddComponent<Rigidbody>();
            }
            maskRigidbody.isKinematic = true;
            maskRigidbody.useGravity = false;
        }
    }

    private Texture2D ResolveMaskTexture()
    {
        IReadOnlyList<BattleMaskSpriteMapping> mappings = maskRenderMesher != null ? maskRenderMesher.Mappings : null;
        if (mappings == null)
        {
            return null;
        }

        for (int i = 0; i < mappings.Count; i++)
        {
            Sprite sprite = mappings[i].Sprite;
            if (sprite != null && sprite.texture != null)
            {
                return sprite.texture;
            }
        }

        return null;
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
                UnityEngine.Object.Destroy(root.gameObject);
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
            Transform existing = owner.Find(name);
            Transform root = existing != null ? existing : new GameObject(name).transform;
            root.SetParent(owner, false);

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
        if (owner == null)
        {
            return;
        }

        for (int i = owner.childCount - 1; i >= 0; i--)
        {
            Transform child = owner.GetChild(i);
            if (child != null && child.name.StartsWith("SolidCollider "))
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
        solidRoots.Clear();
        solidColliders.Clear();
        solidFilters.Clear();
    }

    private void DestroyRuntimeAssets()
    {
        if (maskRenderMesh != null)
        {
            UnityEngine.Object.Destroy(maskRenderMesh);
            maskRenderMesh = null;
        }

        if (maskCollisionMesh != null)
        {
            UnityEngine.Object.Destroy(maskCollisionMesh);
            maskCollisionMesh = null;
        }

        if (maskMaterialInstance != null)
        {
            UnityEngine.Object.Destroy(maskMaterialInstance);
            maskMaterialInstance = null;
        }
    }
}
