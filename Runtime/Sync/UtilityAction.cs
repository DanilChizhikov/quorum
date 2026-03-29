using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class UtilityAction<TAgent, TContext> : ScriptableObject, IUtilityAction
		where TAgent : IUtilityAgent
		where TContext : UtilityContext<TAgent>
	{
		public void Initialize(IUtilityContext context)
		{
			
		}

		public void Enter(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return;
			}
			
			Enter(genericContext);
		}

		public UtilityActionState Tick(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return UtilityActionState.Failed;
			}
			
			return OnTick(genericContext);
		}

		public float CalculateUtility(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return 0f;
			}
		}

		public void Exit(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return;
			}
			
			Exit(genericContext);
		}

		public void Cancel(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return;
			}
			
			Cancel(genericContext);
		}
		
		protected abstract void Enter(TContext context);
		protected abstract UtilityActionState OnTick(TContext context);
		protected abstract float CalculateUtility(TContext context);
		protected abstract void Exit(TContext context);
		protected abstract void Cancel(TContext context);
	}
}