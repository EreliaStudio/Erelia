using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
    public sealed class SaveServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void Save_WithBoundSaveData_CopiesPlayerDataAndEmitsSaveCompleted()
        {
            SaveService saveService = ServiceLocator.Instance.SaveService;
            GameSaveData saveData = new GameSaveData();
            saveService.BindSaveData(saveData);

            bool saveCompleted = false;
            bool saveSucceeded = false;
            EventCenter.SaveCompleted += OnSaveCompleted;

            try
            {
                bool result = saveService.Save();

                Assert.That(result, Is.True);
                Assert.That(saveCompleted, Is.True);
                Assert.That(saveSucceeded, Is.True);
            }
            finally
            {
                EventCenter.SaveCompleted -= OnSaveCompleted;
            }

            void OnSaveCompleted(GameSaveData data, bool success)
            {
                saveCompleted = true;
                saveSucceeded = success;
            }
        }

        [Test]
        public void CreateSaveFileData_UsesWorldSeedAndPlayerServicePayload()
        {
            ServiceLocator.Instance.PlayerService.PlayerData.SetPosition(new Vector3(1.5f, 2f, 3.5f), true);
            ServiceLocator.Instance.WorldService.WorldContext.ApplySeed(42);
            GameSaveData saveData = new GameSaveData();
            saveData.SetRespawnPoint(new Vector3Int(4, 5, 6));
            SaveService saveService = ServiceLocator.Instance.SaveService;
            saveService.BindSaveData(saveData);

            GameSaveFileData fileData = saveService.CreateSaveFileData();

            Assert.That(fileData.WorldSeed, Is.EqualTo(42));
            Assert.That(fileData.RespawnPoint.ToVector3Int(), Is.EqualTo(new Vector3Int(4, 5, 6)));
            Assert.That(Vector3Int.FloorToInt(fileData.Player.Position), Is.EqualTo(new Vector3Int(1, 2, 3)));
        }

        [Test]
        public void TryLoad_WithSaveFileData_AppliesWorldSeedAndPlayerData()
        {
            SaveService saveService = ServiceLocator.Instance.SaveService;
            GameSaveData saveData = new GameSaveData();
            saveService.BindSaveData(saveData);
            GameSaveFileData fileData = new GameSaveFileData
            {
                WorldSeed = 42,
                RespawnPoint = SerializableVector3Int.From(new Vector3Int(4, 5, 6)),
                Player = new PlayerSaveData
                {
                    Position = new Vector3(1.5f, 2f, 3.5f)
                }
            };

            bool result = saveService.TryLoad(fileData);

            Assert.That(result, Is.True);
            Assert.That(ServiceLocator.Instance.WorldService.WorldContext.Seed, Is.EqualTo(42));
            Assert.That(ServiceLocator.Instance.PlayerService.PlayerWorldCell, Is.EqualTo(new Vector3Int(1, 2, 3)));
            Assert.That(saveData.WorldSeed, Is.EqualTo(42));
            Assert.That(saveData.RespawnPoint, Is.EqualTo(new Vector3Int(4, 5, 6)));
        }
    }
}
