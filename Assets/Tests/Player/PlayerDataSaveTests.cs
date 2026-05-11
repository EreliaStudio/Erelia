using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Persistence;
using UnityEngine;

namespace Tests.Player
{
	public sealed class PlayerDataSaveTests
	{
		private ReferenceRegistry registry;
		private CreatureSpecies species;

		[SetUp]
		public void SetUp()
		{
			registry = new ReferenceRegistry();
			species = SaveTestDataFactory.CreateSpecies();
			registry.Bind(new[] { species });
		}

		[Test]
		public void ToJson_WithPositionTeamAndStorage_MatchesExpectedJson()
		{
			PlayerData playerData = new PlayerData();
			playerData.SetPosition(SaveTestDataFactory.PlayerPosition, true);
			playerData.Team[0] = SaveTestDataFactory.CreateCreature(
				species,
				SaveTestDataFactory.DpsFormId,
				p_includeDamageProgress: true);
			playerData.Team[3] = SaveTestDataFactory.CreateCreature(
				species,
				SaveTestDataFactory.DefaultFormId);
			playerData.CreatureStorage.Add(SaveTestDataFactory.CreateCreature(
				species,
				SaveTestDataFactory.DpsFormId));

			JObject json = playerData.ToJson(registry);

			SaveTestDataFactory.AssertJsonEquals(CreateExpectedPlayerJson(), json);
		}

		[Test]
		public void LoadFromJson_WithPlayerPayload_RestoresPositionTeamAndStorage()
		{
			PlayerData playerData = new PlayerData();
			JObject json = CreateExpectedPlayerJson();

			playerData.LoadFromJson(json, registry);

			Assert.That(playerData.Position.Value, Is.EqualTo(SaveTestDataFactory.PlayerPosition));
			Assert.That(playerData.Team, Has.Length.EqualTo(GameRule.TeamMemberCount));
			Assert.That(playerData.Team[0], Is.Not.Null);
			Assert.That(playerData.Team[0].Species, Is.SameAs(species));
			Assert.That(playerData.Team[0].CurrentFormID, Is.EqualTo(SaveTestDataFactory.DpsFormId));
			Assert.That(playerData.Team[0].FeatBoardProgress.FindProgress(SaveTestDataFactory.DamageNodeId).CompletionCount, Is.EqualTo(2));
			Assert.That(playerData.Team[1], Is.Null);
			Assert.That(playerData.Team[3], Is.Not.Null);
			Assert.That(playerData.Team[3].CurrentFormID, Is.EqualTo(SaveTestDataFactory.DefaultFormId));
			Assert.That(playerData.CreatureStorage.Count, Is.EqualTo(1));
			Assert.That(playerData.CreatureStorage.GetAt(0).CurrentFormID, Is.EqualTo(SaveTestDataFactory.DpsFormId));

			SaveTestDataFactory.AssertJsonEquals(json, playerData.ToJson(registry));
		}

		[Test]
		public void LoadFromJson_WithMoreThanSixTeamSlots_IgnoresOverflowSlots()
		{
			JObject overflowCreature = SaveTestDataFactory.ExpectedCreatureJson(
				p_formId: SaveTestDataFactory.DefaultFormId);
			JObject json = CreateExpectedPlayerJson();
			JArray team = json["team"] as JArray;
			team.Add(SaveTestDataFactory.CreatureSlot(overflowCreature));

			PlayerData playerData = new PlayerData();
			playerData.LoadFromJson(json, registry);

			Assert.That(playerData.Team, Has.Length.EqualTo(GameRule.TeamMemberCount));
			Assert.That(playerData.Team[GameRule.TeamMemberCount - 1], Is.Null);
			Assert.That(playerData.CreatureStorage.Count, Is.EqualTo(1));
		}

		private static JObject CreateExpectedPlayerJson()
		{
			JArray team = SaveTestDataFactory.EmptyTeamSlots();
			team[0] = SaveTestDataFactory.CreatureSlot(
				SaveTestDataFactory.ExpectedCreatureJson(p_includeDamageProgress: true));
			team[3] = SaveTestDataFactory.CreatureSlot(
				SaveTestDataFactory.ExpectedCreatureJson(p_formId: SaveTestDataFactory.DefaultFormId));

			JArray storage = new JArray
			{
				SaveTestDataFactory.ExpectedCreatureJson()
			};

			return SaveTestDataFactory.ExpectedPlayerJson(team, storage);
		}
	}
}
