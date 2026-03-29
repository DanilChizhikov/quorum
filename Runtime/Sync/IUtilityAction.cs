namespace DTech.UtilityAI
{
	public interface IUtilityAction
	{
		void Initialize(IUtilityContext context);
		void Enter(IUtilityContext context);
		UtilityActionState Tick(IUtilityContext context);
		float CalculateUtility(IUtilityContext context);
		void Exit(IUtilityContext context);
		void Cancel(IUtilityContext context);
	}
}
