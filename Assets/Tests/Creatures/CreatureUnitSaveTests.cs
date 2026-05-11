using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Persistence;

namespace Tests.Creatures
{
	public sealed class CreatureUnitSaveTests
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
		public void ToJson_WithSpeciesFormAndFeatProgress_MatchesExpectedJson()
		{
			CreatureUnit creature = SaveTestDataFactory.CreateCreature(
				species,
				SaveTestDataFactory.DpsFormId,
				p_includeDamageProgress: true);

			JObject json = creature.ToJson();

			SaveTestDataFactory.AssertJsonEquals(
				SaveTestDataFactory.ExpectedCreatureJson(p_includeDamageProgress: true),
				json);
		}

		[Test]
		public void FromJson_WithRegisteredSpecies_RestoresProgressAndRebuildsDerivedState()
		{
			JObject json = SaveTestDataFactory.ExpectedCreatureJson(
				p_includeDamageProgress: true,
				p_damageCompletionCount: 1,
				p_damageProgress: 25f,
				p_damageRepeatCount: 1);

			CreatureUnit creature = CreatureUnit.FromJson(json, registry);

			Assert.That(creature, Is.Not.Null);
			Assert.That(creature.Species, Is.SameAs(species));
			Assert.That(creature.CurrentFormID, Is.EqualTo(SaveTestDataFactory.DpsFormId));
			Assert.That(creature.FeatBoardProgress.FindProgress(SaveTestDataFactory.RootNodeId).CompletionCount, Is.EqualTo(1));

			FeatNodeProgress damageProgress = creature.FeatBoardProgress.FindProgress(SaveTestDataFactory.DamageNodeId);
			Assert.That(damageProgress, Is.Not.Null);
			Assert.That(damageProgress.CompletionCount, Is.EqualTo(1));
			Assert.That(damageProgress.RequirementProgress[0].CurrentProgress, Is.EqualTo(25f));
			Assert.That(damageProgress.RequirementProgress[0].CompletedRepeatCount, Is.EqualTo(1));

			Assert.That(creature.Attributes.Health, Is.EqualTo(species.Attributes.Health));
			Assert.That(creature.Attributes.Attack, Is.EqualTo(species.Attributes.Attack + 5));
			Assert.That(creature.Abilities, Does.Contain(species.DefaultAbilities[0]));
			Assert.That(creature.Abilities, Does.Contain(SaveTestDataFactory.GetDamageRewardAbility(species)));
			Assert.That(creature.PermanentPassives, Does.Contain(SaveTestDataFactory.GetDamageRewardPassive(species)));
		}

		[Test]
		public void FromJson_WithMissingSpeciesGuid_ReturnsNull()
		{
			JObject json = new JObject
			{
				["speciesGuid"] = string.Empty,
				["formId"] = SaveTestDataFactory.DpsFormId,
				["featBoard"] = new JObject { ["nodes"] = new JArray() }
			};

			CreatureUnit creature = CreatureUnit.FromJson(json, registry);

			Assert.That(creature, Is.Null);
		}
	}
}
