using System;
using UnityEngine;

namespace DTech.UtilityAI.Tests.Performance
{
	[Serializable]
	internal sealed class PerfConstantConsideration : UtilityConsideration
	{
		[SerializeField]
		private float _score;

		public PerfConstantConsideration() { }

		public PerfConstantConsideration(float score)
		{
			_score = score;
		}

		public override float Evaluate(IUtilityContext context) => _score;
	}
}
