using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	/// <summary>
	/// Config entry that routes a brain to an existing <see cref="ScriptableQuorumAction"/> asset.
	/// The referenced asset is shared by every brain and is never injected, because runtime
	/// dependencies must not be written into an asset.
	/// </summary>
	[Serializable]
	[QuorumMenu("Scriptable Action Reference", "ScriptableObject Icon")]
	[Preserve]
	public sealed class ScriptableQuorumActionReference : QuorumAction
	{
		[SerializeField]
		private ScriptableQuorumAction _action = default;

		public ScriptableQuorumAction Action => _action;

		public ScriptableQuorumActionReference() { }

		public ScriptableQuorumActionReference(ScriptableQuorumAction action)
		{
			_action = action;
		}

		public override void Initialize(IQuorumContext context)
		{
			if (_action == null)
			{
				return;
			}

			_action.Initialize(context);
		}

		public override void Enter(IQuorumContext context)
		{
			if (_action == null)
			{
				return;
			}

			_action.Enter(context);
		}

		public override QuorumActionState Tick(IQuorumContext context) =>
			_action != null ? _action.Tick(context) : QuorumActionState.Failed;

		public override float CalculateUtility(IQuorumContext context) =>
			_action != null ? _action.CalculateUtility(context) : 0f;

		public override void Exit(IQuorumContext context)
		{
			if (_action == null)
			{
				return;
			}

			_action.Exit(context);
		}
	}
}
