using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Response curve over a float blackboard variable: the value is remapped from [min, max] to [0, 1]
	/// and fed through the curve. The bread and butter of utility AI - shape the curve, not the code.
	/// </summary>
	[Serializable]
	[UtilityMenu("Blackboard/Float Curve")]
	public sealed class BlackboardCurveConsideration : UtilityConsideration
	{
		[SerializeField]
		private string _key = string.Empty;

		[SerializeField]
		private float _min = 0f;

		[SerializeField]
		private float _max = 1f;

		[SerializeField]
		private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public BlackboardCurveConsideration() { }

		public BlackboardCurveConsideration(string key, float min, float max, AnimationCurve curve)
		{
			_key = key;
			_min = min;
			_max = max;
			_curve = curve;
		}

		public override float Evaluate(IUtilityContext context)
		{
			RuntimeBlackboard blackboard = context.Blackboard;
			if (blackboard == null || _curve == null || !blackboard.TryGetVariable(_key, out BlackboardVariable<float> variable))
			{
				return 0f;
			}

			float normalized = Mathf.InverseLerp(_min, _max, variable.Value);
			return Mathf.Clamp01(_curve.Evaluate(normalized));
		}
	}
}
