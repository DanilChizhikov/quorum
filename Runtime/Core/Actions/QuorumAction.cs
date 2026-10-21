using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	[Serializable]
	[Preserve]
	public abstract class QuorumAction : IQuorumAction
	{
		[SerializeReference]
		private Consideration _consideration = default;

		protected Consideration Consideration => _consideration;

		internal void SetConsideration(Consideration consideration)
		{
			_consideration = consideration;
		}

		internal void InitializeConsideration(IQuorumContext context)
		{
			_consideration?.Initialize(context);
		}

		public virtual void Initialize(IQuorumContext context) { }

		/// <summary>
		/// Creates the per-brain copy of this action. The default is a shallow field copy plus a
		/// copy of the consideration tree, which is what config data needs: value fields are
		/// duplicated and references to shared assets stay shared. Override it when a reference
		/// field holds per-agent state that two brains must not share.
		/// </summary>
		public virtual QuorumAction Clone()
		{
			var clone = (QuorumAction)MemberwiseClone();
			clone._consideration = _consideration?.Clone();
			return clone;
		}

		public abstract void Enter(IQuorumContext context);

		public abstract QuorumActionState Tick(IQuorumContext context);

		public virtual float CalculateUtility(IQuorumContext context) =>
			_consideration != null ? _consideration.Evaluate(context) : 0f;

		public abstract void Exit(IQuorumContext context);
	}

	[Preserve]
	public abstract class QuorumAction<TAgent, TContext> : QuorumAction
		where TAgent : IQuorumAgent
		where TContext : QuorumContext<TAgent>
	{
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
		private bool _loggedContextMismatch;
#endif

		public override void Initialize(IQuorumContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return;
			}

			Initialize(genericContext);
		}

		public override void Enter(IQuorumContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return;
			}

			Enter(genericContext);
		}

		public override QuorumActionState Tick(IQuorumContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return QuorumActionState.Failed;
			}

			return OnTick(genericContext);
		}

		public override float CalculateUtility(IQuorumContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return 0f;
			}

			return CalculateUtility(genericContext);
		}

		public override void Exit(IQuorumContext context)
		{
			if (!TryGetContext(context, out TContext genericContext))
			{
				return;
			}

			Exit(genericContext);
		}

		protected virtual void Initialize(TContext context) { }

		protected abstract void Enter(TContext context);

		protected abstract QuorumActionState OnTick(TContext context);

		protected virtual float CalculateUtility(TContext context) =>
			Consideration != null ? Consideration.Evaluate(context) : 0f;

		protected abstract void Exit(TContext context);

		private bool TryGetContext(IQuorumContext context, out TContext genericContext)
		{
			if (context is TContext typedContext)
			{
				genericContext = typedContext;
				return true;
			}

			genericContext = null;
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
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
