using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BoardRuntimeRegistry
{
	[NonSerialized]
	private BoardNavigationLayer _navigationLayer;

	private readonly Dictionary<BattleObject, Vector3Int> _positionsByObject = new Dictionary<BattleObject, Vector3Int>();
	private readonly Dictionary<Vector3Int, BattleUnit> _unitsByPosition = new Dictionary<Vector3Int, BattleUnit>();
	private readonly Dictionary<Vector3Int, List<BattleInteractiveObject>> _interactiveObjectsByPosition = new Dictionary<Vector3Int, List<BattleInteractiveObject>>();

	public void AttachNavigationLayer(BoardNavigationLayer p_navigationLayer)
	{
		_navigationLayer = p_navigationLayer;
	}

	public void Clear()
	{
		_positionsByObject.Clear();
		_unitsByPosition.Clear();
		_interactiveObjectsByPosition.Clear();
	}

	public bool TryGetPosition(BattleObject p_object, out Vector3Int p_position)
	{
		if (p_object == null)
		{
			p_position = default;
			return false;
		}

		return _positionsByObject.TryGetValue(p_object, out p_position);
	}

	public bool HasUnitAt(Vector3Int p_position)
	{
		return _unitsByPosition.TryGetValue(p_position, out BattleUnit unit) && unit != null;
	}

	public bool TryGetUnitAt(Vector3Int p_position, out BattleUnit p_unit)
	{
		return _unitsByPosition.TryGetValue(p_position, out p_unit) && p_unit != null;
	}

	public bool CanPlaceUnit(Vector3Int p_position, BattleUnit p_unit = null)
	{
		if (_navigationLayer == null || !_navigationLayer.IsStandable(p_position))
		{
			return false;
		}

		return !_unitsByPosition.TryGetValue(p_position, out BattleUnit occupyingUnit) ||
			   occupyingUnit == null ||
			   occupyingUnit == p_unit;
	}

	public bool TryPlaceUnit(BattleUnit p_unit, Vector3Int p_position)
	{
		if (p_unit == null || !CanPlaceUnit(p_position, p_unit))
		{
			return false;
		}

		RemoveObject(p_unit);

		_unitsByPosition[p_position] = p_unit;
		_positionsByObject[p_unit] = p_position;
		return true;
	}

	public bool TryAddInteractiveObject(BattleInteractiveObject p_interactiveObject, Vector3Int p_position)
	{
		if (p_interactiveObject == null || _navigationLayer == null || !_navigationLayer.IsStandable(p_position))
		{
			return false;
		}

		RemoveObject(p_interactiveObject);

		if (!_interactiveObjectsByPosition.TryGetValue(p_position, out List<BattleInteractiveObject> interactiveObjects) || interactiveObjects == null)
		{
			interactiveObjects = new List<BattleInteractiveObject>();
			_interactiveObjectsByPosition[p_position] = interactiveObjects;
		}

		interactiveObjects.Add(p_interactiveObject);
		_positionsByObject[p_interactiveObject] = p_position;
		return true;
	}

	public bool SwapUnits(BattleUnit p_first, BattleUnit p_second)
	{
		if (p_first == null || p_second == null)
		{
			return false;
		}

		if (!TryGetPosition(p_first, out Vector3Int firstPosition) ||
			!TryGetPosition(p_second, out Vector3Int secondPosition))
		{
			return false;
		}

		if (!_unitsByPosition.TryGetValue(firstPosition, out BattleUnit firstUnit) ||
			!_unitsByPosition.TryGetValue(secondPosition, out BattleUnit secondUnit) ||
			firstUnit != p_first ||
			secondUnit != p_second)
		{
			return false;
		}

		_unitsByPosition[firstPosition] = p_second;
		_unitsByPosition[secondPosition] = p_first;
		_positionsByObject[p_first] = secondPosition;
		_positionsByObject[p_second] = firstPosition;

		return true;
	}

	public void RemoveObject(BattleObject p_object)
	{
		if (p_object == null || !_positionsByObject.TryGetValue(p_object, out Vector3Int position))
		{
			_positionsByObject.Remove(p_object);
			return;
		}

		if (p_object is BattleUnit battleUnit)
		{
			if (_unitsByPosition.TryGetValue(position, out BattleUnit occupyingUnit) && occupyingUnit == battleUnit)
			{
				_unitsByPosition.Remove(position);
			}
		}
		else if (p_object is BattleInteractiveObject interactiveObject)
		{
			if (_interactiveObjectsByPosition.TryGetValue(position, out List<BattleInteractiveObject> interactiveObjects) && interactiveObjects != null)
			{
				interactiveObjects.Remove(interactiveObject);

				if (interactiveObjects.Count == 0)
				{
					_interactiveObjectsByPosition.Remove(position);
				}
			}
		}

		_positionsByObject.Remove(p_object);
	}

	public List<BattleInteractiveObject> RemoveInteractiveObjectsByTags(Vector3Int p_position, IReadOnlyCollection<string> p_tags)
	{
		List<BattleInteractiveObject> removedObjects = new List<BattleInteractiveObject>();

		if (p_tags == null || p_tags.Count == 0)
		{
			return removedObjects;
		}

		if (!_interactiveObjectsByPosition.TryGetValue(p_position, out List<BattleInteractiveObject> interactiveObjects) ||
			interactiveObjects == null ||
			interactiveObjects.Count == 0)
		{
			return removedObjects;
		}

		HashSet<string> tags = new HashSet<string>(p_tags);

		for (int index = interactiveObjects.Count - 1; index >= 0; index--)
		{
			BattleInteractiveObject interactiveObject = interactiveObjects[index];
			if (interactiveObject == null)
			{
				interactiveObjects.RemoveAt(index);
				continue;
			}

			if (!interactiveObject.HasAnyTag(tags))
			{
				continue;
			}

			interactiveObjects.RemoveAt(index);
			_positionsByObject.Remove(interactiveObject);
			removedObjects.Add(interactiveObject);
		}

		if (interactiveObjects.Count == 0)
		{
			_interactiveObjectsByPosition.Remove(p_position);
		}

		return removedObjects;
	}

	public List<BattleInteractiveObject> RemoveInteractiveObjectsByTags(IReadOnlyCollection<string> p_tags)
	{
		List<BattleInteractiveObject> removedObjects = new List<BattleInteractiveObject>();

		if (p_tags == null || p_tags.Count == 0)
		{
			return removedObjects;
		}

		List<Vector3Int> occupiedPositions = new List<Vector3Int>(_interactiveObjectsByPosition.Keys);
		for (int index = 0; index < occupiedPositions.Count; index++)
		{
			removedObjects.AddRange(RemoveInteractiveObjectsByTags(occupiedPositions[index], p_tags));
		}

		return removedObjects;
	}

	public IReadOnlyList<BattleInteractiveObject> GetInteractiveObjects(Vector3Int p_position)
	{
		if (!_interactiveObjectsByPosition.TryGetValue(p_position, out List<BattleInteractiveObject> interactiveObjects) || interactiveObjects == null)
		{
			return Array.Empty<BattleInteractiveObject>();
		}

		return interactiveObjects;
	}

	public bool CanRegister(BattleObject p_object, Vector3Int p_position, BoardNavigationLayer p_navigationLayer)
	{
		if (p_object == null || p_navigationLayer == null)
		{
			return false;
		}

		_navigationLayer = p_navigationLayer;

		if (!_navigationLayer.IsStandable(p_position))
		{
			return false;
		}

		if (p_object is BattleUnit battleUnit)
		{
			return CanPlaceUnit(p_position, battleUnit);
		}

		if (p_object is BattleInteractiveObject)
		{
			return true;
		}

		return false;
	}

	public bool TryRegister(BattleObject p_object, Vector3Int p_position, BoardNavigationLayer p_navigationLayer)
	{
		if (!CanRegister(p_object, p_position, p_navigationLayer))
		{
			return false;
		}

		if (p_object is BattleUnit battleUnit)
		{
			return TryPlaceUnit(battleUnit, p_position);
		}

		if (p_object is BattleInteractiveObject interactiveObject)
		{
			return TryAddInteractiveObject(interactiveObject, p_position);
		}

		return false;
	}

	public bool TryMove(BattleObject p_object, Vector3Int p_position, BoardNavigationLayer p_navigationLayer)
	{
		return TryRegister(p_object, p_position, p_navigationLayer);
	}

	public void Remove(BattleObject p_object)
	{
		RemoveObject(p_object);
	}

	public void AdvanceObjectDurations()
	{
		List<Vector3Int> positions = new List<Vector3Int>(_interactiveObjectsByPosition.Keys);
		for (int posIndex = 0; posIndex < positions.Count; posIndex++)
		{
			Vector3Int position = positions[posIndex];
			if (!_interactiveObjectsByPosition.TryGetValue(position, out List<BattleInteractiveObject> objects) || objects == null)
			{
				continue;
			}

			for (int objectIndex = objects.Count - 1; objectIndex >= 0; objectIndex--)
			{
				BattleInteractiveObject obj = objects[objectIndex];
				if (obj?.RemainingDuration == null || obj.RemainingDuration.Type != Duration.Kind.TurnBased)
				{
					continue;
				}

				obj.RemainingDuration.Turns--;
				if (obj.RemainingDuration.Turns <= 0)
				{
					objects.RemoveAt(objectIndex);
					_positionsByObject.Remove(obj);
				}
			}

			if (objects.Count == 0)
			{
				_interactiveObjectsByPosition.Remove(position);
			}
		}
	}

}