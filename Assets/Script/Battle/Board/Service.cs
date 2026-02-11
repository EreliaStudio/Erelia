namespace Battle.Board
{
	public class Service
	{
		private Battle.Board.Model.Data data;
		public bool HasData => data != null;

		public void Init()
		{
			
		}

		public void Setup(Voxel.Model.Cell [,,] cells)
		{
			data = new Battle.Board.Model.Data(cells);
		}
	}
}
