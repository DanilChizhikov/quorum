using System;
using UnityEngine;

namespace DTech.UtilityAI
{
	[Serializable]
	public abstract class UtilityAction : IUtilityAction
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

	public abstract class UtilityAction<TAgent, TContext> : UtilityAction
		where TAgent : IUtilityAgent
		where TContext : UtilityContext<TAgent>
	{
		public override void Initialize(IUtilityContext context)
		{
			if (context is not TContext genericContext)
			{
				return;
			}

			Initialize(genericContext);
		}

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

		protected virtual void Initialize(TContext context) { }

		protected abstract void Enter(TContext context);

		protected abstract UtilityActionState OnTick(TContext context);

		protected virtual float CalculateUtility(TContext context) =>
			Consideration != null ? Consideration.Evaluate(context) : 0f;

		protected abstract void Exit(TContext context);
	}
}
