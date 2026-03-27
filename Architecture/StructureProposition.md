# Structure Proposition

This document rewrites `Proposition.md` into more concrete field sketches.
When `Proposition.md` did not specify an exact field type, a descriptive placeholder type was inferred from the text.

# Model.Core

## GameRun
Kind: Serializable
Composed of:
- World.Data World
- Player.State Player
- Progression.RunProgress Progress

## HealPoint
Kind: Serializable
Composed of:
- World.Structure.Location Location
- string DisplayName

# Model.Player

## Player.State
Kind: Serializable
Composed of:
- World.MapLocation CurrentLocation
- World.MapLocation RespawnLocation
- int Money
- Player.Team Team
- Creature.PCStorage PcStorage
- Item.Inventory Inventory
- List<Progression.Badge> ObtainedBadges

## Player.Team
Kind: Serializable
Composed of:
- List<Creature.Unit> Units

# Model.World

## World.MapLocation
Kind: Serializable
Composed of:
- string WorldName
- Vector3Int PositionInMap

## World.Data
Kind: Serializable
Composed of:
- Dictionary<string, World.MapData> MapsByName
- World.Generator Generator

## World.MapData
Kind: Serializable
Composed of:
- Dictionary<World.Chunk.Coordinates, World.Chunk.Data> Chunks

## World.Biome
Kind: ScriptableObject
Composed of:
- string DisplayName
- World.Biome.TerrainGenerationProfile TerrainGenerationProfile
- World.Biome.GroundVoxelPalette GroundVoxelPalette
- List<World.Structure.SceneryPlacementRule> SceneryPlacementRules
- List<World.Biome.LocationGenerationRule> LocationGenerationRules
- World.Biome.WildEncounter StandardWildEncounter
- World.Biome.WildEncounter RareWildEncounter

## World.Biome.TerrainGenerationProfile
Kind: Serializable
Composed of:
- float BaseHeightOffset
- float HeightAmplitude
- float HeightNoiseScale
- float TerrainRoughness
- float PlateauStrength
- float CliffThreshold
- float RiverCarveStrength

## World.Biome.GroundVoxelPalette
Kind: Serializable
Composed of:
- List<World.Biome.GroundVoxelLayer> GroundLayers
- Voxel.Definition CliffVoxel
- Voxel.Definition RiverbedVoxel
- Voxel.Definition ShoreVoxel

## World.Biome.GroundVoxelLayer
Kind: Serializable
Composed of:
- int MinDepth
- int MaxDepth
- Voxel.Definition Voxel

## World.Biome.LocationGenerationRule
Kind: Serializable
Composed of:
- string RuleId
- World.Structure.LocationType LocationType
- List<World.Structure.Template> CandidateTemplates
- int MinCountPerRegion
- int MaxCountPerRegion
- float Weight
- List<World.Structure.PlacementTag> RequiredPlacementTags

## World.Biome.WildEncounter
Kind: Serializable
Composed of:
- Encounter.Definition Encounter
- float TriggerChancePercent

## World.BiomeRegistry
Kind: ScriptableObject
Composed of:
- SerializedDictionary<string, World.Biome> BiomesByName
- World.Biome FallbackBiome

## World.Generator
Kind: Serializable
Composed of:
- World.Generation.Profile GenerationProfile
- World.BiomeField BiomeField

## World.BiomeField
Kind: Serializable
Composed of:
- World.BiomeField.GenerationSeedData GenerationSeedData
- World.BiomeField.SamplingData SamplingData
- World.BiomeField.CacheData CacheData

## World.BiomeField.GenerationSeedData
Kind: Serializable
Composed of:
- int BaseSeed
- int RegionAssignmentSeed
- int RegionNoiseSeed
- int BlendNoiseSeed

## World.BiomeField.SamplingData
Kind: Serializable
Composed of:
- Vector2Int WorldSize
- float RegionNoiseScale
- float BiomeBlendDistance
- List<World.BiomeField.Region> Regions

## World.BiomeField.Region
Kind: Serializable
Composed of:
- string RegionId
- Vector3Int AnchorPosition
- World.Biome Biome
- float Weight

## World.BiomeField.CacheData
Kind: Serializable
Composed of:
- List<World.BiomeField.CachedChunk> CachedChunks

## World.BiomeField.CachedChunk
Kind: Serializable
Composed of:
- Vector2Int ChunkCoordinate
- string RegionId
- World.Biome Biome

## World.Generation.Profile
Kind: ScriptableObject
Composed of:
- Seed Seed
- List<TerrainNoiseSetting> TerrainNoiseSettings
- List<BiomeDistributionSetting> BiomeDistributionSettings
- List<LocationCountTarget> LocationCountTargets
- List<RoadGenerationSetting> RoadGenerationSettings

## World.Structure.Location
Kind: Serializable
Composed of:
- string LocationId
- World.Structure.LocationType LocationType
- string DisplayName
- Vector3Int WorldPosition
- World.Biome Biome
- List<ConnectedRoad> ConnectedRoads

## World.Structure.LocationType
Kind: Enum
Represents the semantic role of one generated world location.
Values:
- MajorTown: a main city acting as a biome anchor and regional hub.
- Village: a smaller settlement linked to a nearby major town.
- Gym: a battle-focused landmark containing a gym interior.
- PointOfInterest: a notable special place such as a cave, ruin, pond, or dungeon entrance.
- Port: a coastal travel point used to connect sea routes.
- TunnelEntrance: an entrance leading to a generated tunnel or cave passage.

## World.Structure.Town
Kind: Serializable
Composed of:
- InheritedLocationData InheritedLocationData
- World.Structure.TownProfile TownProfile
- List<Building> Buildings
- HealPoint HealPoint
- List<string> NpcOrServiceFlags

## World.Structure.GymLocation
Kind: Serializable
Composed of:
- InheritedLocationData InheritedLocationData
- World.Structure.GymPrefab GymPrefab
- World.Structure.InteriorSpace InteriorSpace

## World.Structure.PointOfInterest
Kind: Serializable
Composed of:
- InheritedLocationData InheritedLocationData
- World.Structure.PointOfInterestPrefab PointOfInterestPrefab
- POICategory POICategory
- List<LinkedStructure> LinkedStructures
- InteriorSpaceIfEnterable InteriorSpaceIfEnterable

