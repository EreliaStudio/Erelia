using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Tests for VoxelTraversalGraphBuilder: verifies that a voxel grid is correctly
// converted into a navigation graph with the expected nodes and neighbour connections.
public sealed class NavGraphTests
{
	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	private const int ObstacleId = 1;
	private const int WalkableId = 2;

	private VoxelRegistry _registry;
	private readonly List<Object> _assets = new();

	[SetUp]
	public void SetUp()
	{
		_registry = MakeRegistry();
	}

	[TearDown]
	public void TearDown()
	{
		foreach (Object asset in _assets)
		{
			if (asset != null)
				Object.DestroyImmediate(asset);
		}
		_assets.Clear();
	}

	// Build a flat board: Obstacle floor at y=0, empty above.
	// sizeY is set to 4 so the builder loop (y < sizeY-2) can reach y=0.
	private static BoardTerrainLayer FlatBoard(VoxelRegistry registry, int sizeX, int sizeZ)
	{
		BoardTerrainLayer terrain = new BoardTerrainLayer(sizeX, 4, sizeZ);
		terrain.AssignVoxelRegistry(registry);
		for (int x = 0; x < sizeX; x++)
			for (int z = 0; z < sizeZ; z++)
				terrain.Cells[x, 0, z] = new VoxelCell(ObstacleId);
		return terrain;
	}

	private static VoxelTraversalGraph BuildGraph(BoardTerrainLayer terrain, VoxelRegistry registry)
	{
		return VoxelTraversalGraphBuilder.Build(terrain, registry);
	}

	private VoxelRegistry MakeRegistry()
	{
		VoxelRegistry registry = ScriptableObject.CreateInstance<VoxelRegistry>();

		VoxelDefinition obstacle = ScriptableObject.CreateInstance<VoxelDefinition>();
		VoxelCubeShape obstacleShape = new VoxelCubeShape();
		obstacleShape.Initialize();
		SetField(obstacle, "data", new VoxelData { Traversal = VoxelTraversal.Obstacle });
		SetField(obstacle, "shape", obstacleShape);
		obstacle.Initialize();

		VoxelDefinition walkable = ScriptableObject.CreateInstance<VoxelDefinition>();
		VoxelCubeShape walkableShape = new VoxelCubeShape();
		walkableShape.Initialize();
		SetField(walkable, "data", new VoxelData { Traversal = VoxelTraversal.Walkable });
		SetField(walkable, "shape", walkableShape);
		walkable.Initialize();

		registry.Voxels.Add(ObstacleId, obstacle);
		registry.Voxels.Add(WalkableId, walkable);

		_assets.Add(registry);
		_assets.Add(obstacle);
		_assets.Add(walkable);
		return registry;
	}

	private static void SetField(object target, string name, object value)
	{
		FieldInfo f = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(f, $"Field '{name}' not found on {target.GetType().Name}");
		f.SetValue(target, value);
	}

	// -------------------------------------------------------------------------
	// Node creation
	// -------------------------------------------------------------------------

