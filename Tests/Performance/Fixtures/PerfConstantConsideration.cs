using System;
using UnityEngine;

namespace DTech.Quorum.Tests.Performance
{
	[Serializable]
	internal sealed class PerfConstantConsideration : Consideration
	{
		[SerializeField]
		private float _score;

		public PerfConstantConsideration() { }

		public PerfConstantConsideration(float score)
		{
			_score = score;
		}

		public override float Evaluate(IQuorumContext context) => _score;
	}
}
