using System;

namespace Battle.Board
{
	public class Service
	{
		private Battle.Board.Model.Data data = null;
		public Battle.Board.Model.Data Data => data;

		public Service()
		{
			
		}

		public void SetData(Core.Voxel.Model.Cell [,,] cells)
		{
			data = new Battle.Board.Model.Data(cells);
			data.Validate();
		}

		public void EmitBoardUpdate()
		{
			data.Validate();
		}
	}
}
