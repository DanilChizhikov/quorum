using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Straight-line movement on the XZ plane. The sample has no NavMesh on purpose:
	/// the interesting part is which action runs, not how the agent walks.
	/// </summary>
	public sealed class VillageMover : MonoBehaviour
	{
		[SerializeField]
		private float _turnSpeed = 720f;

		/// <summary>
		/// Moves one frame towards the target.
		/// </summary>
		/// <param name="target">World position, its Y is ignored.</param>
		/// <param name="speed">Speed in metres per second.</param>
		/// <param name="stopDistance">Distance at which the target counts as reached.</param>
		/// <returns>True when the target is reached.</returns>
		public bool MoveTo(Vector3 target, float speed, float stopDistance = 0.2f)
		{
			Vector3 position = transform.position;
			target.y = position.y;
			Vector3 delta = target - position;
			float stopDistanceSqr = stopDistance * stopDistance;
			if (delta.sqrMagnitude <= stopDistanceSqr)
			{
				return true;
			}

			Vector3 next = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
			transform.position = next;
			Face(delta);
			return (target - next).sqrMagnitude <= stopDistanceSqr;
		}

		public void Face(Vector3 direction)
		{
			direction.y = 0f;
			if (direction.sqrMagnitude < 0.0001f)
			{
				return;
			}

			Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _turnSpeed * Time.deltaTime);
		}

		public void Teleport(Vector3 position)
		{
			position.y = transform.position.y;
			transform.position = position;
		}
	}
}