## World.Structure.Road
Kind: Serializable
Composed of:
- string RoadId
- StartLocation StartLocation
- EndLocation EndLocation
- List<Vector3Int> PathPoints
- World.Structure.RoadProfile RoadProfile

## World.Structure.RoadProfile
Kind: ScriptableObject
Composed of:
- int LocalWidth
- int LocalDepth
- List<VoxelLayoutOnLocalXAndYAxe> VoxelLayoutOnLocalXAndYAxes
- List<EdgeRule> EdgeRules

## World.Structure.Building
Kind: Serializable
Composed of:
- string BuildingId
- World.Structure.BuildingPrefab BuildingPrefab
- Vector3Int ExteriorWorldPosition
- LinkedInterior LinkedInterior

## World.Structure.Template
Kind: ScriptableObject
Composed of:
- TemplateCategory TemplateCategory
- VoxelOrModuleLayoutReference VoxelOrModuleLayoutReference
- List<ConnectionPoint> ConnectionPoints
- List<LocalInteractiveObject> LocalInteractiveObjects
- List<AllowedBiomeTag> AllowedBiomeTags

## World.Structure.TownProfile
Inherits from: World.Structure.Template
Composed of:
- List<AllowedBuildingPrefab> AllowedBuildingPrefabs
- LotCountRange LotCountRange
- World.Structure.RoadProfile RoadProfile
- List<World.Structure.SceneryPlacementRule> SceneryPlacementRules

## World.Structure.SceneryPlacementRule
Kind: Serializable
Composed of:
- string RuleId
- World.Structure.SceneryPrefab SceneryPrefab
- float Weight
- Vector2Int CountRange
- Vector2Int ClusterSizeRange
- int MinimumSpacing
- List<World.Structure.PlacementTag> RequiredPlacementTags

## World.Structure.PlacementTag
Kind: Enum
Represents a placement condition or preference used by world generation rules.
Values:
- RequiresFlatGround: the placement expects mostly level terrain.
- NearRoad: the placement should be close to a generated road.
- NearWater: the placement should be close to any water source.
- Coast: the placement should be near the sea or ocean edge.
- RiverBank: the placement should be adjacent to a river.
- NearTown: the placement should be outside but close to a settlement.
- InsideTown: the placement should happen inside settlement bounds.
- OutsideTown: the placement should avoid settlement bounds.
- MountainSide: the placement should be on steep or elevated terrain.
- Forest: the placement should be inside or near dense forest terrain.
- Remote: the placement should prefer isolated areas away from major hubs.

## World.Structure.BuildingPrefab
Inherits from: World.Structure.Template
Composed of:
- VoxelLayoutToStampIntoTheWorld VoxelLayoutToStampIntoTheWorld
- EntryPosition EntryPosition
- World.Structure.InteriorPrefab InteriorPrefab
- List<LocalInteractiveObject> LocalInteractiveObjects
- List<World.Structure.PlacementTag> PlacementTags

## World.Structure.GymPrefab
Inherits from: World.Structure.Template
Composed of:
- VoxelLayoutToStampIntoTheWorld VoxelLayoutToStampIntoTheWorld
- EntryPosition EntryPosition
- World.Structure.InteriorPrefab InteriorPrefab
- List<LocalInteractiveObject> LocalInteractiveObjects
- List<GymThemeTag> GymThemeTags

## World.Structure.PointOfInterestPrefab
Inherits from: World.Structure.Template
Composed of:
- VoxelLayoutToStampIntoTheWorld VoxelLayoutToStampIntoTheWorld
- EntryPositionIfEnterable EntryPositionIfEnterable
- InteriorPrefab InteriorPrefab
- List<LocalInteractiveObject> LocalInteractiveObjects
- List<World.Structure.PlacementTag> PlacementTags

## World.Structure.SceneryPrefab
Inherits from: World.Structure.Template
Composed of:
- VoxelLayoutToStampIntoTheWorld VoxelLayoutToStampIntoTheWorld
- List<LocalInteractiveObject> LocalInteractiveObjects
- List<World.Structure.PlacementTag> PlacementTags
- List<AllowedBiomeTag> AllowedBiomeTags

## World.Structure.InteriorPrefab
Inherits from: World.Structure.Template
Composed of:
- VoxelLayoutOrRoomLayout VoxelLayoutOrRoomLayout
- List<LocalInteractiveObject> LocalInteractiveObjects
- List<World.Structure.TrainerPlacement> LocalTrainerPlacements
- World.Biome.WildEncounter WildEncounter

## World.Structure.InteriorSpace
Kind: Serializable
Composed of:
- string InteriorId
- SourceBuildingOrEntrance SourceBuildingOrEntrance
- World.Structure.InteriorPrefab InteriorPrefab
- List<GeneratedInteractiveObject> GeneratedInteractiveObjects
- List<Trainer.Definition> GeneratedTrainers

## World.Structure.TrainerPlacement
Kind: Serializable
Composed of:
- Trainer.Definition Trainer
- Vector3Int LocalPosition
- FacingDirection FacingDirection

## World.Structure.InteractiveObjectDefinition
Kind: ScriptableObject
Composed of:
- string DisplayName
- InteractionType InteractionType
- DefaultTriggerMode DefaultTriggerMode
- DefaultPayloadData DefaultPayloadData
- ValidationOrGenerationTags ValidationOrGenerationTags

## World.Structure.InteractiveObject
Kind: Serializable
Composed of:
- InteractiveObjectDefinition InteractiveObjectDefinition
- Vector3Int LocalPosition
- LocalDirection LocalDirection
- TriggerModeOverride TriggerModeOverride
- DestinationDataOrPayloadOverrides DestinationDataOrPayloadOverrides

## World.Structure.PlacedStructure
Kind: Serializable
Composed of:
- string PlacedStructureId
- Template Template
- Vector3Int WorldPosition
- Quaternion Rotation
- BoundsInt WorldBounds
- List<Vector2Int> CoveredChunkCoordinates
- string LocalSeedOrVariationId

## World.Structure.PlacementState
Kind: Serializable
Composed of:
- List<PlacedStructure> PlacedStructures
- List<OccupiedOrReservedWorldBound> OccupiedOrReservedWorldBounds
- List<string> PreloadOrGenerationStateFlags

