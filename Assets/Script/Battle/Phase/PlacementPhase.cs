namespace Battle.Phase
{
	class PlacementPhase : Battle.Phase.AbstractPhase
	{
		public override void OnEnter()
		{
			ClearPlacementMask();
			ApplyPlacementMask();
			Utils.ServiceLocator.Instance.BattleBoardService.Data.ValidateMask();
		}

		public override void OnUpdate()
		{

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

		private int FindTopmostSolidY(Battle.Board.Model.Data data, int x, int z)
		{
			for (int y = data.SizeY - 1; y >= 0; y--)
			{
				Core.Voxel.Model.Cell cell = data.Cells[x, y, z];
				if (cell == null)
				{
					continue;
				}

				if (cell.Id != Core.Voxel.Service.AirID)
				{
					return y;
				}
			}

			return -1;
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
