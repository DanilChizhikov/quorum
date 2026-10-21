using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.UtilityAI
{
	[Preserve]
	public abstract class ScriptableUtilityAction : ScriptableObject, IUtilityAction
	{
		[SerializeReference]
		private UtilityConsideration _consideration = default;
		
		protected UtilityConsideration Consideration => _consideration;
		
		internal void SetConsideration(UtilityConsideration consideration)
		{
			_consideration = consideration;
		}
		
		public virtual void Initialize(IUtilityContext context) { }
		
		public abstract void Enter(IUtilityContext context);

		public abstract UtilityActionState Tick(IUtilityContext context);

		public virtual float CalculateUtility(IUtilityContext context) =>
			_consideration != null ? _consideration.Evaluate(context) : 0f;

		public abstract void Exit(IUtilityContext context);
	}
	
	[Preserve]
	public abstract class ScriptableUtilityAction<TAgent, TContext> : ScriptableUtilityAction
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
		protected abstract float CalculateUtility(TContext context);
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
					$"[{name}] expects context {typeof(TContext).Name} but got " +
					$"{(context == null ? "null" : context.GetType().Name)}: the action does nothing and scores 0.",
					this);
			}
#endif

			return false;
		}
	}
}
