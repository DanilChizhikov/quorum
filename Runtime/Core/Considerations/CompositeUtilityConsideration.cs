using System;
using UnityEngine;

namespace DTech.UtilityAI
{
	[Serializable]
	[UtilityMenu("Composite")]
	public sealed class CompositeUtilityConsideration : UtilityConsideration
	{
		[SerializeField]
		private ConsiderationCombineMode _combineMode = ConsiderationCombineMode.Multiply;

		[SerializeReference]
		private UtilityConsideration[] _considerations = Array.Empty<UtilityConsideration>();

		public CompositeUtilityConsideration() { }
		
		public CompositeUtilityConsideration(ConsiderationCombineMode combineMode, params UtilityConsideration[] considerations)
		{
			_combineMode = combineMode;
			_considerations = considerations ?? Array.Empty<UtilityConsideration>();
		}
		
		public override float Evaluate(IUtilityContext context)
		{
			if (_considerations == null)
			{
				return 0f;
			}

			int count = 0;
			float result = 0f;
			for (int i = 0; i < _considerations.Length; i++)
			{
				UtilityConsideration consideration = _considerations[i];
				if (consideration == null)
				{
					continue;
				}

				float score = consideration.Evaluate(context);
				result = count <= 0 ? score : Combine(result, score);
				count++;
			}

			if (count <= 0)
			{
				return 0f;
			}

			return _combineMode == ConsiderationCombineMode.Average ? result / count : result;
		}

		private float Combine(float result, float score) =>
			_combineMode switch
			{
				ConsiderationCombineMode.Multiply => result * score,
				ConsiderationCombineMode.Min => Mathf.Min(result, score),
				ConsiderationCombineMode.Max => Mathf.Max(result, score),
				ConsiderationCombineMode.Average => result + score,
				ConsiderationCombineMode.Sum => result + score,
				_ => throw new ArgumentOutOfRangeException(nameof(_combineMode), _combineMode, null)
			};
	}
}