## World.Structure.PlacementRule
Kind: ScriptableObject
Composed of:
- SourcePrefabOrTemplate SourcePrefabOrTemplate
- float Weight
- List<AllowedBiomeTag> AllowedBiomeTags
- List<SpacingConstraint> SpacingConstraints
- List<TerrainConstraint> TerrainConstraints
- QuantityOrDensityData QuantityOrDensityData

## World.Chunk
Kind: Namespace

### World.Chunk.Coordinates
Kind: Serializable
Composed of:
- int X
- int Y

### World.Chunk.Data
Kind: Serializable
Composed of:
- List<VoxelCellsStoringCompactVoxelDefinitionIndexe> VoxelCellsStoringCompactVoxelDefinitionIndexes

# Model.Voxel

## Voxel.Registry
Kind: ScriptableObject
Composed of:
- VoxelDefinitionList VoxelDefinitionList
- LookupByVoxelDefinitionIndex LookupByVoxelDefinitionIndex
- OptionalFallbackVoxelDefinition OptionalFallbackVoxelDefinition

## Voxel.Definition
Kind: ScriptableObject
Composed of:
- string DisplayName
- Voxel.Data VoxelData
- Voxel.Shape Shape

## Voxel.Data
Kind: Serializable
Composed of:
- TraversalType TraversalType
- bool BlocksLineOfSightFlag
- MovementCost MovementCost
- List<SurfaceTag> SurfaceTags
- List<OptionalWorldInteractionTag> OptionalWorldInteractionTags
- OptionalReplacementOrRemovedStateBehavior OptionalReplacementOrRemovedStateBehavior
- MaterialOrTextureData MaterialOrTextureData

## Voxel.Cell
Kind: Serializable
Composed of:
- VoxelDefinitionIndexResolvedThroughVoxelRegistry VoxelDefinitionIndexResolvedThroughVoxelRegistry
- Orientation Orientation
- FlipOrientation FlipOrientation

## Voxel.Traversal
Kind: Enum
Represents how a voxel behaves for movement and occupancy.
Values:
- Walkable: units may stand and move on this voxel normally.
- Obstacle: this voxel blocks normal movement.
- Water: this voxel is water terrain and follows water-specific movement rules.
- Climbable: this voxel may be traversed through climbing rules rather than normal walking.

## Voxel.Shape
Kind: Serializable
Composed of:
- List<RenderFace> RenderFaces
- List<CollisionFace> CollisionFaces
- List<OverlayFace> OverlayFaces
- CardinalPointSet CardinalPointSet
- List<SpriteReferencesUsedToBuildFaceUV> SpriteReferencesUsedToBuildFaceUVs

## Voxel.CardinalPointSet
Kind: Serializable
Composed of:
- Vector3 PositiveXPoint
- Vector3 NegativeXPoint
- Vector3 PositiveZPoint
- Vector3 NegativeZPoint
- Vector3 StationaryPoint

## Voxel.Mesher
Composed of:
- List<RenderMeshBuildRule> RenderMeshBuildRules
- List<CollisionMeshBuildRule> CollisionMeshBuildRules
- List<OverlayMaskMeshBuildRule> OverlayMaskMeshBuildRules

## Voxel.MaskSpriteRegistry
Kind: ScriptableObject
Composed of:
- SpriteByMaskType SpriteByMaskType

# Model.Creature

## Creature.Form
Kind: Serializable
Composed of:
- string IdentificationName
- string DisplayName
- List<FormTag> FormTags
- Sprite Icon
- FormTier FormTier
- ModelPrefab ModelPrefab
- AnimationSet AnimationSet
- List<OptionalMaterialOrVFXOverride> OptionalMaterialOrVFXOverrides

## Creature.Species
Kind: ScriptableObject
Composed of:
- string IdentificationName
- string DisplayName
- Creature.Stats BaseStats
- List<Action.Definition> DefaultActions
- Feat.Board Board
- List<Creature.Form> AvailableForms
- Dictionary<string, Creature.Form> AvailableFormsByName

## Creature.SpeciesRegistry
Kind: ScriptableObject
Composed of:
- List<Creature.Species> SpeciesAssets
- Dictionary<string, Creature.Species> SpeciesByName

## Creature.Unit
Kind: Serializable
Composed of:
- string UnitId
- Creature.Species Species
- Creature.Form CurrentForm
- string Nickname
- Feat.BoardProgress BoardProgress
- Creature.Stats AdditionalStats
- List<Action.Definition> UnlockedActions
- List<Action.UnitEffect> PersistentUnitEffects

## Creature.Stats
Kind: Serializable
Composed of:
- int Health
- int Strength
- int Ability
- int Armor
- int Resistance
- int ActionPoints
- int MovementPoints
- int Stamina
- int Range

## PCStorage
Kind: Serializable
Composed of:
- List<Creature.Unit> StoredUnits

# Model.Item

## Item.Definition
Kind: ScriptableObject
Composed of:
- string DisplayName
- string Description
- Sprite Icon
- bool Stackable
- int MaxStack
- bool Consumable

## Item.BattleItem
Kind: ScriptableObject
Inherits from: `Item.Definition`
Composed of:
- InheritedItemData InheritedItemData
- BattleTargetingRules BattleTargetingRules
- List<Action.Effect> BattleEffects

## Item.PassiveItem
Kind: ScriptableObject
Inherits from: `Item.Definition`
Composed of:
- InheritedItemData InheritedItemData
- Creature.Stats GrantedStatBonuses
- List<Action.UnitEffect> GrantedPassiveEffects
- DurationMode DurationMode

## Item.ActionItem
Kind: ScriptableObject
Inherits from: `Item.Definition`
Composed of:
- InheritedItemData InheritedItemData
- Action.Definition ActionToTeach
- TargetCreatureFilters TargetCreatureFilters

## Item.KeyItem
Kind: ScriptableObject
Inherits from: `Item.Definition`
Composed of:
- InheritedItemData InheritedItemData
- WorldUsageMeaning WorldUsageOrProgressionMeaning

## Item.Inventory
Kind: Serializable
Composed of:
- List<Item.Stack> ItemStacks

## Item.Stack
Kind: Serializable
Composed of:
- Item.Definition Item
- int Quantity

# Model.Action

