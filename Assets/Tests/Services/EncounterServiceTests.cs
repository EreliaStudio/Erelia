using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
    public sealed class EncounterServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void RequestBattle_WithValidConfig_EmitsBattleLaunchRequested()
        {
            EncounterService encounterService = ServiceLocator.Instance.EncounterService;
            BoardConfiguration config = new BoardConfiguration();

            bool launchRequested = false;
            EventCenter.BattleLaunchRequested += OnLaunchRequested;

            try
            {
                encounterService.RequestBattle(config, Vector3.zero, null, PlacementStyle.HalfBoard, false);

                Assert.That(launchRequested, Is.True);
            }
            finally
            {
                EventCenter.BattleLaunchRequested -= OnLaunchRequested;
            }

            void OnLaunchRequested(BoardConfiguration c, Vector3 pos, IReadOnlyList<EncounterUnit> units, PlacementStyle style, bool allowsTaming)
                => launchRequested = true;
        }
    }
}
