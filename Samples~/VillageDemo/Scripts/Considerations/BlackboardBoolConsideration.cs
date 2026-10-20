using System;
using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Gate on a bool blackboard variable. Multiplied inside a composite it switches an action off.
	/// </summary>
	[Serializable]
	[UtilityMenu("Blackboard/Bool Gate")]
	public sealed class BlackboardBoolConsideration : UtilityConsideration
	{
		[SerializeField]
		private string _key = string.Empty;

		[SerializeField]
		private float _whenTrue = 1f;

		[SerializeField]
		private float _whenFalse = 0f;

		public BlackboardBoolConsideration() { }

		public BlackboardBoolConsideration(string key, float whenTrue = 1f, float whenFalse = 0f)
		{
			_key = key;
			_whenTrue = whenTrue;
			_whenFalse = whenFalse;
		}

		public override float Evaluate(IUtilityContext context)
		{
			RuntimeBlackboard blackboard = context.Blackboard;
			if (blackboard == null || !blackboard.TryGetVariable(_key, out BlackboardVariable<bool> variable))
			{
				return _whenFalse;
			}

			return variable.Value ? _whenTrue : _whenFalse;
		}
	}
}