## Action.Definition
Kind: ScriptableObject
Composed of:
- string DisplayName
- APCost APCost
- RangeData RangeData
- TargetingProfile TargetingProfile
- CastProfile CastProfile
- ActivationConditions ActivationConditions
- LineOfSightRule LineOfSightRule
- List<PolymorphicEffectsStoredThroughManagedReferenceSerialization> PolymorphicEffectsStoredThroughManagedReferenceSerialization
- PendingCastAnimationNameToRequestOnTheCaster PendingCastAnimationNameToRequestOnTheCaster
- CastReleaseAnimationNameToRequestOnTheCaster CastReleaseAnimationNameToRequestOnTheCaster
- bool UsableInBattleFlag
- bool UsableInWorldFlag
- List<WorldInteractionTag> WorldInteractionTags

## Action.TargetingProfile
Kind: Serializable
Composed of:
- ExpectedTargetSelectionKind ExpectedTargetSelectionKind
- List<MinimumSelectedTarget> MinimumSelectedTargets
- List<MaximumSelectedTarget> MaximumSelectedTargets
- SelectionOrderOrDuplicateSelectionRule SelectionOrderOrDuplicateSelectionRule
- AreaShape AreaShape
- List<Filter> Filters

## Action.CastProfile
Kind: Serializable
Composed of:
- CastModeImmediateDelayedCastOrImmediateWithRecovery CastModeImmediateDelayedCastOrImmediateWithRecovery
- DelayValueInTurnsOrSeconds DelayValueInTurnsOrSeconds
- TargetLockModeChooseAtStartOrChooseAtRelease TargetLockModeChooseAtStartOrChooseAtRelease
- bool BlockCasterWhilePendingFlag
- RecoveryValueInTurnsOrSeconds RecoveryValueInTurnsOrSeconds
- CancelConditionsOnMoveOnDamageOrOnDeath CancelConditionsOnMoveOnDamageOrOnDeath

## Action.ActivationCondition
Kind: Serializable
Composed of:
- TheDataSharedByAllActivationConditionSubtypesIfAny TheDataSharedByAllActivationConditionSubtypesIfAny

## Action.RequiredFormTagCondition
Kind: Serializable
Inherits from: `Action.ActivationCondition`
Composed of:
- List<RequiredFormTag> RequiredFormTags
- MatchAllOrMatchAnyRule MatchAllOrMatchAnyRule

## Action.ForbiddenFormTagCondition
Kind: Serializable
Inherits from: `Action.ActivationCondition`
Composed of:
- List<ForbiddenFormTag> ForbiddenFormTags
- MatchAllOrMatchAnyRule MatchAllOrMatchAnyRule

## Action.UnitEffectStackCondition
Kind: Serializable
Inherits from: `Action.ActivationCondition`
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- int MinimumRequiredStackCount

## Action.Effect
Kind: Serializable
Composed of:
- TheDataSharedByAllEffectSubtypesIfAny TheDataSharedByAllEffectSubtypesIfAny

## Action.ValueFormula
Kind: Serializable
Composed of:
- TheDataSharedByAllFormulaSubtypesIfAny TheDataSharedByAllFormulaSubtypesIfAny

## Action.ConstantFormula
Kind: Serializable
Composed of:
- int ConstantValue

## Action.StatFormula
Kind: Serializable
Composed of:
- StatSourceSourceOrTarget StatSourceSourceOrTarget
- StatType StatType
- float Multiplier

## Action.ResourceConsumedFormula
Kind: Serializable
Composed of:
- ConsumedResourceType ConsumedResourceType
- float Multiplier

## Action.StackCountFormula
Kind: Serializable
Composed of:
- StackSourceSourceOrTarget StackSourceSourceOrTarget
- ReferencedUnitEffect ReferencedUnitEffect
- float Multiplier

## Action.AddFormula
Kind: Serializable
Composed of:
- List<ChildFormula> ChildFormulas

## Action.MultiplyFormula
Kind: Serializable
Composed of:
- List<ChildFormula> ChildFormulas

## Action.DamageEffect
Kind: Serializable
Composed of:
- DamageFormula DamageFormula
- DamageType DamageType

## Action.HealEffect
Kind: Serializable
Composed of:
- HealFormula HealFormula

## Action.ReviveEffect
Kind: Serializable
Composed of:
- RevivedHealthFormula RevivedHealthFormula
- List<OptionalTargetFilter> OptionalTargetFilters

## Action.ApplyUnitEffect
Kind: Serializable
Composed of:
- UnitEffect UnitEffect
- UnitEffectDurationOverride UnitEffectDurationOverride
- StackChange StackChange

## Action.RemoveUnitEffect
Kind: Serializable
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- AmountToRemove AmountToRemove

## Action.CleanseEffect
Kind: Serializable
Composed of:
- FiltersDecidingWhatKindsOfUnitEffectsCanBeRemoved FiltersDecidingWhatKindsOfUnitEffectsCanBeRemoved
- AmountToRemove AmountToRemove

## Action.ResourceChangeEffect
Kind: Serializable
Composed of:
- ResourceType ResourceType
- SignedValueFormula SignedValueFormula

## Action.CreateBoardEffect
Kind: Serializable
Composed of:
- BoardEffect BoardEffect
- PlacementOrAreaRule PlacementOrAreaRule
- BoardEffectDurationOverride BoardEffectDurationOverride

## Action.RemoveBoardEffect
Kind: Serializable
Composed of:
- List<BoardEffectFilter> BoardEffectFilters
- AmountToRemove AmountToRemove

## Action.MoveUnitEffect
Kind: Serializable
Composed of:
- MovementMode MovementMode
- DistanceFormula DistanceFormula
- DestinationOrDirectionRule DestinationOrDirectionRule
- CollisionRule CollisionRule

## Action.SwapPositionEffect
Kind: Serializable
Composed of:
- List<TargetFilter> TargetFilters
- List<OptionalPlacementValidationRule> OptionalPlacementValidationRules

## Action.TeleportEffect
Kind: Serializable
Composed of:
- DestinationRule DestinationRule
- List<OptionalPlacementValidationRule> OptionalPlacementValidationRules

## Action.RecordPositionEffect
Kind: Serializable
Composed of:
- RecordedPositionSource RecordedPositionSource
- RecordTargetSelfOrOneTarget RecordTargetSelfOrOneTarget

## Action.StealResourceEffect
Kind: Serializable
Composed of:
- ResourceType ResourceType
- StolenAmountFormula StolenAmountFormula
- List<SourceAndDestinationFilter> SourceAndDestinationFilters

