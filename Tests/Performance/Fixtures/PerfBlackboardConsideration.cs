using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Tests.Performance
{
	[Serializable]
	internal sealed class PerfBlackboardConsideration : UtilityConsideration
	{
		[SerializeField]
		private string _key = string.Empty;

		[SerializeField]
		private float _min;

		[SerializeField]
		private float _max = 1f;

		[SerializeField]
		private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public PerfBlackboardConsideration() { }

		public PerfBlackboardConsideration(string key)
		{
			_key = key;
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
