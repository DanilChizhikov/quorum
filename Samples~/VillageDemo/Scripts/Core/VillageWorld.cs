using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Scene-level service: knows every agent and place of interest, grows food and counts wood.
	/// Actions never look it up themselves - they receive it from <see cref="VillageActionInjector"/>,
	/// the same way a Zenject or VContainer injector would hand them a service.
	/// Everything that belongs to the world is parented under it and finds it with GetComponentInParent.
	/// </summary>
	public sealed class VillageWorld : MonoBehaviour
	{
		[SerializeField]
		private Vector2 _halfExtents = new(14f, 9f);

		[SerializeField]
		private FoodSource _foodPrefab = default;

		[SerializeField]
		private float _foodGrowInterval = 3f;

		[SerializeField]
		private int _maxFood = 5;

		[SerializeField]
		private VillagerAgent _villagerPrefab = default;

		[SerializeField]
		private WolfAgent _wolfPrefab = default;

		private readonly List<VillagerAgent> _villagers = new();
		private readonly List<WolfAgent> _wolves = new();
		private readonly List<FoodSource> _foods = new();
		private readonly List<VillagePoint> _beds = new();
		private readonly List<VillagePoint> _trees = new();
		private readonly List<VillagePoint> _stockpiles = new();
		private readonly List<VillagePoint> _foodSpawns = new();

		private float _foodGrowTimer;

		public IReadOnlyList<VillagerAgent> Villagers => _villagers;
		public IReadOnlyList<WolfAgent> Wolves => _wolves;
		public int FoodCount => _foods.Count;
		public int Wood { get; private set; }

		public void Register(VillagerAgent villager) => AddUnique(_villagers, villager);
		public void Unregister(VillagerAgent villager) => _villagers.Remove(villager);
		public void Register(WolfAgent wolf) => AddUnique(_wolves, wolf);
		public void Unregister(WolfAgent wolf) => _wolves.Remove(wolf);
		public void Register(FoodSource food) => AddUnique(_foods, food);
		public void Unregister(FoodSource food) => _foods.Remove(food);
		public void Register(VillagePoint point) => AddUnique(GetPoints(point.Kind), point);
		public void Unregister(VillagePoint point) => GetPoints(point.Kind).Remove(point);

		public bool TryGetNearestFood(Vector3 position, out FoodSource food) => TryGetNearest(_foods, position, out food);
		public bool TryGetNearestVillager(Vector3 position, out VillagerAgent villager) => TryGetNearest(_villagers, position, out villager);
		public bool TryGetNearestWolf(Vector3 position, out WolfAgent wolf) => TryGetNearest(_wolves, position, out wolf);
		public bool TryGetNearestPoint(VillagePointKind kind, Vector3 position, out VillagePoint point) =>
			TryGetNearest(GetPoints(kind), position, out point);

		/// <summary>
		/// Removes the food if it still exists. Two villagers can walk to the same bush: the second one
		/// gets false here and its Eat action fails, which makes the brain pick again.
		/// </summary>
		public bool TryConsume(FoodSource food)
		{
			if (food == null || !_foods.Remove(food))
			{
				return false;
			}

			Destroy(food.gameObject);
			return true;
		}

		public void AddWood(int amount)
		{
			Wood += Mathf.Max(0, amount);
		}

		public Vector3 ClampToBounds(Vector3 position)
		{
			Vector3 center = transform.position;
			position.x = Mathf.Clamp(position.x, center.x - _halfExtents.x, center.x + _halfExtents.x);
			position.z = Mathf.Clamp(position.z, center.z - _halfExtents.y, center.z + _halfExtents.y);
			return position;
		}

		public Vector3 GetRandomPoint()
		{
			Vector3 center = transform.position;
			return new Vector3(
				center.x + Random.Range(-_halfExtents.x, _halfExtents.x),
				center.y,
				center.z + Random.Range(-_halfExtents.y, _halfExtents.y));
		}

		public void SpawnFood()
		{
			if (_foodPrefab == null)
			{
				return;
			}

			Vector3 position = GetRandomPoint();
			if (_foodSpawns.Count > 0)
			{
				Vector2 offset = Random.insideUnitCircle * 1.5f;
				position = _foodSpawns[Random.Range(0, _foodSpawns.Count)].Position + new Vector3(offset.x, 0f, offset.y);
			}

			position.y = transform.position.y;
			Instantiate(_foodPrefab, ClampToBounds(position), Quaternion.identity, transform);
		}

		public void SpawnVillager()
		{
			if (_villagerPrefab != null)
			{
				Instantiate(_villagerPrefab, GetRandomPoint(), Quaternion.identity, transform);
			}
		}

		public void SpawnWolf()
		{
			if (_wolfPrefab != null)
			{
				Instantiate(_wolfPrefab, GetRandomPoint(), Quaternion.identity, transform);
			}
		}

		public void RemoveWolf()
		{
			if (_wolves.Count > 0)
			{
				Destroy(_wolves[_wolves.Count - 1].gameObject);
			}
		}

		private void Update()
		{
			if (_foods.Count >= _maxFood)
			{
				_foodGrowTimer = 0f;
				return;
			}

			_foodGrowTimer += Time.deltaTime;
			if (_foodGrowTimer >= _foodGrowInterval)
			{
				_foodGrowTimer = 0f;
				SpawnFood();
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, new Vector3(_halfExtents.x * 2f, 0.1f, _halfExtents.y * 2f));
		}

		private List<VillagePoint> GetPoints(VillagePointKind kind) =>
			kind switch
			{
				VillagePointKind.Bed => _beds,
				VillagePointKind.Tree => _trees,
				VillagePointKind.Stockpile => _stockpiles,
				_ => _foodSpawns,
			};

		private static void AddUnique<T>(List<T> list, T item)
			where T : Component
		{
			if (item != null && !list.Contains(item))
			{
				list.Add(item);
			}
		}

		private static bool TryGetNearest<T>(List<T> items, Vector3 position, out T nearest)
			where T : Component
		{
			nearest = null;
			float bestDistanceSqr = float.PositiveInfinity;
			for (int i = 0; i < items.Count; i++)
			{
				T item = items[i];
				if (item == null)
				{
					continue;
				}

				Vector3 delta = item.transform.position - position;
				delta.y = 0f;
				float distanceSqr = delta.sqrMagnitude;
				if (distanceSqr < bestDistanceSqr)
				{
					bestDistanceSqr = distanceSqr;
					nearest = item;
				}
			}

			return nearest != null;
		}
	}
}
