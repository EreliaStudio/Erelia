using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
    public sealed class PlayerServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void AddCreatureToTeamOrStorage_ValidCreature_AddsToTeamAndEmitsEvent()
        {
            PlayerService playerService = ServiceLocator.Instance.PlayerService;
            CreatureUnit creature = new CreatureUnit();

            CreatureUnit addedCreature = null;
            EventCenter.PlayerCreatureAdded += OnCreatureAdded;

            try
            {
                bool result = playerService.AddCreatureToTeamOrStorage(creature);

                Assert.That(result, Is.True);
                Assert.That(addedCreature, Is.SameAs(creature));
            }
            finally
            {
                EventCenter.PlayerCreatureAdded -= OnCreatureAdded;
            }

            void OnCreatureAdded(CreatureUnit unit, bool hadOpenSlot) => addedCreature = unit;
        }

        [Test]
        public void CreateSaveData_WithPlayerState_CapturesTeamStorageAndPosition()
        {
            PlayerService playerService = ServiceLocator.Instance.PlayerService;
            CreatureSpecies species = LoadTestSpecies();
            playerService.PlayerData.WorldCell = new Vector3Int(3, 4, 5);
            playerService.PlayerData.Team[0] = CreateCreature(species, "DPS");
            playerService.PlayerData.CreatureStorage.Add(CreateCreature(species, "Default"));

            PlayerSaveData saveData = playerService.CreateSaveData();

            Assert.That(saveData.WorldCell.ToVector3Int(), Is.EqualTo(new Vector3Int(3, 4, 5)));
            Assert.That(saveData.TeamSlots.Count, Is.EqualTo(GameRule.TeamMemberCount));
            Assert.That(saveData.TeamSlots[0].HasCreature, Is.True);
            Assert.That(saveData.TeamSlots[0].Creature.SpeciesResourceId, Is.EqualTo("Creature/CreatureA/CreatureA"));
            Assert.That(saveData.TeamSlots[0].Creature.CurrentFormId, Is.EqualTo("DPS"));
            Assert.That(saveData.StoredCreatures.Count, Is.EqualTo(1));
            Assert.That(saveData.StoredCreatures[0].CurrentFormId, Is.EqualTo("Default"));
        }

        [Test]
        public void LoadFromSaveData_WithPlayerSaveData_RestoresTeamStorageAndPosition()
        {
            PlayerService playerService = ServiceLocator.Instance.PlayerService;
            PlayerSaveData saveData = CreatePlayerSaveData();

            bool result = playerService.LoadFromSaveData(saveData);

            Assert.That(result, Is.True);
            Assert.That(playerService.PlayerData.WorldCell, Is.EqualTo(new Vector3Int(3, 4, 5)));
            Assert.That(playerService.PlayerData.Team[0], Is.Not.Null);
            Assert.That(playerService.PlayerData.Team[0].Species.name, Is.EqualTo("CreatureA"));
            Assert.That(playerService.PlayerData.Team[0].CurrentFormID, Is.EqualTo("DPS"));
            Assert.That(playerService.PlayerData.Team[1], Is.Null);
            Assert.That(playerService.PlayerData.CreatureStorage.Count, Is.EqualTo(1));
            Assert.That(playerService.PlayerData.CreatureStorage.GetAt(0).CurrentFormID, Is.EqualTo("Default"));
        }

        [Test]
        public void PlayerWorldCell_WithPlayerData_ReturnsCurrentWorldCell()
        {
            PlayerService playerService = ServiceLocator.Instance.PlayerService;
            playerService.PlayerData.WorldCell = new Vector3Int(7, 8, 9);

            Assert.That(playerService.PlayerWorldCell, Is.EqualTo(new Vector3Int(7, 8, 9)));
        }

        private static PlayerSaveData CreatePlayerSaveData()
        {
            var saveData = new PlayerSaveData
            {
                WorldCell = SerializableVector3Int.From(new Vector3Int(3, 4, 5))
            };

            for (int index = 0; index < GameRule.TeamMemberCount; index++)
            {
                saveData.TeamSlots.Add(new CreatureSlotSaveData());
            }

            saveData.TeamSlots[0] = new CreatureSlotSaveData
            {
                HasCreature = true,
                Creature = new CreatureUnitSaveData
                {
                    SpeciesResourceId = "Creature/CreatureA/CreatureA",
                    CurrentFormId = "DPS"
                }
            };
            saveData.StoredCreatures.Add(new CreatureUnitSaveData
            {
                SpeciesResourceId = "Creature/CreatureA/CreatureA",
                CurrentFormId = "Default"
            });

            return saveData;
        }

        private static CreatureUnit CreateCreature(CreatureSpecies p_species, string p_formId)
        {
            var creature = new CreatureUnit
            {
                Species = p_species,
                CurrentFormID = p_formId
            };

            FeatBoardService.ApplyProgress(creature);
            return creature;
        }

        private static CreatureSpecies LoadTestSpecies()
        {
            CreatureSpecies species = Resources.Load<CreatureSpecies>("Creature/CreatureA/CreatureA");
            Assert.That(species, Is.Not.Null);
            return species;
        }
    }
}
