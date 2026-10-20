namespace DTech.UtilityAI.Editor
{
	internal enum BrainConfigIssueCode
	{
		MissingScoreAggregator = 0,
		NullActionEntry = 1,
		MissingManagedType = 2,
		EmptyActionList = 3,
		DuplicateActionType = 4,
		LegacyActionsNotUpgraded = 5,
		EmptyScriptableActionReference = 6,
		NullManagedReferenceElement = 7,
		CompositeWithoutChildren = 8,
		CompositeWithSingleChild = 9,
		ActionWithoutScoreSource = 10,
	}
}
