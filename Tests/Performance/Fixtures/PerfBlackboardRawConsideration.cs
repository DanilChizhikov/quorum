using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum.Tests.Performance
{
	[Serializable]
	internal sealed class PerfBlackboardRawConsideration : Consideration
	{
		[SerializeField]
		private string _key = string.Empty;

		public PerfBlackboardRawConsideration() { }

		public PerfBlackboardRawConsideration(string key)
		{
			_key = key;
		}

		public override float Evaluate(IQuorumContext context)
		{
			RuntimeBlackboard blackboard = context.Blackboard;
			if (blackboard == null || !blackboard.TryGetVariable(_key, out BlackboardVariable<float> variable))
			{
				return 0f;
			}

			return variable.Value;
		}
	}
}
