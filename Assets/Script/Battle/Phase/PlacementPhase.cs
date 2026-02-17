using System;
using System.Collections.Generic;
using Battle.Board.Controller;
using UnityEngine;
using Utils;
using UnityEngine.InputSystem;
using UI.Battle.Placement;


namespace Battle.Phase
{
	public class PlacementPhase : Battle.Phase.AbstractPhase
	{
		private const int UnselectedSlotIndex = -1;
		protected Battle.Player.Controller.BattleController playerController;
		public Battle.Player.Controller.BattleController PlayerController => playerController;
		private Battle.Context.Model.TeamPlacement playerPlacement;
		private int activeSlotIndex;
		private TeamPlacementPanel teamPlacementPanel;
		private GameObject placementPhaseHud;
		private Transform placementRoot;
		private readonly Dictionary<int, GameObject> placementCreatures = new Dictionary<int, GameObject>();
		private readonly Core.Voxel.Model.CardinalPointSetByOrientationCollection entryPointCache = new Core.Voxel.Model.CardinalPointSetByOrientationCollection();

		public event Action<int> ActiveSlotChanged;
		public event Action PlacementChanged;
		public event Action PlacementValidated;

		public int ActiveSlotIndex => activeSlotIndex;
		public Battle.Context.Model.TeamPlacement PlayerPlacement => playerPlacement;

		public override void Configure(GameObject playerObject)
		{
			playerController = playerObject.GetComponent<Battle.Player.Controller.BattleController>();
			placementPhaseHud = FindHudObject("PlacementPhaseHUD");
			if (placementPhaseHud == null)
			{
				Debug.LogWarning("PlacementPhase: PlacementPhaseHUD was not found in the scene.");
			}
		}

		public override void OnEnter()
		{
			if (placementPhaseHud != null)
			{
				placementPhaseHud.SetActive(true);
			}

			InitializePlacementData();
			activeSlotIndex = UnselectedSlotIndex;
			ClearPlacementMask();
			ApplyPlacementMask();
			Utils.ServiceLocator.Instance.BattleBoardService.Data.ValidateMask();
			EnsurePlacementRoot();
			SyncAllPlacementCreatures();
			BindTeamPanel();
			ActiveSlotChanged?.Invoke(activeSlotIndex);
		}

		public override void OnUpdate()
		{
			if (playerController == null || playerPlacement == null)
			{
				return;
			}

			Mouse mouse = Mouse.current;
			if (mouse != null && mouse.rightButton.wasPressedThisFrame)
			{
				TryClearAtSelectedCell();
				return;
			}

			if (mouse != null && mouse.leftButton.wasPressedThisFrame)
			{
				TryPlaceActiveAtSelectedCell();
			}
		}

		public override void OnExit()
		{
			ClearPlacementMask();
			Utils.ServiceLocator.Instance.BattleBoardService.Data.ValidateMask();
			ClearPlacementCreatures();
			UnbindTeamPanel();

			if (placementPhaseHud != null)
			{
				placementPhaseHud.SetActive(false);
			}
		}

		private void ApplyPlacementMask()
		{
			Battle.Board.Model.Data data = Utils.ServiceLocator.Instance.BattleBoardService.Data;
			
			int halfZ = data.SizeZ / 2;
			int lowerStartZ = 0;
			int lowerEndZ = halfZ - 1;

			int startZ = lowerStartZ;
			int endZ = lowerEndZ;

			if (startZ > endZ)
			{
				return;
			}

			for (int x = 0; x < data.SizeX; x++)
			{
				for (int z = startZ; z <= endZ; z++)
				{
					int topY = FindTopmostSolidY(data, x, z);
					if (topY < 0)
					{
						continue;
					}

					data.MaskCells[x, topY, z].AddMask(Core.Mask.Model.Value.Placement);
				}
			}
		}

		private void InitializePlacementData()
		{
			Battle.Context.Service contextService = Utils.ServiceLocator.Instance.BattleContextService;
			if (contextService == null)
			{
				playerPlacement = null;
				return;
			}

			if (contextService.PlayerPlacement == null)
			{
				contextService.InitializeFromPlayerTeam(Utils.ServiceLocator.Instance.PlayerTeam);
			}

			playerPlacement = contextService.PlayerPlacement;
		}

		public bool TrySetActiveSlot(int slotIndex)
		{
			if (playerPlacement == null)
			{
				return false;
			}

			if (slotIndex < 0 || slotIndex >= playerPlacement.SlotCount)
			{
				return false;
			}

			activeSlotIndex = slotIndex;
			ActiveSlotChanged?.Invoke(activeSlotIndex);
			return true;
		}

