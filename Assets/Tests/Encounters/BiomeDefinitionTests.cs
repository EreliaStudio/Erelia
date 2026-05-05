using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Encounters
{
    public sealed class BiomeDefinitionTests
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

        // ── TryGetEncounterRule ──────────────────────────────────────────────

        [Test]
        public void TryGetEncounterRule_ReturnsFalse_WhenTagIsNull()
        {
            var biome = CreateBiome();
            Assert.That(biome.TryGetEncounterRule(null, out _), Is.False);
        }

        [Test]
        public void TryGetEncounterRule_ReturnsFalse_WhenTagIsWhitespace()
        {
            var biome = CreateBiome();
            Assert.That(biome.TryGetEncounterRule("   ", out _), Is.False);
        }

        [Test]
        public void TryGetEncounterRule_ReturnsFalse_WhenTagNotRegistered()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule();
            Assert.That(biome.TryGetEncounterRule("sand", out _), Is.False);
        }

        [Test]
        public void TryGetEncounterRule_ReturnsTrue_WhenExactTagMatches()
        {
            var biome = CreateBiome();
            var rule = new BiomeEncounterRule();
            biome.WildEncounterRulesByTriggerTag["grass"] = rule;
            var result = biome.TryGetEncounterRule("grass", out var found);
            Assert.That(result, Is.True);
            Assert.That(found, Is.SameAs(rule));
        }

        [Test]
        public void TryGetEncounterRule_ReturnsTrue_WhenTagDiffersByCase()
        {
            var biome = CreateBiome();
            var rule = new BiomeEncounterRule();
            biome.WildEncounterRulesByTriggerTag["Grass"] = rule;
            var result = biome.TryGetEncounterRule("grass", out var found);
            Assert.That(result, Is.True);
            Assert.That(found, Is.SameAs(rule));
        }

        [Test]
        public void TryGetEncounterRule_ReturnsTrue_WhenTagHasLeadingTrailingSpaces()
        {
            var biome = CreateBiome();
            var rule = new BiomeEncounterRule();
            biome.WildEncounterRulesByTriggerTag["grass"] = rule;
            var result = biome.TryGetEncounterRule("  grass  ", out var found);
            Assert.That(result, Is.True);
            Assert.That(found, Is.SameAs(rule));
        }

        // ── CleanTriggerTag ──────────────────────────────────────────────────

        [Test]
        public void CleanTriggerTag_TrimsLeadingAndTrailingWhitespace()
        {
            Assert.That(BiomeDefinition.CleanTriggerTag("  grass  "), Is.EqualTo("grass"));
        }

        [Test]
        public void CleanTriggerTag_ReturnsEmpty_WhenNull()
        {
            Assert.That(BiomeDefinition.CleanTriggerTag(null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void CleanTriggerTag_ReturnsEmpty_WhenWhitespaceOnly()
        {
            Assert.That(BiomeDefinition.CleanTriggerTag("   "), Is.EqualTo(string.Empty));
        }

        [Test]
        public void CleanTriggerTag_ReturnsUnchanged_WhenAlreadyClean()
        {
            Assert.That(BiomeDefinition.CleanTriggerTag("grass"), Is.EqualTo("grass"));
        }

        // ── AreTriggerTagsEquivalent ─────────────────────────────────────────

        [Test]
        public void AreTriggerTagsEquivalent_ReturnsTrue_WhenSameTag()
        {
            Assert.That(BiomeDefinition.AreTriggerTagsEquivalent("grass", "grass"), Is.True);
        }

        [Test]
        public void AreTriggerTagsEquivalent_ReturnsTrue_WhenDiffersByCase()
        {
            Assert.That(BiomeDefinition.AreTriggerTagsEquivalent("GRASS", "grass"), Is.True);
        }

        [Test]
        public void AreTriggerTagsEquivalent_ReturnsFalse_WhenBothEmpty()
        {
            Assert.That(BiomeDefinition.AreTriggerTagsEquivalent("", ""), Is.False);
        }

        [Test]
        public void AreTriggerTagsEquivalent_ReturnsFalse_WhenEitherIsNull()
        {
            Assert.That(BiomeDefinition.AreTriggerTagsEquivalent(null, "grass"), Is.False);
            Assert.That(BiomeDefinition.AreTriggerTagsEquivalent("grass", null), Is.False);
        }

        [Test]
        public void AreTriggerTagsEquivalent_ReturnsFalse_WhenTagsAreDifferent()
        {
            Assert.That(BiomeDefinition.AreTriggerTagsEquivalent("grass", "sand"), Is.False);
        }

        // ── GetEncounterRuleTags ─────────────────────────────────────────────

        [Test]
        public void GetEncounterRuleTags_ReturnsEmpty_WhenNoRulesRegistered()
        {
            var biome = CreateBiome();
            Assert.That(biome.GetEncounterRuleTags(), Is.Empty);
        }

        [Test]
        public void GetEncounterRuleTags_ReturnsAllRegisteredTags()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule();
            biome.WildEncounterRulesByTriggerTag["sand"] = new BiomeEncounterRule();
            var tags = biome.GetEncounterRuleTags();
            Assert.That(tags, Has.Count.EqualTo(2));
            Assert.That(tags, Does.Contain("grass"));
            Assert.That(tags, Does.Contain("sand"));
        }

        [Test]
        public void GetEncounterRuleTags_DeduplicatesEquivalentTags()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule();
            biome.WildEncounterRulesByTriggerTag["GRASS"] = new BiomeEncounterRule();
            var tags = biome.GetEncounterRuleTags();
            Assert.That(tags, Has.Count.EqualTo(1));
        }

        [Test]
        public void GetEncounterRuleTags_SkipsWhitespaceOnlyKeys()
        {
            var biome = CreateBiome();
            biome.WildEncounterRulesByTriggerTag["   "] = new BiomeEncounterRule();
            biome.WildEncounterRulesByTriggerTag["grass"] = new BiomeEncounterRule();
            var tags = biome.GetEncounterRuleTags();
            Assert.That(tags, Has.Count.EqualTo(1));
            Assert.That(tags, Does.Contain("grass"));
        }
    }
}
