using Battle.Board.Controller;
using UnityEngine;
using Utils;
using UnityEngine.InputSystem;


namespace Battle.Phase
{
	class PlacementPhase : Battle.Phase.AbstractPhase
	{
		protected Battle.Player.Controller.BattleController playerController;
		public Battle.Player.Controller.BattleController PlayerController => playerController;
		private Battle.Context.Model.TeamPlacement playerPlacement;
		private int activeSlotIndex;

		public override void Configure(GameObject playerObject)
		{
			playerController = playerObject.GetComponent<Battle.Player.Controller.BattleController>();
		}

		public override void OnEnter()
		{
			InitializePlacementData();
			activeSlotIndex = FindNextUnplacedSlot(0);
			ClearPlacementMask();
			ApplyPlacementMask();
			Utils.ServiceLocator.Instance.BattleBoardService.Data.ValidateMask();
		}

		public override void OnUpdate()
		{
			if (playerController == null || playerPlacement == null)
			{
				return;
			}

			Mouse mouse = Mouse.current;
			if (mouse != null && mouse.leftButton.wasPressedThisFrame)
			{
				TryPlaceActiveAtSelectedCell();
			}
		}

		public override void OnExit()
		{
			ClearPlacementMask();
			Utils.ServiceLocator.Instance.BattleBoardService.Data.ValidateMask();
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
				contextService.InitializeFromPlayerTeam(Utils.ServiceLocator.Instance.PlayerService.Team);
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
			return true;
		}

		public bool TryPlaceActiveAtSelectedCell()
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
			if (!IsPlacementCell(cell))
			{
				return false;
			}

			if (!playerPlacement.TryPlace(activeSlotIndex, cell))
			{
				return false;
			}

			activeSlotIndex = FindNextUnplacedSlot(activeSlotIndex + 1);
			return true;
		}

		private int FindNextUnplacedSlot(int startIndex)
		{
			if (playerPlacement == null || playerPlacement.SlotCount == 0)
			{
				return 0;
			}

			int slotCount = playerPlacement.SlotCount;
			for (int offset = 0; offset < slotCount; offset++)
			{
				int index = (startIndex + offset) % slotCount;
				if (!playerPlacement.Instances[index].HasPlacement)
				{
					return index;
				}
			}

			return 0;
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
	}
}
