using System;

[Serializable]
public class EncounterTable
{
	public EncounterTier NoBadge = new EncounterTier();
	public EncounterTier OneBadge = new EncounterTier();
	public EncounterTier TwoBadges = new EncounterTier();
	public EncounterTier ThreeBadges = new EncounterTier();
	public EncounterTier FourBadges = new EncounterTier();
	public EncounterTier FiveBadges = new EncounterTier();
	public EncounterTier SixBadges = new EncounterTier();
	public EncounterTier SevenBadges = new EncounterTier();
	public EncounterTier EightBadges = new EncounterTier();
	public EncounterTier PostGame = new EncounterTier();

	public EncounterTier GetTierForBadgeCount(int badgeCount)
	{
		return badgeCount switch
		{
			<= 0 => NoBadge,
			1 => OneBadge,
			2 => TwoBadges,
			3 => ThreeBadges,
			4 => FourBadges,
			5 => FiveBadges,
			6 => SixBadges,
			7 => SevenBadges,
			8 => EightBadges,
			_ => PostGame
		};
	}
}
