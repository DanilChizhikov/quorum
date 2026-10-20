namespace DTech.UtilityAI
{
	public interface IUtilityAction
	{
		void Initialize(IUtilityContext context);
		void Enter(IUtilityContext context);
		UtilityActionState Tick(IUtilityContext context);
		/// <summary>
		/// Returns the score of this action for the given context.
		/// Must be free of side effects: editor tools call it outside of the brain tick,
		/// several times per frame and while the game is paused.
		/// </summary>
		/// <param name="context">Context the owning brain was built with.</param>
		/// <returns>Score used by the aggregator to pick an action. Higher wins.</returns>
		float CalculateUtility(IUtilityContext context);
		void Exit(IUtilityContext context);
	}
}
