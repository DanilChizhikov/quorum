namespace DTech.Quorum.Editor
{
	internal readonly struct BrainConfigIssue
	{
		public BrainConfig Config { get; }
		public BrainConfigIssueSeverity Severity { get; }
		public BrainConfigIssueCode Code { get; }
		public string Message { get; }
		public string PropertyPath { get; }

		public BrainConfigIssue(
			BrainConfig config,
			BrainConfigIssueSeverity severity,
			BrainConfigIssueCode code,
			string message,
			string propertyPath = null)
		{
			Config = config;
			Severity = severity;
			Code = code;
			Message = message;
			PropertyPath = propertyPath;
		}
	}
}
