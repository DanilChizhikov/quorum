using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Tests.Performance
{
	[Serializable]
	internal sealed class PerfBlackboardCachedConsideration : UtilityConsideration
	{
		[SerializeField]
		private string _key = string.Empty;

		[SerializeField]
		private float _min;

		[SerializeField]
		private float _max = 1f;

		[SerializeField]
		private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		private BlackboardVariable<float> _variable;

		public PerfBlackboardCachedConsideration() { }

		public PerfBlackboardCachedConsideration(string key)
		{
			_key = key;
		}

		public override void Initialize(IUtilityContext context)
		{
			RuntimeBlackboard blackboard = context.Blackboard;
			if (blackboard == null || !blackboard.TryGetVariable(_key, out BlackboardVariable<float> variable))
			{
				_variable = null;
				return;
			}

			_variable = variable;
		}

		public override float Evaluate(IUtilityContext context)
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
