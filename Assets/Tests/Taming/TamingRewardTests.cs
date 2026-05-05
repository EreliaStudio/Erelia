using System;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Taming.Rewards
{
	public sealed class TamingRewardTests
	{
		[Test]
		public void AwardWonBattleTamingRewards_PlayerWin_AddsCreatureToEmptyTeamSlot()
		{
			var playerData = new PlayerData();
			CreatureUnit recruit = CreateCreatureUnit();

			BattleOutcome outcome = CreateOutcome(BattleSide.Player, recruit);

			TamingProgressService.AwardWonBattleTamingRewards(playerData, outcome);

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.GreaterThanOrEqualTo(0));
			Assert.That(playerData.CreatureStorage.Count, Is.EqualTo(0));
		}

		[Test]
		public void AwardWonBattleTamingRewards_PlayerLoss_ForfeitsCreature()
		{
			var playerData = new PlayerData();
			CreatureUnit recruit = CreateCreatureUnit();

			BattleOutcome outcome = CreateOutcome(BattleSide.Enemy, recruit);

			TamingProgressService.AwardWonBattleTamingRewards(playerData, outcome);

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.EqualTo(-1));
			Assert.That(playerData.CreatureStorage.Contains(recruit), Is.False);
		}

		[Test]
		public void AwardWonBattleTamingRewards_NeutralOutcome_DoesNotAwardCreature()
		{
			var playerData = new PlayerData();
			CreatureUnit recruit = CreateCreatureUnit();

			BattleOutcome outcome = CreateOutcome(BattleSide.Neutral, recruit);

			TamingProgressService.AwardWonBattleTamingRewards(playerData, outcome);

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.EqualTo(-1));
			Assert.That(playerData.CreatureStorage.Contains(recruit), Is.False);
		}

		[Test]
		public void AwardWonBattleTamingRewards_FullTeam_SendsCreatureToStorage()
		{
			var playerData = new PlayerData();

			for (int index = 0; index < playerData.Team.Length; index++)
			{
				playerData.Team[index] = CreateCreatureUnit();
			}

			CreatureUnit recruit = CreateCreatureUnit();
			BattleOutcome outcome = CreateOutcome(BattleSide.Player, recruit);

			TamingProgressService.AwardWonBattleTamingRewards(playerData, outcome);

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.EqualTo(-1));
			Assert.That(playerData.CreatureStorage.Contains(recruit), Is.True);
			Assert.That(playerData.CreatureStorage.Count, Is.EqualTo(1));
		}

		[Test]
		public void AwardWonBattleTamingRewards_NullPlayerData_DoesNotThrow()
		{
			CreatureUnit recruit = CreateCreatureUnit();
			BattleOutcome outcome = CreateOutcome(BattleSide.Player, recruit);

			Assert.DoesNotThrow(() =>
				TamingProgressService.AwardWonBattleTamingRewards(null, outcome));
		}

		[Test]
		public void AwardWonBattleTamingRewards_NullOutcome_DoesNotThrow()
		{
			var playerData = new PlayerData();

			Assert.DoesNotThrow(() =>
				TamingProgressService.AwardWonBattleTamingRewards(playerData, null));
		}

		private static BattleOutcome CreateOutcome(BattleSide p_winner, params CreatureUnit[] p_impressedCreatures)
		{
			return new BattleOutcome(
				p_winner,
				Array.Empty<BattleUnit>(),
				Array.Empty<BattleUnit>(),
				new BattleStats(),
				p_impressedCreatures);
		}

		private static CreatureUnit CreateCreatureUnit()
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();

			return new CreatureUnit
			{
				Species = species
			};
		}
	}
}