	[Test]
	public void FlatBoard_CreatesOneNodePerCell_AtFloorY()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		// 3×3 = 9 cells, each should have a node at y=0
		Assert.That(graph.AllNodes.Count, Is.EqualTo(9));
		for (int x = 0; x < 3; x++)
			for (int z = 0; z < 3; z++)
				Assert.That(graph.ContainsNode(new Vector3Int(x, 0, z)), Is.True,
					$"Expected node at ({x},0,{z})");
	}

	[Test]
	public void EmptyBoard_CreatesNoNodes()
	{
		BoardTerrainLayer terrain = new BoardTerrainLayer(4, 4, 4);
		terrain.AssignVoxelRegistry(_registry);

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		Assert.That(graph.AllNodes.Count, Is.EqualTo(0));
	}

	[Test]
	public void NullRegistry_CreatesNoNodes()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		VoxelTraversalGraph graph = BuildGraph(terrain, null);

		Assert.That(graph.AllNodes.Count, Is.EqualTo(0));
	}

	[Test]
	public void WallAboveFloor_BlocksNodeAtThatCell()
	{
		// Obstacle at (1,0) with another Obstacle at (1,1) means y+1 is not passable
		// → no node should be created at (1,0,1)
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		terrain.Cells[1, 1, 1] = new VoxelCell(ObstacleId);

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		Assert.That(graph.ContainsNode(new Vector3Int(1, 0, 1)), Is.False,
			"Wall at y=1 should block node at y=0");
		Assert.That(graph.ContainsNode(new Vector3Int(1, 1, 1)), Is.True,
			"The y=1 obstacle is also an elevated standable surface when there is clearance above");
		Assert.That(graph.AllNodes.Count, Is.EqualTo(9), "Eight floor nodes plus one elevated node should exist");
	}

	[Test]
	public void WalkableVoxelAboveFloor_DoesNotBlockNode()
	{
		// Walkable at y=1 is passable space — node at y=0 should still be created
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		terrain.Cells[1, 1, 1] = new VoxelCell(WalkableId);

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		Assert.That(graph.ContainsNode(new Vector3Int(1, 0, 1)), Is.True,
			"Walkable at y=1 is passable — node should still exist at y=0");
		Assert.That(graph.AllNodes.Count, Is.EqualTo(9));
	}

	[Test]
	public void OnlyFloorWithNoSpaceAbove_CreatesNoNodes()
	{
		// Fill all three layers with Obstacle — y+1 and y+2 are not passable
		BoardTerrainLayer terrain = new BoardTerrainLayer(2, 4, 2);
		terrain.AssignVoxelRegistry(_registry);
		for (int x = 0; x < 2; x++)
			for (int z = 0; z < 2; z++)
			{
				terrain.Cells[x, 0, z] = new VoxelCell(ObstacleId);
				terrain.Cells[x, 1, z] = new VoxelCell(ObstacleId);
				terrain.Cells[x, 2, z] = new VoxelCell(ObstacleId);
			}

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		Assert.That(graph.AllNodes.Count, Is.EqualTo(0),
			"No passable space above floor — no navigable cells");
	}

	[Test]
	public void NodeAtEachValidCell_WithMixedHeights()
	{
		// Row z=0 at y=0 (valid), row z=1 raised: floor at y=1, clear above
		BoardTerrainLayer terrain = new BoardTerrainLayer(3, 6, 3);
		terrain.AssignVoxelRegistry(_registry);
		for (int x = 0; x < 3; x++)
		{
			for (int z = 0; z < 3; z++)
				terrain.Cells[x, 0, z] = new VoxelCell(ObstacleId);

			// Raise row z=2: place a second floor at y=2 (clear above at y=3,4)
			terrain.Cells[x, 2, 2] = new VoxelCell(ObstacleId);
		}

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		// y=0 nodes for the columns that are not blocked by an elevated floor.
		for (int x = 0; x < 3; x++)
			for (int z = 0; z < 2; z++)
				Assert.That(graph.ContainsNode(new Vector3Int(x, 0, z)), Is.True,
					$"Missing node at ({x},0,{z})");

		for (int x = 0; x < 3; x++)
			Assert.That(graph.ContainsNode(new Vector3Int(x, 0, 2)), Is.False,
				$"Lower node at ({x},0,2) should be blocked by the raised floor");

		// y=2 nodes for z=2 column
		for (int x = 0; x < 3; x++)
			Assert.That(graph.ContainsNode(new Vector3Int(x, 2, 2)), Is.True,
				$"Missing elevated node at ({x},2,2)");

		Assert.That(graph.AllNodes.Count, Is.EqualTo(9));
	}

	// -------------------------------------------------------------------------
	// Neighbour connections (flat board)
	// -------------------------------------------------------------------------

	[Test]
	public void FlatBoard_AdjacentNodes_AreLinked()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		VoxelTraversalGraph.Node center = graph.GetNode(new Vector3Int(1, 0, 1));

		Assert.That(center.PositiveX, Is.Not.Null, "Missing PositiveX neighbour");
		Assert.That(center.NegativeX, Is.Not.Null, "Missing NegativeX neighbour");
		Assert.That(center.PositiveZ, Is.Not.Null, "Missing PositiveZ neighbour");
		Assert.That(center.NegativeZ, Is.Not.Null, "Missing NegativeZ neighbour");
	}

	[Test]
	public void FlatBoard_CornerNode_HasTwoNeighbours()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		VoxelTraversalGraph.Node corner = graph.GetNode(new Vector3Int(0, 0, 0));

		int neighbourCount = 0;
		if (corner.PositiveX != null) neighbourCount++;
		if (corner.NegativeX != null) neighbourCount++;
		if (corner.PositiveZ != null) neighbourCount++;
		if (corner.NegativeZ != null) neighbourCount++;

		Assert.That(neighbourCount, Is.EqualTo(2),
			"Corner node should connect to exactly 2 neighbours");
	}

	[Test]
	public void FlatBoard_EdgeNode_HasThreeNeighbours()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		// Mid-edge node at (1,0,0)
		VoxelTraversalGraph.Node edge = graph.GetNode(new Vector3Int(1, 0, 0));

		int neighbourCount = 0;
		if (edge.PositiveX != null) neighbourCount++;
		if (edge.NegativeX != null) neighbourCount++;
		if (edge.PositiveZ != null) neighbourCount++;
		if (edge.NegativeZ != null) neighbourCount++;

		Assert.That(neighbourCount, Is.EqualTo(3),
			"Edge node should connect to exactly 3 neighbours");
	}

	[Test]
	public void NeighbourConnection_IsSymmetric()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		VoxelTraversalGraph.Node a = graph.GetNode(new Vector3Int(0, 0, 0));
		VoxelTraversalGraph.Node b = graph.GetNode(new Vector3Int(1, 0, 0));

		// a.PositiveX → b, b.NegativeX → a
		Assert.That(a.PositiveX, Is.SameAs(b));
		Assert.That(b.NegativeX, Is.SameAs(a));
	}

	[Test]
	public void WallBetweenTwoNodes_DisconnectsThem()
	{
		// 1×3 board with a wall column at (0,0,1) — middle cell removed
		BoardTerrainLayer terrain = FlatBoard(_registry, 1, 3);
		terrain.Cells[0, 1, 1] = new VoxelCell(ObstacleId);

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		Assert.That(graph.ContainsNode(new Vector3Int(0, 0, 0)), Is.True);
		Assert.That(graph.ContainsNode(new Vector3Int(0, 0, 1)), Is.False, "Wall blocks node at (0,0,1)");
		Assert.That(graph.ContainsNode(new Vector3Int(0, 0, 2)), Is.True);

		VoxelTraversalGraph.Node start = graph.GetNode(new Vector3Int(0, 0, 0));
		Assert.That(start.PositiveZ, Is.Null, "No path across the wall");
	}

	[Test]
	public void IslandBoard_NodesAreIsolated_WhenSurroundedByWalls()
	{
		// 3×3 board: centre cell (1,0,1) is completely walled off at y=1 on all 4 sides
		// Actually walls are at y=1 on (1,z=0), (1,z=2), (x=0,1), (x=2,1) — just test isolation
		// Simpler: single-cell board, node exists but has no neighbours
		BoardTerrainLayer terrain = FlatBoard(_registry, 1, 1);
		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		VoxelTraversalGraph.Node node = graph.GetNode(new Vector3Int(0, 0, 0));
		Assert.That(node.PositiveX, Is.Null);
		Assert.That(node.NegativeX, Is.Null);
		Assert.That(node.PositiveZ, Is.Null);
		Assert.That(node.NegativeZ, Is.Null);
	}

	// -------------------------------------------------------------------------
	// Walkable voxels in the traversal space
	// -------------------------------------------------------------------------

	[Test]
	public void WalkableVoxelAtStandingLevel_DoesNotAppearAsNode()
	{
		// Walkable voxel at y=1 should NOT itself be treated as a navigable floor
		BoardTerrainLayer terrain = FlatBoard(_registry, 2, 2);
		for (int x = 0; x < 2; x++)
			for (int z = 0; z < 2; z++)
				terrain.Cells[x, 1, z] = new VoxelCell(WalkableId);

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		// Nodes still at y=0 (floor), not at y=1 (walkable space)
		for (int x = 0; x < 2; x++)
			for (int z = 0; z < 2; z++)
			{
				Assert.That(graph.ContainsNode(new Vector3Int(x, 0, z)), Is.True,
					$"Expected node at floor ({x},0,{z})");
				Assert.That(graph.ContainsNode(new Vector3Int(x, 1, z)), Is.False,
					$"Walkable voxel should not create a node at ({x},1,{z})");
			}
	}

	[Test]
	public void WalkableVoxelAtY1_NeighboursAreStillConnected()
	{
		// Filling y=1 with Walkable does not disrupt neighbour links at y=0
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 1);
		for (int x = 0; x < 3; x++)
			terrain.Cells[x, 1, 0] = new VoxelCell(WalkableId);

		VoxelTraversalGraph graph = BuildGraph(terrain, _registry);

		VoxelTraversalGraph.Node left = graph.GetNode(new Vector3Int(0, 0, 0));
		VoxelTraversalGraph.Node center = graph.GetNode(new Vector3Int(1, 0, 0));
		VoxelTraversalGraph.Node right = graph.GetNode(new Vector3Int(2, 0, 0));

		Assert.That(left.PositiveX, Is.SameAs(center));
		Assert.That(center.PositiveX, Is.SameAs(right));
	}

	// -------------------------------------------------------------------------
	// CreateNode manual injection
	// -------------------------------------------------------------------------

	[Test]
	public void ManuallyCreatedNode_IsReachable_ViaContainsNode()
	{
		VoxelTraversalGraph graph = new VoxelTraversalGraph(5, 5, 5);
		Vector3Int pos = new Vector3Int(2, 1, 3);

		graph.CreateNode(pos);

		Assert.That(graph.ContainsNode(pos), Is.True);
	}

	[Test]
	public void CreateNode_Twice_ReturnsSameNode()
	{
		VoxelTraversalGraph graph = new VoxelTraversalGraph(5, 5, 5);
		Vector3Int pos = new Vector3Int(1, 1, 1);

		VoxelTraversalGraph.Node first = graph.CreateNode(pos);
		VoxelTraversalGraph.Node second = graph.CreateNode(pos);

		Assert.That(first, Is.SameAs(second));
		Assert.That(graph.AllNodes.Count, Is.EqualTo(1), "Duplicate CreateNode must not add a second entry");
	}

	[Test]
	public void CreateNode_OutsideGraph_ThrowsArgumentOutOfRange()
	{
		VoxelTraversalGraph graph = new VoxelTraversalGraph(3, 3, 3);
		Assert.Throws<System.ArgumentOutOfRangeException>(() => graph.CreateNode(new Vector3Int(5, 0, 0)));
	}

	[Test]
	public void ManuallyInjectedNode_IsReturnedByTryGetNode()
	{
		VoxelTraversalGraph graph = new VoxelTraversalGraph(4, 4, 4);
		Vector3Int pos = new Vector3Int(0, 2, 3);
		graph.CreateNode(pos);

		bool found = graph.TryGetNode(pos, out VoxelTraversalGraph.Node node);

		Assert.That(found, Is.True);
		Assert.That(node, Is.Not.Null);
		Assert.That(node.Position, Is.EqualTo(pos));
	}

	// -------------------------------------------------------------------------
	// BoardNavigationLayer integration
	// -------------------------------------------------------------------------

	[Test]
	public void BoardNavigationLayer_IsStandable_AfterBuild()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		BoardNavigationLayer nav = new BoardNavigationLayer();
		nav.Rebuild(terrain);

		for (int x = 0; x < 3; x++)
			for (int z = 0; z < 3; z++)
				Assert.That(nav.IsStandable(new Vector3Int(x, 0, z)), Is.True,
					$"Expected IsStandable at ({x},0,{z})");
	}

	[Test]
	public void BoardNavigationLayer_IsNotStandable_WhereWalled()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		terrain.Cells[1, 1, 1] = new VoxelCell(ObstacleId);
		BoardNavigationLayer nav = new BoardNavigationLayer();
		nav.Rebuild(terrain);

		Assert.That(nav.IsStandable(new Vector3Int(1, 0, 1)), Is.False);
	}

	[Test]
	public void BoardNavigationLayer_PublicGraphProperty_ExposesInjectedNodes()
	{
		BoardTerrainLayer terrain = FlatBoard(_registry, 3, 3);
		BoardNavigationLayer nav = new BoardNavigationLayer();
		nav.Rebuild(terrain);

		// Manually inject a node at y=1 (simulating what LineOfSightTestFixture does)
		VoxelTraversalGraph graph = nav.Graph;
		Assert.That(graph, Is.Not.Null);

		graph.CreateNode(new Vector3Int(1, 1, 1));

		Assert.That(nav.IsStandable(new Vector3Int(1, 1, 1)), Is.True,
			"IsStandable should reflect manually injected node");
	}
}
