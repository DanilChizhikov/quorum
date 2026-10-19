using System;
using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[Serializable]
	internal sealed class TestUtilityConsideration : UtilityConsideration
	{
		[SerializeField]
		private float _score = 0f;
		
		public float Score => _score;
		
		public TestUtilityConsideration() { }
		
		public TestUtilityConsideration(float score)
		{
			_score = score;
		}
		
		public void SetScore(float score)
		{
			_score = score;
		}
		
		public override float Evaluate(IUtilityContext context) => _score;
	}
}
