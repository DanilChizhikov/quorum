using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	[Preserve]
	public abstract class ScriptableQuorumAction : ScriptableObject, IQuorumAction
	{
		[SerializeReference]
		private Consideration _consideration = default;
		
		protected Consideration Consideration => _consideration;
		
		internal void SetConsideration(Consideration consideration)
		{
			_consideration = consideration;
		}
		
		public virtual void Initialize(IQuorumContext context) { }
		
		public abstract void Enter(IQuorumContext context);

		public abstract QuorumActionState Tick(IQuorumContext context);

		public virtual float CalculateUtility(IQuorumContext context) =>
			_consideration != null ? _consideration.Evaluate(context) : 0f;

		public abstract void Exit(IQuorumContext context);
	}
	
	[Preserve]
	public abstract class ScriptableQuorumAction<TAgent, TContext> : ScriptableQuorumAction
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
		protected abstract float CalculateUtility(TContext context);
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
					$"[{name}] expects context {typeof(TContext).Name} but got " +
					$"{(context == null ? "null" : context.GetType().Name)}: the action does nothing and scores 0.",
					this);
			}
#endif

			return false;
		}
	}
}
