using System;

namespace Battle.Board
{
	public class Service
	{
		public event Action<Battle.Board.Model.Data> DataUpdated;

		private Battle.Board.Model.Data data = null;
		public Battle.Board.Model.Data Data => data;

		public Service()
		{
			
		}

		public void SetData(Voxel.Model.Cell [,,] cells)
		{
			data = new Battle.Board.Model.Data(cells);
			EmitBoardUpdate();
		}

		public void EmitBoardUpdate()
		{
			DataUpdated?.Invoke(data);
		}
	}
}
