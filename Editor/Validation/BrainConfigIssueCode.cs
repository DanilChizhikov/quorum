namespace DTech.UtilityAI.Editor
{
	internal enum BrainConfigIssueCode
	{
		MissingScoreAggregator = 0,
		NullActionEntry = 1,
		MissingManagedType = 2,
		EmptyActionList = 3,
		DuplicateActionType = 4,
		EmptyScriptableActionReference = 5,
		NullManagedReferenceElement = 6,
		CompositeWithoutChildren = 7,
		CompositeWithSingleChild = 8,
		ActionWithoutScoreSource = 9,
		InvalidFormula = 10,
		UnknownFormulaVariable = 11,
	}
}
