using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	[Serializable]
	[QuorumMenu("Composite")]
	[Preserve]
	public sealed class CompositeConsideration : Consideration
	{
		[SerializeField]
		private ConsiderationCombineMode _combineMode = ConsiderationCombineMode.Multiply;

		[SerializeReference]
		private Consideration[] _considerations = Array.Empty<Consideration>();

		public CompositeConsideration() { }
		
		public CompositeConsideration(ConsiderationCombineMode combineMode, params Consideration[] considerations)
		{
			_combineMode = combineMode;
			_considerations = considerations ?? Array.Empty<Consideration>();
		}
		
		public override void Initialize(IQuorumContext context)
		{
			if (_considerations == null)
			{
				return;
			}

			for (int i = 0; i < _considerations.Length; i++)
			{
				_considerations[i]?.Initialize(context);
			}
		}

		public override Consideration Clone()
		{
			var clone = (CompositeConsideration)base.Clone();
			if (_considerations == null)
			{
				return clone;
			}

			var children = new Consideration[_considerations.Length];
			for (int i = 0; i < children.Length; i++)
			{
				children[i] = _considerations[i]?.Clone();
			}

			clone._considerations = children;
			return clone;
		}

		public override float Evaluate(IQuorumContext context)
		{
			if (_considerations == null)
			{
				return 0f;
			}

			int count = 0;
			float result = 0f;
			for (int i = 0; i < _considerations.Length; i++)
			{
				Consideration consideration = _considerations[i];
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
				_ => throw new ArgumentOutOfRangeException(nameof(_combineMode), _combineMode, "Unknown consideration combine mode.")
			};
	}
}
