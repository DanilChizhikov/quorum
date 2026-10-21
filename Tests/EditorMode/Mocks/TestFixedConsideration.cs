using System;
using UnityEngine;

namespace DTech.Quorum.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestFixedConsideration : Consideration
	{
		[SerializeField]
		private float _score = 0f;
		
		public float Score => _score;
		
		public TestFixedConsideration() { }
		
		public TestFixedConsideration(float score)
		{
			_score = score;
		}
		
		public void SetScore(float score)
		{
			_score = score;
		}
		
		public override float Evaluate(IQuorumContext context) => _score;
	}
}
