using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class UtilityAction : ScriptableObject, IUtilityAction
	{
		[field: SerializeField]
		protected UtilityConsideration Consideration { get; private set; }
		
		public abstract void Enter(IUtilityContext context);

		public abstract UtilityActionState Tick(IUtilityContext context);

		public virtual float CalculateUtility(IUtilityContext context) => Consideration.Evaluate(context);

		public abstract void Exit(IUtilityContext context);
	}
	
	public abstract class UtilityAction<TAgent, TContext> : UtilityAction
		where TAgent : IUtilityAgent
		where TContext : UtilityContext<TAgent>
	{
		public override void Enter(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return;
			}
			
			Enter(genericContext);
		}

		public override UtilityActionState Tick(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return UtilityActionState.Failed;
			}
			
			return OnTick(genericContext);
		}

		public override float CalculateUtility(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return 0f;
			}
			
			return CalculateUtility(genericContext);
		}

		public override void Exit(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return;
			}
			
			Exit(genericContext);
		}
		
		protected abstract void Enter(TContext context);
		protected abstract UtilityActionState OnTick(TContext context);
		protected abstract float CalculateUtility(TContext context);
		protected abstract void Exit(TContext context);
	}
}