		public bool TryPlaceActiveAtSelectedCell()
		{
			if (playerController == null || playerPlacement == null)
			{
				return false;
			}

			if (activeSlotIndex < 0 || activeSlotIndex >= playerPlacement.SlotCount)
			{
				return false;
			}

			if (!playerController.HasSelectedCell())
			{
				return false;
			}

			Vector3Int cell = playerController.SelectedCell();
			if (!IsPlacementCell(cell))
			{
				return false;
			}

			if (!playerPlacement.TryPlace(activeSlotIndex, cell))
			{
				return false;
			}

			SyncPlacementCreature(activeSlotIndex);
			activeSlotIndex = UnselectedSlotIndex;
			PlacementChanged?.Invoke();
			ActiveSlotChanged?.Invoke(activeSlotIndex);
			return true;
		}

		public bool TryClearAtSelectedCell()
		{
			if (playerController == null || playerPlacement == null)
			{
				return false;
			}

			if (!playerController.HasSelectedCell())
			{
				return false;
			}

			Vector3Int cell = playerController.SelectedCell();
			if (playerPlacement.TryClearAtCell(cell, out int slotIndex))
			{
				RemovePlacementCreature(slotIndex);
				PlacementChanged?.Invoke();
				return true;
			}

			return false;
		}

		public bool TryValidatePlacement()
		{
			if (playerPlacement == null)
			{
				return false;
			}

			if (playerPlacement.PlacedCount <= 0)
			{
				return false;
			}

			PlacementValidated?.Invoke();
			Debug.Log($"Placement validated ({playerPlacement.PlacedCount}/{playerPlacement.MaxPlacements}).");
			return true;
		}

		private bool IsPlacementCell(Vector3Int cell)
		{
			Battle.Board.Model.Data data = Utils.ServiceLocator.Instance.BattleBoardService.Data;

			if (cell.x < 0 || cell.x >= data.SizeX || cell.y < 0 || cell.y >= data.SizeY || cell.z < 0 || cell.z >= data.SizeZ)
			{
				return false;
			}

			return data.MaskCells[cell.x, cell.y, cell.z].HasMask(Core.Mask.Model.Value.Placement);
		}

		private int FindTopmostSolidY(Battle.Board.Model.Data data, int x, int z)
		{
			for (int y = data.SizeY - 1; y >= 0; y--)
			{
				Core.Voxel.Model.Cell cell = data.Cells[x, y, z];
				if (!IsSolidCell(cell))
				{
					continue;
				}

				if (!HasVerticalSpace(data, x, y, z, 2))
				{
					continue;
				}

				return y;
			}

			return -1;
		}

		private bool HasVerticalSpace(Battle.Board.Model.Data data, int x, int y, int z, int required)
		{
			for (int offset = 1; offset <= required; offset++)
			{
				int checkY = y + offset;
				if (checkY >= data.SizeY)
				{
					return false;
				}

				Core.Voxel.Model.Cell cell = data.Cells[x, checkY, z];
				if (!IsAirOrWalkableCell(cell))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsSolidCell(Core.Voxel.Model.Cell cell)
		{
			if (cell == null || cell.Id == Core.Voxel.Service.AirID)
			{
				return false;
			}

			if (!ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Core.Voxel.Model.Definition voxelDefinition))
			{
				return false;
			}

			return voxelDefinition.Data.Traversal == Core.Voxel.Model.Traversal.Obstacle;
		}

		private bool IsAirOrWalkableCell(Core.Voxel.Model.Cell cell)
		{
			if (cell == null || cell.Id == Core.Voxel.Service.AirID)
			{
				return true;
			}

			if (!ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Core.Voxel.Model.Definition voxelDefinition))
			{
				return false;
			}

			Core.Voxel.Model.Traversal traversal = voxelDefinition.Data.Traversal;
			return traversal == Core.Voxel.Model.Traversal.Air || traversal == Core.Voxel.Model.Traversal.Walkable;
		}

		private void ClearPlacementMask()
		{
			Battle.Board.Model.Data data = Utils.ServiceLocator.Instance.BattleBoardService.Data;

			for (int x = 0; x < data.SizeX; x++)
			{
				for (int y = 0; y < data.SizeY; y++)
				{
					for (int z = 0; z < data.SizeZ; z++)
					{
						data.MaskCells[x, y, z].RemoveMask(Core.Mask.Model.Value.Placement);
					}
				}
			}
		}

		private void BindTeamPanel()
		{
			if (teamPlacementPanel != null)
			{
				return;
			}

			teamPlacementPanel = UnityEngine.Object.FindFirstObjectByType<TeamPlacementPanel>();
			if (teamPlacementPanel == null)
			{
				return;
			}

			teamPlacementPanel.Bind(this);
		}

