using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Berry bush. Disappears when eaten; VillageWorld grows new ones over time.
	/// </summary>
	public sealed class FoodSource : MonoBehaviour
	{
		private VillageWorld _world;

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
