using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.UtilityAI
{
	/// <summary>
	/// Config entry that routes a brain to an existing <see cref="ScriptableUtilityAction"/> asset.
	/// The referenced asset is shared by every brain and is never injected, because runtime
	/// dependencies must not be written into an asset.
	/// </summary>
	[Serializable]
	[UtilityMenu("Scriptable Action Reference", "ScriptableObject Icon")]
	[Preserve]
	public sealed class ScriptableUtilityActionReference : UtilityAction
	{
		[SerializeField]
		private ScriptableUtilityAction _action = default;

		public ScriptableUtilityAction Action => _action;

		public ScriptableUtilityActionReference() { }

		public ScriptableUtilityActionReference(ScriptableUtilityAction action)
		{
			_action = action;
		}

		public override void Initialize(IUtilityContext context)
		{
			if (_action == null)
			{
				return;
			}

			_action.Initialize(context);
		}

		public override void Enter(IUtilityContext context)
		{
			if (_action == null)
			{
				return;
			}

			_action.Enter(context);
		}

		public override UtilityActionState Tick(IUtilityContext context) =>
			_action != null ? _action.Tick(context) : UtilityActionState.Failed;

		public override float CalculateUtility(IUtilityContext context) =>
			_action != null ? _action.CalculateUtility(context) : 0f;

		public override void Exit(IUtilityContext context)
		{
			if (_action == null)
			{
				return;
			}

			_action.Exit(context);
		}
	}
}