		private void UnbindTeamPanel()
		{
			if (teamPlacementPanel == null)
			{
				return;
			}

			teamPlacementPanel.Unbind();
			teamPlacementPanel = null;
		}

		private void EnsurePlacementRoot()
		{
			if (placementRoot != null)
			{
				return;
			}

			var go = new GameObject("PlacementCreatures");
			if (manager != null)
			{
				go.transform.SetParent(manager.transform, false);
			}

			placementRoot = go.transform;
		}

		private void ClearPlacementCreatures()
		{
			foreach (var pair in placementCreatures)
			{
				if (pair.Value != null)
				{
					UnityEngine.Object.Destroy(pair.Value);
				}
			}

			placementCreatures.Clear();

			if (placementRoot != null)
			{
				UnityEngine.Object.Destroy(placementRoot.gameObject);
				placementRoot = null;
			}
		}

		private void SyncAllPlacementCreatures()
		{
			if (playerPlacement == null)
			{
				return;
			}

			for (int i = 0; i < playerPlacement.SlotCount; i++)
			{
				SyncPlacementCreature(i);
			}
		}

		private void SyncPlacementCreature(int slotIndex)
		{
			if (playerPlacement == null)
			{
				return;
			}

			if (slotIndex < 0 || slotIndex >= playerPlacement.SlotCount)
			{
				return;
			}

			var instance = playerPlacement.Instances[slotIndex];
			if (instance == null || instance.Source == null || instance.Source.SpeciesDefinition == null)
			{
				RemovePlacementCreature(slotIndex);
				return;
			}

			GameObject prefab = instance.Source.SpeciesDefinition.Presenter != null
				? instance.Source.SpeciesDefinition.Presenter.ModelPrefab
				: null;
			if (prefab == null || !instance.HasPlacement)
			{
				RemovePlacementCreature(slotIndex);
				return;
			}

			if (!placementCreatures.TryGetValue(slotIndex, out GameObject creature) || creature == null)
			{
				EnsurePlacementRoot();
				creature = UnityEngine.Object.Instantiate(prefab, placementRoot);
				placementCreatures[slotIndex] = creature;
			}

			creature.transform.position = CellToWorld(instance.Cell);
		}

		private void RemovePlacementCreature(int slotIndex)
		{
			if (!placementCreatures.TryGetValue(slotIndex, out GameObject creature))
			{
				return;
			}

			if (creature != null)
			{
				UnityEngine.Object.Destroy(creature);
			}

			placementCreatures.Remove(slotIndex);
		}

		private Vector3 CellToWorld(Vector3Int cell)
		{
			Battle.Board.Model.Data boardData = Utils.ServiceLocator.Instance.BattleBoardService.Data;
			if (boardData != null &&
				cell.x >= 0 && cell.x < boardData.SizeX &&
				cell.y >= 0 && cell.y < boardData.SizeY &&
				cell.z >= 0 && cell.z < boardData.SizeZ)
			{
				Core.Voxel.Model.Cell voxelCell = boardData.Cells[cell.x, cell.y, cell.z];
				if (voxelCell != null &&
					voxelCell.Id != Core.Voxel.Service.AirID &&
					ServiceLocator.Instance.VoxelService.TryGetDefinition(voxelCell.Id, out Core.Voxel.Model.Definition definition) &&
					definition != null)
				{
					Vector3 origin = new Vector3(0.5f, 1f, 0.5f);
					Core.Voxel.Geometry.Shape shape = definition.Shape;
					if (shape != null && entryPointCache.TryGetValue(shape.CardinalPoints, voxelCell.Orientation, voxelCell.FlipOrientation, out Core.Voxel.Model.CardinalPointSet entryPoints))
					{
						origin = entryPoints.Get(Core.Voxel.Model.CardinalPoint.Stationary);
					}

					return new Vector3(cell.x, cell.y, cell.z) + origin;
				}
			}

			return new Vector3(cell.x + 0.5f, cell.y + 1f, cell.z + 0.5f);
		}

		private GameObject FindHudObject(string objectName)
		{
			if (string.IsNullOrWhiteSpace(objectName))
			{
				return null;
			}

			GameObject found = GameObject.Find(objectName);
			if (found != null)
			{
				return found;
			}

			var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
			for (int i = 0; i < allObjects.Length; i++)
			{
				GameObject candidate = allObjects[i];
				if (candidate == null)
				{
					continue;
				}

				if (!candidate.scene.IsValid())
				{
					continue;
				}

				if (candidate.name == objectName)
				{
					return candidate;
				}
			}

			return null;
		}
	}
}
