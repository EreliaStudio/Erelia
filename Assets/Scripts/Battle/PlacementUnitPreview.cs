using UnityEngine;
using UnityEngine.Rendering;

public sealed class PlacementUnitPreview
{
	private readonly BoardData boardData;
	private readonly GameObject root;

	public PlacementUnitPreview(GameObject p_modelPrefab, Transform p_parent, BoardData p_boardData)
	{
		boardData = p_boardData;
		root = new GameObject("PlacementPreview");
		root.transform.SetParent(p_parent, false);

		if (p_modelPrefab != null)
		{
			GameObject modelInstance = Object.Instantiate(p_modelPrefab, root.transform, false);
			ApplyTransparency(modelInstance);
		}

		root.SetActive(false);
	}

	public void Hide()
	{
		Logger.LogDebug("[PlacementUnitPreview] Hide — hovered cell is outside player placement area.");
		root.SetActive(false);
	}

	public void UpdatePosition(Vector3Int? p_cell)
	{
		if (!p_cell.HasValue)
		{
			return;
		}

		if (boardData?.Terrain?.VoxelRegistry == null)
		{
			Logger.LogDebug("[PlacementUnitPreview] UpdatePosition skipped — boardData or VoxelRegistry is null.");
			return;
		}

		if (!VoxelTraversalUtility.TryGetWorldHeight(boardData.Terrain, p_cell.Value, CardinalHeightSet.Direction.Stationary, boardData.Terrain.VoxelRegistry, out float height))
		{
			Logger.LogDebug($"[PlacementUnitPreview] UpdatePosition skipped — TryGetWorldHeight failed for cell {p_cell.Value}.");
			return;
		}

		Vector3Int anchor = boardData.WorldAnchor;
		Vector3 worldPos = new(
			anchor.x + p_cell.Value.x + 0.5f,
			height,
			anchor.z + p_cell.Value.z + 0.5f);
		root.transform.position = worldPos;
		root.SetActive(true);
		Logger.LogDebug($"[PlacementUnitPreview] Moved to cell {p_cell.Value} → world {worldPos}.");
	}

	public void Dispose()
	{
		if (root == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			Object.Destroy(root);
		}
		else
		{
			Object.DestroyImmediate(root);
		}
	}

	private static void ApplyTransparency(GameObject p_instance)
	{
		foreach (Renderer renderer in p_instance.GetComponentsInChildren<Renderer>(true))
		{
			Material[] sharedMats = renderer.sharedMaterials;
			Material[] instanceMats = new Material[sharedMats.Length];
			for (int i = 0; i < sharedMats.Length; i++)
			{
				instanceMats[i] = new Material(sharedMats[i]);
				SetMaterialTransparent(instanceMats[i], 0.5f);
			}

			renderer.materials = instanceMats;
		}
	}

	private static void SetMaterialTransparent(Material p_material, float p_alpha)
	{
		if (p_material.HasProperty("_BaseColor"))
		{
			Color color = p_material.GetColor("_BaseColor");
			color.a = p_alpha;
			p_material.SetColor("_BaseColor", color);
			if (p_material.HasProperty("_Surface"))
			{
				p_material.SetFloat("_Surface", 1f);
			}

			p_material.renderQueue = (int)RenderQueue.Transparent;
			return;
		}

		if (p_material.HasProperty("_Color"))
		{
			Color color = p_material.GetColor("_Color");
			color.a = p_alpha;
			p_material.SetColor("_Color", color);
			p_material.SetFloat("_Mode", 3f);
			p_material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
			p_material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
			p_material.SetInt("_ZWrite", 0);
			p_material.DisableKeyword("_ALPHATEST_ON");
			p_material.EnableKeyword("_ALPHABLEND_ON");
			p_material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			p_material.renderQueue = (int)RenderQueue.Transparent;
		}
	}
}
