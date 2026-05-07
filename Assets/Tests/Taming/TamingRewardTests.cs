using System;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Taming.Rewards
{
	public sealed class TamingRewardTests
	{
		[Test]
		public void TamingResolved_AddsCreatureToEmptyTeamSlot()
		{
			PlayerService playerService = CreatePlayerService(out GameContext gameContext);
			PlayerData playerData = gameContext.Player;
			CreatureUnit recruit = CreateCreatureUnit();

			try
			{
				EmitTamingResolved(recruit);
			}
			finally
			{
				playerService.Shutdown();
			}

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.GreaterThanOrEqualTo(0));
			Assert.That(playerData.CreatureStorage.Count, Is.EqualTo(0));
		}

		[Test]
		public void NoTamingResolution_PlayerLoss_ForfeitsCreature()
		{
			var playerData = new PlayerData();
			CreatureUnit recruit = CreateCreatureUnit();

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.EqualTo(-1));
			Assert.That(playerData.CreatureStorage.Contains(recruit), Is.False);
		}

		[Test]
		public void NoTamingResolution_NeutralOutcome_DoesNotAwardCreature()
		{
			var playerData = new PlayerData();
			CreatureUnit recruit = CreateCreatureUnit();

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.EqualTo(-1));
			Assert.That(playerData.CreatureStorage.Contains(recruit), Is.False);
		}

		[Test]
		public void TamingResolved_FullTeam_SendsCreatureToStorage()
		{
			PlayerService playerService = CreatePlayerService(out GameContext gameContext);
			PlayerData playerData = gameContext.Player;

			for (int index = 0; index < playerData.Team.Length; index++)
			{
				playerData.Team[index] = CreateCreatureUnit();
			}

			CreatureUnit recruit = CreateCreatureUnit();
			try
			{
				EmitTamingResolved(recruit);
			}
			finally
			{
				playerService.Shutdown();
			}

			Assert.That(Array.IndexOf(playerData.Team, recruit), Is.EqualTo(-1));
			Assert.That(playerData.CreatureStorage.Contains(recruit), Is.True);
			Assert.That(playerData.CreatureStorage.Count, Is.EqualTo(1));
		}

		[Test]
		public void PlayerService_AddCreature_NullCreatureDoesNotThrow()
		{
			PlayerService playerService = CreatePlayerService(out _);

			try
			{
				Assert.DoesNotThrow(() => playerService.AddCreatureToTeamOrStorage(null));
			}
			finally
			{
				playerService.Shutdown();
			}
		}

		[Test]
		public void TamingResolved_NullArgumentsDoNotThrow()
		{
			PlayerService playerService = CreatePlayerService(out _);

			try
			{
				Assert.DoesNotThrow(() => EventCenter.EmitTamingResolved(null, null));
			}
			finally
			{
				playerService.Shutdown();
			}
		}

		private static PlayerService CreatePlayerService(out GameContext p_gameContext)
		{
			p_gameContext = new GameContext();
			PlayerService playerService = new PlayerService(p_gameContext);
			playerService.Initialize();
			return playerService;
		}

		private static void EmitTamingResolved(params CreatureUnit[] p_recruits)
		{
			EventCenter.EmitTamingResolved(
				TestBattleContextFactory.CreateEmpty(),
				p_recruits);
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
