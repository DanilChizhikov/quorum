namespace DTech.Quorum.Editor
{
	internal static class ActionLabel
	{
		private const string EmptyLabel = "None";

		public static string Get(IQuorumAction action)
		{
			if (action == null)
			{
				return EmptyLabel;
			}

			if (action is ScriptableQuorumActionReference reference && reference.Action != null)
			{
				return reference.Action.name;
			}

			return TypeRegistry.GetDisplayName(action.GetType());
		}
	}
}
