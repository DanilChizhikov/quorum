using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Response curve over a float blackboard variable: the value is remapped from [min, max] to [0, 1]
	/// and fed through the curve. The bread and butter of utility AI - shape the curve, not the code.
	/// The variable is resolved once in Initialize and then read straight from the cached reference:
	/// a lookup by name costs far more than the whole rest of the score, so never do it in Evaluate.
	/// </summary>
	[Serializable]
	[QuorumMenu("Blackboard/Float Curve")]
	public sealed class BlackboardCurveConsideration : Consideration
	{
		[SerializeField]
		private string _key = string.Empty;

		[SerializeField]
		private float _min = 0f;

		[SerializeField]
		private float _max = 1f;

		[SerializeField]
		private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		private BlackboardVariable<float> _variable;

		public BlackboardCurveConsideration() { }

		public BlackboardCurveConsideration(string key, float min, float max, AnimationCurve curve)
		{
			_key = key;
			_min = min;
			_max = max;
			_curve = curve;
		}

		public override void Initialize(IQuorumContext context)
		{
			RuntimeBlackboard blackboard = context.Blackboard;
			if (blackboard == null || !blackboard.TryGetVariable(_key, out BlackboardVariable<float> variable))
			{
				_variable = null;
				return;
			}

			_variable = variable;
		}

		public override float Evaluate(IQuorumContext context)
		{
			if (_variable == null || _curve == null)
			{
				return 0f;
			}

			float normalized = Mathf.InverseLerp(_min, _max, _variable.Value);
			return Mathf.Clamp01(_curve.Evaluate(normalized));
		}
	}
}