## Action.ConsumeUnitEffect
Kind: Serializable
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- ConsumeRule ConsumeRule
- ChildEffectOrValueFormulaProducedByTheConsumption ChildEffectOrValueFormulaProducedByTheConsumption

## Action.ConditionalEffect
Kind: Serializable
Composed of:
- ConditionData ConditionData
- List<SuccessEffect> SuccessEffects
- FailureEffects FailureEffects

## Action.ChangeFormEffect
Kind: Serializable
Composed of:
- TargetForm TargetForm
- TargetFilters TargetFilters

## Action.UnitEffect
Kind: Serializable
Composed of:
- string DisplayName
- DefaultUnitEffectDuration DefaultUnitEffectDuration
- List<EffectRule> EffectRules

## Action.UnitEffectDuration
Kind: Serializable
Composed of:
- DurationModePermanentTurnBasedOrTimeBased DurationModePermanentTurnBasedOrTimeBased
- float DurationValue

## Action.BoardEffect
Kind: Serializable
Composed of:
- string DisplayName
- DefaultBoardEffectDuration DefaultBoardEffectDuration
- List<BoardEffectRule> BoardEffectRules

## Action.BoardEffectDuration
Kind: Serializable
Composed of:
- DurationModeUntilTriggeredTurnBasedOrTimeBased DurationModeUntilTriggeredTurnBasedOrTimeBased
- float DurationValue

## Action.BoardEffectRule
Kind: Serializable
Composed of:
- HookPoint HookPoint
- List<FiltersDecidingWhenTheRuleApply> FiltersDecidingWhenTheRuleApplies
- ChildEffectsOrChildActionsDependingOnTheRuleSubtype ChildEffectsOrChildActionsDependingOnTheRuleSubtype

## Action.BoardEffectHookPoint
Kind: Enum
Represents when a board effect rule is evaluated.
Values:
- WhenTheBoardEffectIsCreated: fire when the board effect is first placed.
- WhenItsTimerEnds: fire when the effect duration expires naturally.
- WhenAUnitEntersTheAffectedCells: fire when a unit steps into an affected cell.
- WhenAUnitStartsItsTurnInTheAffectedCells: fire at turn start for a unit already inside the area.
- WhenAUnitEndsItsTurnInTheAffectedCells: fire at turn end for a unit remaining inside the area.
- WhenAUnitLeavesTheAffectedCells: fire when a unit exits the affected area.
- WhenTheBoardEffectIsTriggered: fire when the board effect explicitly resolves its trigger behavior.

## Action.UnitEffectHookPoint
Kind: Enum
Represents when a unit effect rule is evaluated on its owner.
Values:
- BeforeValidatingWhetherAnActionCanBeUsed: fire before checking whether an action is currently usable.
- BeforeValidatingTargetsOrAffectedCells: fire before validating targets or affected cells.
- BeforePayingAResourceCost: fire before resources such as AP or stamina are spent.
- BeforeDealingOutgoingDamage: fire before the owner deals damage.
- BeforeReceivingIncomingDamage: fire before the owner receives damage.
- BeforeApplyingAnEffect: fire before another effect is applied to the owner or by the owner, depending on the rule.
- AtTurnStart: fire at the start of the owner's turn.
- AtTurnEnd: fire at the end of the owner's turn.
- WhenMoving: fire when the owner moves.
- WhenConsumingAResource: fire when the owner consumes a tracked resource.

## Action.UnitEffectRule
Kind: Serializable
Composed of:
- HookPoint HookPoint
- List<FiltersDecidingWhenTheRuleApply> FiltersDecidingWhenTheRuleApplies
- ValueFormulaOrChildEffectDependingOnTheRuleSubtype ValueFormulaOrChildEffectDependingOnTheRuleSubtype

# Model.Feat

## Feat.Board
Kind: ScriptableObject
Composed of:
- List<Feat.Node> Nodes
- Feat.Node StartingNode

## Feat.Node
Kind: Serializable
Composed of:
- string DisplayName
- Vector2Int BoardPosition
- List<Feat.Requirement> Requirements
- List<AdjacentNode> AdjacentNodes

## Feat.ActionNode
Kind: Serializable
Composed of:
- InheritedNodeData InheritedNodeData
- ActionDefinitionToUnlock ActionDefinitionToUnlock

## Feat.StatNode
Kind: Serializable
Composed of:
- InheritedNodeData InheritedNodeData
- List<StatBonus> StatBonuses

## Feat.PassiveNode
Kind: Serializable
Composed of:
- InheritedNodeData InheritedNodeData
- UnitEffectToGrantPermanently UnitEffectToGrantPermanently

## Feat.FormNode
Kind: Serializable
Composed of:
- InheritedNodeData InheritedNodeData
- List<FormNameToApplyResolvedThroughTheSpecy> FormNameToApplyResolvedThroughTheSpecies

## Feat.Link
Kind: Serializable
Composed of:
- SourceNode SourceNode
- TargetNode TargetNode

## Feat.Requirement
Kind: Serializable
Composed of:
- TheDataSharedByAllRequirementSubtypesIfAny TheDataSharedByAllRequirementSubtypesIfAny

## Feat.RequirementScope
Kind: Enum
Represents how long requirement progress should be accumulated before it resets.
Values:
- Lifetime: progress persists across the whole creature lifetime or run.
- SingleBattle: progress resets when the current battle ends.
- SingleTurn: progress resets at the end of the current turn.
- SingleAction: progress is only measured during one action resolution.

## Feat.DamageTypeFilter
Kind: Enum
Represents which damage category a feat requirement should accept.
Values:
- PhysicalOnly: only physical damage counts.
- MagicalOnly: only magical damage counts.
- BothOrAny: either physical or magical damage counts.

## Feat.ActionUseRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<ActionFilter> ActionFilters
- int TargetUseCount
- RequirementScope RequirementScope
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.ActionHitRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<ActionFilter> ActionFilters
- int TargetHitCount
- RequirementScope RequirementScope
- List<OptionalTargetFilter> OptionalTargetFilters
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.ActionTargetCountRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<ActionFilter> ActionFilters
- int MinimumSelectedOrAffectedTargetCount
- RequirementScope RequirementScope
- List<OptionalTargetSelectionFilter> OptionalTargetSelectionFilters

