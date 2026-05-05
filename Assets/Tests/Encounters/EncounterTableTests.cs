using NUnit.Framework;

namespace Tests.Encounters
{
    public sealed class EncounterTableTests
    {
        // ── GetTierForBadgeCount ─────────────────────────────────────────────

        [Test]
        public void GetTierForBadgeCount_ReturnsNoBadge_WhenCountIsZero()
        {
            var table = new EncounterTable();
            Assert.That(table.GetTierForBadgeCount(0), Is.SameAs(table.NoBadge));
        }

        [Test]
        public void GetTierForBadgeCount_ReturnsNoBadge_WhenCountIsNegative()
        {
            var table = new EncounterTable();
            Assert.That(table.GetTierForBadgeCount(-1), Is.SameAs(table.NoBadge));
            Assert.That(table.GetTierForBadgeCount(-100), Is.SameAs(table.NoBadge));
        }

        [Test]
        public void GetTierForBadgeCount_ReturnsPostGame_WhenCountExceedsEight()
        {
            var table = new EncounterTable();
            Assert.That(table.GetTierForBadgeCount(9), Is.SameAs(table.PostGame));
            Assert.That(table.GetTierForBadgeCount(100), Is.SameAs(table.PostGame));
        }

        [Test]
        public void GetTierForBadgeCount_ReturnsCorrectTier_ForEachBadgeCount()
        {
            var table = new EncounterTable();
            Assert.That(table.GetTierForBadgeCount(1), Is.SameAs(table.OneBadge));
            Assert.That(table.GetTierForBadgeCount(2), Is.SameAs(table.TwoBadges));
            Assert.That(table.GetTierForBadgeCount(3), Is.SameAs(table.ThreeBadges));
            Assert.That(table.GetTierForBadgeCount(4), Is.SameAs(table.FourBadges));
            Assert.That(table.GetTierForBadgeCount(5), Is.SameAs(table.FiveBadges));
            Assert.That(table.GetTierForBadgeCount(6), Is.SameAs(table.SixBadges));
            Assert.That(table.GetTierForBadgeCount(7), Is.SameAs(table.SevenBadges));
            Assert.That(table.GetTierForBadgeCount(8), Is.SameAs(table.EightBadges));
        }

        [Test]
        public void GetTierForBadgeCount_EachBadgeCount_ReturnsDifferentTierInstance()
        {
            var table = new EncounterTable();
            var tiers = new[]
            {
                table.GetTierForBadgeCount(0),
                table.GetTierForBadgeCount(1),
                table.GetTierForBadgeCount(2),
                table.GetTierForBadgeCount(3),
                table.GetTierForBadgeCount(4),
                table.GetTierForBadgeCount(5),
                table.GetTierForBadgeCount(6),
                table.GetTierForBadgeCount(7),
                table.GetTierForBadgeCount(8),
                table.GetTierForBadgeCount(9)
            };

            for (int i = 0; i < tiers.Length; i++)
                for (int j = i + 1; j < tiers.Length; j++)
                    Assert.That(tiers[i], Is.Not.SameAs(tiers[j]),
                        $"Badge counts {i} and {j} should not return the same tier instance");
        }

        // ── WeightedTeam selection via EncounterTier ─────────────────────────

        [Test]
        public void WeightedTeams_StartsEmpty_OnNewTable()
        {
            var table = new EncounterTable();
            Assert.That(table.NoBadge.WeightedTeams, Is.Empty);
            Assert.That(table.PostGame.WeightedTeams, Is.Empty);
        }

        [Test]
        public void WeightedTeams_CanAddEntries()
        {
            var tier = new EncounterTier();
            tier.WeightedTeams.Add(new EncounterTier.Entry { DisplayName = "Pack A", Weight = 3 });
            tier.WeightedTeams.Add(new EncounterTier.Entry { DisplayName = "Pack B", Weight = 1 });
            Assert.That(tier.WeightedTeams, Has.Count.EqualTo(2));
        }

        [Test]
        public void Entry_DefaultWeight_IsOne()
        {
            var entry = new EncounterTier.Entry();
            Assert.That(entry.Weight, Is.EqualTo(1));
        }
    }
}
