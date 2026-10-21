using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Gate on a bool blackboard variable. Multiplied inside a composite it switches an action off.
	/// </summary>
	[Serializable]
	[QuorumMenu("Blackboard/Bool Gate")]
	public sealed class BlackboardBoolConsideration : Consideration
	{
		[SerializeField]
		private string _key = string.Empty;

		[SerializeField]
		private float _whenTrue = 1f;

		[SerializeField]
		private float _whenFalse = 0f;

		private BlackboardVariable<bool> _variable;

		public BlackboardBoolConsideration() { }

		public BlackboardBoolConsideration(string key, float whenTrue = 1f, float whenFalse = 0f)
		{
			_key = key;
			_whenTrue = whenTrue;
			_whenFalse = whenFalse;
		}

		public override void Initialize(IQuorumContext context)
		{
			RuntimeBlackboard blackboard = context.Blackboard;
			if (blackboard == null || !blackboard.TryGetVariable(_key, out BlackboardVariable<bool> variable))
			{
				_variable = null;
				return;
			}

			_variable = variable;
		}

		public override float Evaluate(IQuorumContext context)
		{
			if (_variable == null)
			{
				return _whenFalse;
			}

			return _variable.Value ? _whenTrue : _whenFalse;
		}
	}
}