## Feat.ActionSequenceRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<OrderedActionFilter> OrderedActionFilters
- int TargetSequenceCount
- RequirementScope RequirementScope
- List<OptionalMaximumDelayBetweenSequenceStep> OptionalMaximumDelayBetweenSequenceSteps

## Feat.DamageDealtRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetDamageAmount
- DamageTypeFilter DamageTypeFilter
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters
- List<OptionalTargetFilter> OptionalTargetFilters

## Feat.DamageTakenRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetDamageAmount
- DamageTypeFilter DamageTypeFilter
- RequirementScope RequirementScope
- List<OptionalSourceFilter> OptionalSourceFilters
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.HealingDoneRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetHealingAmount
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters
- List<OptionalTargetFilter> OptionalTargetFilters

## Feat.ResourceSpentRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- ResourceType ResourceType
- int TargetSpentAmount
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters

## Feat.UnitEffectAppliedRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- int TargetApplicationCount
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters

## Feat.UnitEffectReceivedRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- int TargetReceiveCount
- RequirementScope RequirementScope
- List<OptionalSourceFilter> OptionalSourceFilters

## Feat.CleanseRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- int TargetCleanseCount
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters

## Feat.UnitDefeatedRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetCount
- RequirementScope RequirementScope
- List<OptionalTargetFilter> OptionalTargetFilters
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.CaptureCountRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetCaptureCount
- RequirementScope RequirementScope
- List<OptionalTargetFilter> OptionalTargetFilters

## Feat.BattleCountRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetBattleCount
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.BattleVictoryRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetVictoryCount
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.MoveDistanceRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetMovedDistance
- RequirementScope RequirementScope
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.KeepDistanceRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- MinimumDistanceFromEveryEnemy MinimumDistanceFromEveryEnemy
- int TargetSafeTurnCountOrBattleCount
- RequirementScope RequirementScope
- bool OptionalConsecutiveRequirementFlag
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.AdjacentToEnemyRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetAdjacencyCount
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters
- List<OptionalTargetFilter> OptionalTargetFilters

## Feat.BoardAreaOccupationRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<BoardAreaFilter> BoardAreaFilters
- TargetOccupationCountOrDuration TargetOccupationCountOrDuration
- RequirementScope RequirementScope

## Feat.BoardEffectTriggeredRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- List<BoardEffectFilter> BoardEffectFilters
- int TargetTriggerCount
- RequirementScope RequirementScope
- List<OptionalActionFilter> OptionalActionFilters

## Feat.NoDamageBattleRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int TargetBattleCount
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.LowHealthSurvivalRequirement
Kind: Serializable
Inherits from: `Feat.Requirement`
Composed of:
- int HealthThresholdValue
- int TargetBattleCount
- List<OptionalBattleContextFilter> OptionalBattleContextFilters

## Feat.BoardProgress
Kind: Serializable
Composed of:
- Feat.Board Board
- List<Feat.Node> CompletedNodes
- List<Feat.Node> ActiveNodes
- List<Feat.NodeProgress> NodeProgressEntries

## Feat.NodeProgress
Kind: Serializable
Composed of:
- Node Node
- bool CompletionFlag
- List<PerRequirementCurrentValue> PerRequirementCurrentValues

# Model.Encounter

## Encounter.Definition
Kind: Serializable
Composed of:
- Battle.BoardConfiguration BattleBoardConfiguration
- Encounter.Table EncounterTable

## Encounter.Unit
Kind: Serializable
Inherits from: `Creature.Unit`
Composed of:
- Creature.Unit BaseUnitData
- Battle.AI.Profile AIProfile

## Encounter.Team
Kind: Serializable
Composed of:
- List<Encounter.Unit> Units

## Encounter.Table
Kind: ScriptableObject
Composed of:
- Dictionary<int, Encounter.Tier> TiersByBadgeCount

## Encounter.Tier
Kind: Serializable
Composed of:
- List<EncounterTeamsLinkedToEncounterWeight> EncounterTeamsLinkedToEncounterWeights

# Model.Battle

## Battle.Input
Composed of:
- Encounter.Team EnemyEncounterTeam
- BattleType BattleType
- Vector3Int SourceWorldPosition
- BoundsInt SourceVoxelArea
- bool CaptureAllowed

## Battle.State
Composed of:
- Battle.Input BattleInput
- Battle.Board Board
- List<Battle.Team> Teams
- Battle.PlacementState PlacementState
- List<Battle.TurnEntry> TurnEntries
- Battle.Unit ActiveUnit
- Battle.Result BattleResult

## Battle.Team
Composed of:
- BattleSide Side
- List<Battle.Unit> BattleUnits

## Battle.Unit
Composed of:
- SourceCreatureUnit SourceCreatureUnit
- int CurrentHealth
- int CurrentActionPoints
- int CurrentMovementPoints
- Vector2Int BoardPosition
- List<Battle.AppliedUnitEffect> AppliedUnitEffects
- Battle.PendingAction CurrentPendingAction
- bool BlockedOrRecoveringFlag
- bool IsAlive

## Battle.BoardConfiguration
Kind: Serializable
Composed of:
- BoardSourceMode BoardSourceMode
- BoardSizeOrBoundsRule BoardSizeOrBoundsRule
- BoardShapeRule BoardShapeRule
- WalkableSurfaceSamplingRule WalkableSurfaceSamplingRule
- SourceWorldOrInteriorSamplingRule SourceWorldOrInteriorSamplingRule
- TriggerBoundsOrSourceInteractionBounds TriggerBoundsOrSourceInteractionBounds
- List<AcceptablePlayerDeploymentPatternType> AcceptablePlayerDeploymentPatternTypes
- List<AcceptableEnemyDeploymentPatternType> AcceptableEnemyDeploymentPatternTypes
- PatternWeightsOrPriorities PatternWeightsOrPriorities
- AutoPlacementBehavior AutoPlacementBehavior
- bool DimOutsideAreaFlag

## Battle.Board
Composed of:
- List<SourceVoxelBound> SourceVoxelBounds
- Dictionary<Vector2Int, Battle.BoardCell> CellsByCoordinates

## Battle.BoardCell
Composed of:
- List<Battle.OverlayMaskType> Masks
- OccupyingBattleUnitIfAny OccupyingBattleUnitIfAny
- List<Battle.AppliedBoardEffect> BoardEffects

