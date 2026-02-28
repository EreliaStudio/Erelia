namespace Erelia.Exploration
{
	[System.Serializable]
	public sealed class Data
	{
		public Erelia.Exploration.World.Model WorldModel;
		public Erelia.Exploration.Player.Model PlayerModel;

		public Data()
		{
			WorldModel = new Erelia.Exploration.World.Model();
			PlayerModel = new Erelia.Exploration.Player.Model();
		}
	}
}
