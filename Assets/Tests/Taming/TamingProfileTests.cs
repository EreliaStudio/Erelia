using NUnit.Framework;
using UnityEngine;

namespace Tests.Taming.Profile
{
	public sealed class TamingProfileTests
	{
		[Test]
		public void NewCreatureSpecies_AlwaysHasTamingProfile()
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();

			Assert.That(species.TamingProfile, Is.Not.Null);
			Assert.That(species.TamingProfile.Conditions, Is.Not.Null);

			Object.DestroyImmediate(species);
		}

		[Test]
		public void NewCreatureSpecies_TamingProfileStartsEmpty()
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();

			Assert.That(species.TamingProfile.HasConditions, Is.False);
			Assert.That(species.TamingProfile.Conditions.Count, Is.EqualTo(0));

			Object.DestroyImmediate(species);
		}

		[Test]
		public void TamingProfile_WithCondition_HasConditions()
		{
			var profile = new TamingProfile();

			profile.Conditions.Add(new DealDamageRequirement { RequiredAmount = 10 });

			Assert.That(profile.HasConditions, Is.True);
		}

		[Test]
		public void EnsureInitialized_RecreatesMissingConditionList()
		{
			var profile = new TamingProfile
			{
				Conditions = null
			};

			profile.EnsureInitialized();

			Assert.That(profile.Conditions, Is.Not.Null);
			Assert.That(profile.Conditions.Count, Is.EqualTo(0));
		}
	}
}