## Battle.OverlayMaskType
Kind: Enum
Represents a visual overlay drawn on battle board cells.
Values:
- Deployment: cells where a unit may be placed during setup.
- MovementRange: cells reachable by the currently selected movement.
- ActionRange: cells within the currently selected action range.
- Selection: cells currently selected by the player.
- TargetPreview: cells previewed as affected by the pending action.

## Battle.DeploymentZone
Composed of:
- BattleSide Side
- List<Vector2Int> AllowedBoardCoordinates

## Battle.DeploymentPatternType
Kind: Enum
Represents the high-level shape used to build deployment zones on the board.
Values:
- Split: divide the board into two opposing sides, one for each team.
- Spot: choose root cells for each side, then grow them into clustered patches.
- Random: distribute placement cells randomly for each side.
- Quarter: divide the board into quarters and assign separated regions to each side.
- Ambush: place the player near the center and surround them with enemy deployment zones.
- Surprise: place the enemy near the center and surround them with player deployment zones.

## Battle.PlacementResolver
Composed of:
- SharedDeploymentGenerationRulesIfCentralized SharedDeploymentGenerationRulesIfCentralized
- PatternValidationLogic PatternValidationLogic
- DeploymentZoneGenerationLogic DeploymentZoneGenerationLogic

## Battle.PlacementState
Composed of:
- ResolvedDeploymentZonesBySide ResolvedDeploymentZonesBySide
- CurrentPlacedBattleUnitsDuringDeployment CurrentPlacedBattleUnitsDuringDeployment
- bool EnemyPlacementCompletionFlag
- CurrentSetupStep CurrentSetupStep

## Battle.TurnEntry
Composed of:
- Battle.Unit BattleUnit
- List<CurrentFillOrProgress> CurrentFillOrProgress
- bool IsReady
- bool IsPaused

## Battle.BattleCommand
Composed of:
- ActingBattleUnit ActingBattleUnit
- CommandType CommandType

## Battle.TargetSelection
Composed of:
- TheDataSharedByAllTargetSelectionSubtypesIfAny TheDataSharedByAllTargetSelectionSubtypesIfAny

## Battle.BoardCellTargetSelection
Composed of:
- List<Vector2Int> SelectedBoardCoordinates

## Battle.UnitTargetSelection
Composed of:
- List<SelectedBattleUnit> SelectedBattleUnits

## Battle.DirectionTargetSelection
Composed of:
- Vector2Int SourceBoardCoordinate
- ChosenDirection ChosenDirection

## Battle.PathTargetSelection
Composed of:
- List<Vector2Int> PathCoordinates

## Battle.MoveCommand
Inherits from: `Battle.BattleCommand`
Composed of:
- InheritedCommandData InheritedCommandData
- Vector2Int DestinationBoardCoordinate

## Battle.ActionCommand
Inherits from: `Battle.BattleCommand`
Composed of:
- InheritedCommandData InheritedCommandData
- ActionDefinition ActionDefinition
- TargetSelection TargetSelection

## Battle.PendingAction
Composed of:
- SourceBattleUnit SourceBattleUnit
- ActionDefinition ActionDefinition
- StoredTargetSelectionIfChosenAtCastStart StoredTargetSelectionIfChosenAtCastStart
- DelayDurationState DelayDurationState
- bool BlockedCasterFlag
- RecoveryDurationState RecoveryDurationState
- CancelConditionsCopiedFromTheActionCastProfile CancelConditionsCopiedFromTheActionCastProfile

## Battle.CaptureCommand
Inherits from: `Battle.BattleCommand`
Composed of:
- InheritedCommandData InheritedCommandData
- TargetBattleUnit TargetBattleUnit

## Battle.CaptureRule
Composed of:
- HPThresholdRule HPThresholdRule
- SuccessChanceRule SuccessChanceRule
- List<AllowedEncounterCategory> AllowedEncounterCategories

## Battle.DurationState
Composed of:
- DurationMode DurationMode
- RemainingTurnsIfTurnBased RemainingTurnsIfTurnBased
- float RemainingSecondsIfTimeBased
- bool ExpiredFlag

## Battle.DurationController
Composed of:
- DurationUpdateLogic DurationUpdateLogic
- ExpirationCheckLogic ExpirationCheckLogic

## Battle.AppliedUnitEffect
Composed of:
- UnitEffect UnitEffect
- SourceBattleUnit SourceBattleUnit
- DurationState DurationState
- CurrentStacks CurrentStacks

## Battle.AppliedBoardEffect
Composed of:
- BoardEffect BoardEffect
- SourceBattleUnit SourceBattleUnit
- List<Vector2Int> AffectedBoardCoordinates
- DurationState DurationState
- bool TriggeredFlag

## Battle.Result
Composed of:
- BattleSide WinningSide
- CapturedUnitIfAny CapturedUnitIfAny
- FeatProgressSummary FeatProgressSummary
- ClearFlagsToApplyAfterBattle ClearFlagsToApplyAfterBattle

## Battle.AI
Kind: Namespace

### Battle.AI.Profile
Kind: ScriptableObject
Composed of:
- List<OrderedRule> OrderedRules

### Battle.AI.Rule
Kind: Serializable
Composed of:
- List<Condition> Conditions
- ActionChoiceData ActionChoiceData

### Battle.AI.Condition
Kind: Serializable
Composed of:
- TheDataSharedByAllAIConditionSubtypesIfAny TheDataSharedByAllAIConditionSubtypesIfAny

### Battle.AI.EnemyWithinRangeCondition
Kind: Serializable
Inherits from: `Battle.AI.Condition`
Composed of:
- ActionOrRangeSource ActionOrRangeSource
- List<TargetFilter> TargetFilters

### Battle.AI.AllyHealthBelowCondition
Kind: Serializable
Inherits from: `Battle.AI.Condition`
Composed of:
- int HealthThresholdValue
- List<TargetFilter> TargetFilters

### Battle.AI.ResourceAtLeastCondition
Kind: Serializable
Inherits from: `Battle.AI.Condition`
Composed of:
- ResourceType ResourceType
- int MinimumRequiredValue

### Battle.AI.TargetHasUnitEffectCondition
Kind: Serializable
Inherits from: `Battle.AI.Condition`
Composed of:
- List<UnitEffectFilter> UnitEffectFilters
- PresenceOrAbsenceRule PresenceOrAbsenceRule
- List<TargetFilter> TargetFilters

