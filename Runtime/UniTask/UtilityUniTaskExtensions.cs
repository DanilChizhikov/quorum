namespace DTech.UtilityAI.Runtime.UniTask
{
	public static class UtilityUniTaskExtensions
	{
		public static IAsyncUtilityAction AsAsyncUtilityAction(this IUniTaskUtilityAction action)
		{
			return new UniTaskUtilityActionAdapter(action);
		}

		public static IUniTaskUtilityAction AsUniTaskUtilityAction(this IAsyncUtilityAction action)
		{
			return new AsyncUtilityActionUniTaskAdapter(action);
		}

		public static IAsyncUtilityConsideration AsAsyncUtilityConsideration(this IUniTaskUtilityConsideration consideration)
		{
			return new UniTaskUtilityConsiderationAdapter(consideration);
		}

		public static IUniTaskUtilityConsideration AsUniTaskUtilityConsideration(this IAsyncUtilityConsideration consideration)
		{
			return new AsyncUtilityConsiderationUniTaskAdapter(consideration);
		}
	}
}
