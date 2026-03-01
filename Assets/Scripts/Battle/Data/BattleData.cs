namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class Data
	{
		public Erelia.Battle.Board.Model Board;
		public Erelia.Battle.Info Info;
		public Erelia.Core.Encounter.EncounterTable EncounterTable;

		public Data()
		{
			Board = null;
			EncounterTable = null;
			Info = new Erelia.Battle.Info();
		}
	}
}
