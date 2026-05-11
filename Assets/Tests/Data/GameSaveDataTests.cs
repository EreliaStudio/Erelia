using NUnit.Framework;
using Tests.Persistence;
using UnityEngine;

namespace Tests.Data
{
	public sealed class GameSaveDataTests
	{
		[Test]
		public void SetPlayerWorldCell_StoresCenteredPlayerPositionAndCell()
		{
			GameSaveData saveData = new GameSaveData();

			saveData.SetPlayerWorldCell(new Vector3Int(10, 2, -3));

			Assert.That(saveData.PlayerWorldCell, Is.EqualTo(new Vector3Int(10, 2, -3)));
			Assert.That(saveData.Player.Position.Value, Is.EqualTo(new Vector3(10.5f, 2f, -2.5f)));
		}

		[Test]
		public void CopyPlayerFrom_WithTeamAndStorage_CopiesSaveRelevantPlayerState()
		{
			CreatureSpecies species = SaveTestDataFactory.CreateSpecies();
			PlayerData source = new PlayerData();
			source.SetPosition(SaveTestDataFactory.PlayerPosition, true);
			source.Team[0] = SaveTestDataFactory.CreateCreature(species, SaveTestDataFactory.DpsFormId);
			source.CreatureStorage.Add(SaveTestDataFactory.CreateCreature(species, SaveTestDataFactory.DefaultFormId));

			GameSaveData saveData = new GameSaveData();
			saveData.CopyPlayerFrom(source);

			Assert.That(saveData.Player.Position.Value, Is.EqualTo(SaveTestDataFactory.PlayerPosition));
			Assert.That(saveData.Player.Team, Has.Length.EqualTo(GameRule.TeamMemberCount));
			Assert.That(saveData.Player.Team[0], Is.SameAs(source.Team[0]));
			Assert.That(saveData.Player.CreatureStorage.Count, Is.EqualTo(1));
			Assert.That(saveData.Player.CreatureStorage.GetAt(0), Is.SameAs(source.CreatureStorage.GetAt(0)));
		}

		[Test]
		public void CopyPlayerFrom_NullSource_ResetsPlayerState()
		{
			GameSaveData saveData = new GameSaveData();
			saveData.SetPlayerWorldCell(new Vector3Int(10, 2, -3));
			saveData.Player.Team[0] = new CreatureUnit();

			saveData.CopyPlayerFrom(null);

			Assert.That(saveData.Player.Position.Value, Is.EqualTo(Vector3.zero));
			Assert.That(saveData.Player.Team, Has.Length.EqualTo(GameRule.TeamMemberCount));
			Assert.That(saveData.Player.Team[0], Is.Null);
			Assert.That(saveData.Player.CreatureStorage.Count, Is.EqualTo(0));
		}
	}
}
