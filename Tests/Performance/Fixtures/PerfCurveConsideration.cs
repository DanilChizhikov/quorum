using System;
using UnityEngine;

namespace DTech.UtilityAI.Tests.Performance
{
	[Serializable]
	internal sealed class PerfCurveConsideration : UtilityConsideration
	{
		[SerializeField]
		private float _value = 0.5f;

		[SerializeField]
		private float _min;

		[SerializeField]
		private float _max = 1f;

		[SerializeField]
		private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public PerfCurveConsideration() { }

		public PerfCurveConsideration(float value)
		{
			_value = value;
		}

		public override float Evaluate(IUtilityContext context)
		{
			if (_curve == null)
			{
				return 0f;
			}

			float normalized = Mathf.InverseLerp(_min, _max, _value);
			return Mathf.Clamp01(_curve.Evaluate(normalized));
		}
	}
}
