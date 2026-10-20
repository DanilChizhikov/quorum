namespace DTech.UtilityAI.Editor
{
	internal static class UtilityActionLabel
	{
		private const string EmptyLabel = "None";

		public static string Get(IUtilityAction action)
		{
			if (action == null)
			{
				return EmptyLabel;
			}

			if (action is ScriptableUtilityActionReference reference && reference.Action != null)
			{
				return reference.Action.name;
			}

			return UtilityTypeRegistry.GetDisplayName(action.GetType());
		}
	}
}
