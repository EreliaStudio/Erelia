using System;
using System.Collections.Generic;
using Erelia.Core;
using UnityEngine;




namespace Erelia.Battle.Phase.Placement
{
	/// <summary>
	/// Placement phase that applies precomputed placement masks and handles unit placement.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private GameObject hudRoot = null;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Placement;

		/// <summary>
		/// Presenter used to access the battle board.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		/// <summary>
		/// Enters the placement phase and applies placement masks.
		/// </summary>
		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			if (hudRoot == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] HUD root can't be empty");
			}

			if (hudRoot != null)
			{
				hudRoot.SetActive(true);
				PopulateCreatureCards();
			}

			InitializePlacementMaskCells();
		}

		/// <summary>
		/// Exits the placement phase and clears placement masks.
		/// </summary>
		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			ClearPlacementMaskCells();

			if (hudRoot != null)
			{
				hudRoot.SetActive(false);
			}
		}

		/// <summary>
		/// Ticks the placement phase until masks are applied.
		/// </summary>
		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
		}

		/// <summary>
		/// Handles confirm input during placement.
		/// </summary>
		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			//1) check si c'est bien une case avec un mask de palcement
			//2) Check si il y a une creature ici -> rejet
			//3) Check si la creature est deja placée -> Bouge
			//4) Creation de la creature "dans el monde" et tu la bouges la ou il faut
		}

		/// <summary>
		/// Handles cancel input during placement.
		/// </summary>
		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
		}

		private void InitializePlacementMaskCells()
		{
			GetAcceptableCoordinates(out Erelia.Battle.Board.Model board, out IReadOnlyList<Vector3Int> acceptableCoordinates);

			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (!IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
				{
					continue;
				}

				if (!IsInPlacementPolicy(definition, coordinate.x, coordinate.y, coordinate.z) ||
					!HasAvailableSpace(board, coordinate.x, coordinate.y, coordinate.z))
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void ClearPlacementMaskCells()
		{
			GetAcceptableCoordinates(out Erelia.Battle.Board.Model board, out IReadOnlyList<Vector3Int> acceptableCoordinates);

			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (!IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void GetAcceptableCoordinates(
			out Erelia.Battle.Board.Model board,
			out IReadOnlyList<Vector3Int> acceptableCoordinates)
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			board = battleData.Board;
			acceptableCoordinates = battleData.PhaseInfo.AcceptableCoordinates;
		}

		private void PopulateCreatureCards()
		{
			if (hudRoot == null)
			{
				return;
			}

			Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardElement[] cardElements =
				hudRoot.GetComponentsInChildren<Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardElement>(true);
			if (cardElements == null || cardElements.Length == 0)
			{
				return;
			}

			Array.Sort(cardElements, CompareCardsBySiblingIndex);

			Erelia.Core.Creature.Instance.Model[] slots = Context.Instance.SystemData?.PlayerTeam?.Slots;
			for (int i = 0; i < cardElements.Length; i++)
			{
				Erelia.Core.Creature.Instance.Model creature =
					slots != null && i < slots.Length ? slots[i] : null;
				cardElements[i].LinkCreature(creature);
			}
		}

		private static int CompareCardsBySiblingIndex(
			Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardElement left,
			Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardElement right)
		{
			if (ReferenceEquals(left, right))
			{
				return 0;
			}

			if (left == null)
			{
				return 1;
			}

			if (right == null)
			{
				return -1;
			}

			return left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex());
		}

		private bool IsInPlacementPolicy(Erelia.Core.VoxelKit.Definition definition, int x, int y, int z)
		{
			return z < Context.Instance.BattleData.Board.SizeZ / 2;
		}

		private bool HasAvailableSpace(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			const int PlayerHeight = 2;
			for (int deltaY = 1; deltaY < PlayerHeight; deltaY++)
			{
				int targetY = y + deltaY;
				if (targetY >= board.SizeY)
				{
					return false;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[x, targetY, z];
				if (Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition) &&
					definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle)
				{
					return false;
				}
			}

			return true;
		}

		private bool IsInsideBoard(Erelia.Battle.Board.Model board, Vector3Int coordinate)
		{
			return coordinate.x >= 0 && coordinate.x < board.SizeX &&
				coordinate.y >= 0 && coordinate.y < board.SizeY &&
				coordinate.z >= 0 && coordinate.z < board.SizeZ;
		}
	}
}
