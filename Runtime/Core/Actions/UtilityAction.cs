using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.UtilityAI
{
	[Serializable]
	[Preserve]
	public abstract class UtilityAction : IUtilityAction
	{
		[SerializeReference]
		private UtilityConsideration _consideration = default;

		protected UtilityConsideration Consideration => _consideration;

		internal void SetConsideration(UtilityConsideration consideration)
		{
			_consideration = consideration;
		}

		internal void InitializeConsideration(IUtilityContext context)
		{
			_consideration?.Initialize(context);
		}

		public virtual void Initialize(IUtilityContext context) { }

		/// <summary>
		/// Creates the per-brain copy of this action. The default is a shallow field copy plus a
		/// copy of the consideration tree, which is what config data needs: value fields are
		/// duplicated and references to shared assets stay shared. Override it when a reference
		/// field holds per-agent state that two brains must not share.
		/// </summary>
		public virtual UtilityAction Clone()
		{
			var clone = (UtilityAction)MemberwiseClone();
			clone._consideration = _consideration?.Clone();
			return clone;
		}

		public abstract void Enter(IUtilityContext context);

		public abstract UtilityActionState Tick(IUtilityContext context);

		public virtual float CalculateUtility(IUtilityContext context) =>
			_consideration != null ? _consideration.Evaluate(context) : 0f;

		public abstract void Exit(IUtilityContext context);
	}

	[Preserve]
	public abstract class UtilityAction<TAgent, TContext> : UtilityAction
		where TAgent : IUtilityAgent
		where TContext : UtilityContext<TAgent>
	{
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
		private bool _loggedContextMismatch;
#endif

		public override void Initialize(IUtilityContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return;
			}

			Initialize(genericContext);
		}

		public override void Enter(IUtilityContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return;
			}

			Enter(genericContext);
		}

		public override UtilityActionState Tick(IUtilityContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return UtilityActionState.Failed;
			}

			return OnTick(genericContext);
		}

		public override float CalculateUtility(IUtilityContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return 0f;
			}

			return CalculateUtility(genericContext);
		}

		public override void Exit(IUtilityContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return;
			}

			Exit(genericContext);
		}

		protected virtual void Initialize(TContext context) { }

		protected abstract void Enter(TContext context);

		protected abstract UtilityActionState OnTick(TContext context);

		protected virtual float CalculateUtility(TContext context) =>
			Consideration != null ? Consideration.Evaluate(context) : 0f;

		protected abstract void Exit(TContext context);

		private bool TryGetContext(IUtilityContext context, out TContext genericContext)
		{
			if (context is TContext typedContext)
			{
				genericContext = typedContext;
				return true;
			}

			genericContext = null;
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			if (!_loggedContextMismatch)
			{
				_loggedContextMismatch = true;
				Debug.LogError(
					$"[{GetType().Name}] expects context {typeof(TContext).Name} but got " +
					$"{(context == null ? "null" : context.GetType().Name)}: the action does nothing and scores 0.");
			}
#endif

			return false;
		}
	}
}
