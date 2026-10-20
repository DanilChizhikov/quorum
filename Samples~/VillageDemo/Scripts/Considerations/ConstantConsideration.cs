using System;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Fixed score. Used for fallback actions (Wander, Idle, Patrol) and as a weight inside a composite.
	/// </summary>
	[Serializable]
	[UtilityMenu("Constant")]
	public sealed class ConstantConsideration : UtilityConsideration
	{
		[SerializeField]
		private float _value = 0.1f;

		public ConstantConsideration() { }

		public ConstantConsideration(float value)
		{
			_value = value;
		}

		public override float Evaluate(IUtilityContext context) => _value;
	}
}
