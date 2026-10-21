using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Fixed score. Used for fallback actions (Wander, Idle, Patrol) and as a weight inside a composite.
	/// </summary>
	[Serializable]
	[QuorumMenu("Constant")]
	public sealed class ConstantConsideration : Consideration
	{
		[SerializeField]
		private float _value = 0.1f;

		public ConstantConsideration() { }

		public ConstantConsideration(float value)
		{
			_value = value;
		}

		public override float Evaluate(IQuorumContext context) => _value;
	}
}