### Battle.AI.CanUseActionCondition
Kind: Serializable
Inherits from: `Battle.AI.Condition`
Composed of:
- List<ActionFilter> ActionFilters
- List<OptionalTargetFilter> OptionalTargetFilters

### Battle.AI.ActionChoice
Kind: Serializable
Composed of:
- ActionType ActionType
- PreferredAction PreferredAction
- TargetPreference TargetPreference
- MovementPreference MovementPreference

# Model.Progression

## RunProgress
Kind: Serializable
Composed of:
- Dictionary<string, Progression.SourceState> SourceStatesById
- List<Progression.MilestoneFlag> UnlockedMilestoneFlags

## Progression.SourceState
Kind: Serializable
Composed of:
- string SourceId
- string SourceKind
- bool Enabled
- bool Cleared
- int StateValue

## Progression.Badge
Kind: ScriptableObject
Composed of:
- string DisplayName
- UnityEngine.Object IconAsset
- string Description
- OptionalProgressionTagsOrMetadata OptionalProgressionTagsOrMetadata

## Progression.MilestoneFlag
Kind: ScriptableObject
Composed of:
- string DisplayName
- string Description

# Model.Trainer

## Trainer.Definition
Kind: ScriptableObject
Composed of:
- string DisplayName
- Encounter.Definition EncounterDefinition
- string StartBattleDialogue
- string PlayerVictoryDialogue
- string PlayerDefeatDialogue
- List<Trainer.Reward> Rewards

## Trainer.Reward
Kind: Serializable
Composed of:
- TheDataSharedByAllTrainerRewardSubtypesIfAny TheDataSharedByAllTrainerRewardSubtypesIfAny

## Trainer.GiveMoneyReward
Kind: Serializable
Inherits from: `Trainer.Reward`
Composed of:
- Amount Amount

## Trainer.GiveItemReward
Kind: Serializable
Inherits from: `Trainer.Reward`
Composed of:
- Item.Definition ItemDefinition
- Amount Amount

## Trainer.GiveBadgeReward
Kind: Serializable
Inherits from: `Trainer.Reward`
Composed of:
- Progression.Badge Badge

## Trainer.UnlockMilestoneReward
Kind: Serializable
Inherits from: `Trainer.Reward`
Composed of:
- Progression.MilestoneFlag MilestoneFlag

# Model.Save

## SaveData
Kind: Serializable
Composed of:
- Core.GameRun Run
- SaveTimestamp SaveTimestamp
- SaveSlotId SaveSlotId

# View.Animation

## View.Animation.Recipe
Kind: ScriptableObject
Composed of:
- string DisplayName
- List<Phas> Phases
- bool LoopFlag

## View.Animation.Set
Kind: ScriptableObject
Composed of:
- Dictionary<AnimationName, Recipe> RecipeByAnimationName
- List<IdleAnimationNameToAllowTheAnimationToReturnToASpecificAnimationLoopWhenEndingTheOther> IdleAnimationNameToAllowTheAnimationToReturnToASpecificAnimationLoopWhenEndingTheOthers

## View.Animation.Phase
Kind: Serializable
Composed of:
- float Duration
- List<Step> Steps

## View.Animation.Step
Kind: Serializable
Composed of:
- TheDataSharedByAllStepSubtypesIfAny TheDataSharedByAllStepSubtypesIfAny

## View.Animation.MoveLocalStep
Kind: Serializable
Composed of:
- TargetLogicalPart TargetLogicalPart
- LocalOffset LocalOffset
- EasingCurve EasingCurve
- bool AdditiveFlag

## View.Animation.RotateLocalStep
Kind: Serializable
Composed of:
- TargetLogicalPart TargetLogicalPart
- LocalRotationOffset LocalRotationOffset
- EasingCurve EasingCurve
- bool AdditiveFlag

## View.Animation.ScaleStep
Kind: Serializable
Composed of:
- TargetLogicalPart TargetLogicalPart
- LocalScaleMultiplierOrOffset LocalScaleMultiplierOrOffset
- EasingCurve EasingCurve
- bool AdditiveFlag

## View.Animation.ShakeStep
Kind: Serializable
Composed of:
- TargetLogicalPart TargetLogicalPart
- float Amplitude
- float Frequency
- EasingCurve EasingCurve

## View.Animation.FlashStep
Kind: Serializable
Composed of:
- TargetScopeWholeRigOrSpecificPart TargetScopeWholeRigOrSpecificPart
- ColorOrFlashStyle ColorOrFlashStyle
- float Intensity
- EasingCurve EasingCurve

## View.Animation.WaitStep
Kind: Serializable
Composed of:
- OptionalNoteOrLabel OptionalNoteOrLabel

## View.Animation.SpawnVfxStep
Kind: Serializable
Composed of:
- TargetLogicalPartOrAnchor TargetLogicalPartOrAnchor
- VFXReference VFXReference
- LocalOffset LocalOffset

## View.Animation.PlaySoundStep
Kind: Serializable
Composed of:
- SoundReference SoundReference
- TargetLogicalPartOrWorldAnchor TargetLogicalPartOrWorldAnchor

## View.Animation.LogicalPart
Kind: Enum
Represents a logical rig slot that animation steps may target.
Values:
- Root: the global root of the animated model.
- Body: the main torso or central mass.
- Head: the head or head-equivalent part.
- Front: the front-facing body section for creatures with directional bodies.
- Rear: the rear body section for creatures with directional bodies.
- DominantLimb: the primary arm, claw, wing, or striking limb.
- OffLimb: the secondary arm, claw, wing, or support limb.
- Weapon: a held or attached weapon element.
- Jaw: the mouth or jaw portion used for bite-like animation.
- Tail: the tail or rear appendage.
- WholeRig: the full model as one animation target.

## View.Animation.Rig
Composed of:
- Dictionary<LogicalPart, Transform> TransformByLogicalPart

## View.Animation.Animator
Composed of:
- AnimationRig AnimationRig
- AnimationSet AnimationSet
- CurrentMainRecipeState CurrentMainRecipeState
- CurrentOverlayRecipeState CurrentOverlayRecipeState
- List<RuntimePoseOffset> RuntimePoseOffsets

