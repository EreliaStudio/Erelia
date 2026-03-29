public enum VoxelTraversal
{
	Obstacle = 0,
	Walkable = 1
}

public enum VoxelOrientation
{
	PositiveX = 0,
	PositiveZ = 1,
	NegativeX = 2,
	NegativeZ = 3
}

public enum VoxelFlipOrientation
{
	PositiveY = 0,
	NegativeY = 1
}

public enum VoxelAxisPlane
{
	PosX = 0,
	NegX = 1,
	PosY = 2,
	NegY = 3,
	PosZ = 4,
	NegZ = 5
}

public enum MaskType
{
	None = 0,
	Placement = 1,
	AttackRange = 2,
	MovementRange = 3,
	AreaOfEffect = 4,
	Selected = 5
}
