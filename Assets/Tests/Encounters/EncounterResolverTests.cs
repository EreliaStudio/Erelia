using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Encounters
{
    public sealed class EncounterResolverTests
    {
        private readonly List<UnityEngine.Object> ownedAssets = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < ownedAssets.Count; i++)
                if (ownedAssets[i] != null)
                    UnityEngine.Object.DestroyImmediate(ownedAssets[i]);
            ownedAssets.Clear();
        }

        private BiomeDefinition CreateBiome()
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            ownedAssets.Add(biome);
            return biome;
        }

        private static BiomeEncounterRule CreateRule(float chance, params EncounterTier.Entry[] entries)
        {
            var table = new EncounterTable();
            for (int i = 0; i < entries.Length; i++)
                table.NoBadge.WeightedTeams.Add(entries[i]);
            return new BiomeEncounterRule { BaseChancePerStep = chance, EncounterTable = table };
        }

        private static EncounterTier.Entry CreateEntry(int weight = 1)
        {
            return new EncounterTier.Entry { Weight = weight, Team = new EncounterUnit[0] };
        }

        // EncounterResolver.seed is [SerializeField] private — inject via JsonUtility.
        private static EncounterResolver CreateResolver(int seed = 0)
        {
            return JsonUtility.FromJson<EncounterResolver>($"{{\"seed\":{seed}}}");
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenBiomeIsNull()
        {
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(null, "grass", Vector3Int.zero, out var team);
            Assert.That(result, Is.False);
            Assert.That(team, Is.Null);
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenTagNotRegisteredInBiome()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, CreateEntry());
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "sand", Vector3Int.zero, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenChanceIsZero()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(0f, CreateEntry());
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "grass", Vector3Int.zero, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenSameCellCheckedTwice()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, CreateEntry());
            var resolver = CreateResolver();
            var cell = new Vector3Int(3, 0, 5);
            resolver.TryResolveEncounter(biome, "grass", cell, out _);
            var result = resolver.TryResolveEncounter(biome, "grass", cell, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryResolve_AllowsEncounter_AfterMovingToDifferentCell()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, CreateEntry());
            var resolver = CreateResolver();
            resolver.TryResolveEncounter(biome, "grass", new Vector3Int(0, 0, 0), out _);
            var result = resolver.TryResolveEncounter(biome, "grass", new Vector3Int(1, 0, 0), out var team);
            Assert.That(result, Is.True);
            Assert.That(team, Is.Not.Null);
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenEncounterTableIsNull()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule
            {
                BaseChancePerStep = 1f,
                EncounterTable = null
            };
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "grass", Vector3Int.zero, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenTierHasNoEntries()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule
            {
                BaseChancePerStep = 1f,
                EncounterTable = new EncounterTable()
            };
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "grass", Vector3Int.zero, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryResolve_ReturnsFalse_WhenAllEntriesHaveZeroWeight()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, CreateEntry(weight: 0));
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "grass", Vector3Int.zero, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryResolve_ReturnsSelectedTeam_WhenChanceIs1_AndSingleEntry()
        {
            var biome = CreateBiome();
            var expectedTeam = new EncounterUnit[0];
            var entry = new EncounterTier.Entry { Weight = 1, Team = expectedTeam };
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, entry);
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "grass", Vector3Int.zero, out var team);
            Assert.That(result, Is.True);
            Assert.That(team, Is.SameAs(expectedTeam));
        }

        [Test]
        public void TryResolve_SkipsNullEntries_AndStillSelectsValidEntry()
        {
            var biome = CreateBiome();
            var expectedTeam = new EncounterUnit[0];
            var table = new EncounterTable();
            table.NoBadge.WeightedTeams.Add(null);
            table.NoBadge.WeightedTeams.Add(new EncounterTier.Entry { Weight = 1, Team = expectedTeam });
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule
            {
                BaseChancePerStep = 1f,
                EncounterTable = table
            };
            var resolver = CreateResolver();
            var result = resolver.TryResolveEncounter(biome, "grass", Vector3Int.zero, out var team);
            Assert.That(result, Is.True);
            Assert.That(team, Is.SameAs(expectedTeam));
        }

        [Test]
        public void TryResolve_IsReproducible_WithSameSeed()
        {
            var biome = CreateBiome();
            var teamA = new EncounterUnit[0];
            var teamB = new EncounterUnit[0];
            var entryA = new EncounterTier.Entry { Weight = 1, Team = teamA };
            var entryB = new EncounterTier.Entry { Weight = 1, Team = teamB };
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, entryA, entryB);

            var resolverA = CreateResolver(seed: 42);
            var resolverB = CreateResolver(seed: 42);

            for (int i = 0; i < 5; i++)
            {
                var cell = new Vector3Int(i, 0, 0);
                resolverA.TryResolveEncounter(biome, "grass", cell, out var team1);
                resolverB.TryResolveEncounter(biome, "grass", cell, out var team2);
                Assert.That(team1, Is.SameAs(team2), $"Resolvers with same seed should pick the same team at step {i}");
            }
        }

        [Test]
        public void TryResolve_ProducesDifferentResults_WithDifferentSeeds()
        {
            var biome = CreateBiome();
            var teamA = new EncounterUnit[0];
            var teamB = new EncounterUnit[0];
            var entryA = new EncounterTier.Entry { Weight = 1, Team = teamA };
            var entryB = new EncounterTier.Entry { Weight = 1, Team = teamB };
            biome.WildEncounterRulesByTriggerTag["grass"] = CreateRule(1f, entryA, entryB);

            // Run 20 steps with each seed and collect selected teams.
            var resolver1 = CreateResolver(seed: 1);
            var resolver2 = CreateResolver(seed: 999);

            var teams1 = new List<EncounterUnit[]>();
            var teams2 = new List<EncounterUnit[]>();

            for (int i = 0; i < 20; i++)
            {
                var cell = new Vector3Int(i, 0, 0);
                resolver1.TryResolveEncounter(biome, "grass", cell, out var t1);
                resolver2.TryResolveEncounter(biome, "grass", cell, out var t2);
                teams1.Add(t1);
                teams2.Add(t2);
            }

            bool anyDifference = false;
            for (int i = 0; i < 20; i++)
                if (!ReferenceEquals(teams1[i], teams2[i])) { anyDifference = true; break; }

            Assert.That(anyDifference, Is.True, "Different seeds should produce at least one different team selection over 20 steps");
        }
    }
}
