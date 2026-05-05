using NUnit.Framework;
using UnityEngine;

namespace Tests.Creature.Storage
{
	public sealed class CreatureStorageTests
	{
		[Test]
		public void NewStorage_StartsEmpty()
		{
			var storage = new CreatureStorage();

			Assert.That(storage.Count, Is.EqualTo(0));
			Assert.That(storage.StoredCreatures, Is.Not.Null);
		}

		[Test]
		public void Add_NullCreature_DoesNothing()
		{
			var storage = new CreatureStorage();

			storage.Add(null);

			Assert.That(storage.Count, Is.EqualTo(0));
		}

		[Test]
		public void Add_ValidCreature_StoresCreature()
		{
			var storage = new CreatureStorage();
			CreatureUnit creature = CreateCreatureUnit();

			storage.Add(creature);

			Assert.That(storage.Count, Is.EqualTo(1));
			Assert.That(storage.Contains(creature), Is.True);
			Assert.That(storage.GetAt(0), Is.SameAs(creature));
		}

		[Test]
		public void Remove_StoredCreature_RemovesAndReturnsTrue()
		{
			var storage = new CreatureStorage();
			CreatureUnit creature = CreateCreatureUnit();

			storage.Add(creature);

			bool removed = storage.Remove(creature);

			Assert.That(removed, Is.True);
			Assert.That(storage.Count, Is.EqualTo(0));
			Assert.That(storage.Contains(creature), Is.False);
		}

		[Test]
		public void Remove_MissingCreature_ReturnsFalse()
		{
			var storage = new CreatureStorage();
			CreatureUnit creature = CreateCreatureUnit();

			bool removed = storage.Remove(creature);

			Assert.That(removed, Is.False);
		}

		[Test]
		public void GetAt_OutOfRange_ReturnsNull()
		{
			var storage = new CreatureStorage();

			Assert.That(storage.GetAt(-1), Is.Null);
			Assert.That(storage.GetAt(0), Is.Null);
		}

		[Test]
		public void Clone_CopiesCreatureReferencesIntoIndependentList()
		{
			var storage = new CreatureStorage();
			CreatureUnit firstCreature = CreateCreatureUnit();
			CreatureUnit secondCreature = CreateCreatureUnit();

			storage.Add(firstCreature);

			CreatureStorage clone = storage.Clone();
			clone.Add(secondCreature);

			Assert.That(storage.Count, Is.EqualTo(1));
			Assert.That(clone.Count, Is.EqualTo(2));
			Assert.That(clone.Contains(firstCreature), Is.True);
			Assert.That(clone.Contains(secondCreature), Is.True);
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