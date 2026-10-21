using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	public enum VillagePointKind : byte
	{
		Bed = 0,
		Tree = 1,
		Stockpile = 2,
		FoodSpawn = 3,
	}

	/// <summary>
	/// Static place of interest. Registers itself in the VillageWorld it is parented under.
	/// </summary>
	public sealed class VillagePoint : MonoBehaviour
	{
		[SerializeField]
		private VillagePointKind _kind = VillagePointKind.Bed;

		private VillageWorld _world;

		public VillagePointKind Kind => _kind;
		public Vector3 Position => transform.position;

		private void OnEnable()
		{
			_world = GetComponentInParent<VillageWorld>();
			if (_world != null)
			{
				_world.Register(this);
			}
		}

		private void OnDisable()
		{
			if (_world != null)
			{
				_world.Unregister(this);
			}
		}
	}
}
