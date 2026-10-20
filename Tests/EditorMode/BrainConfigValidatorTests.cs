using System.Collections.Generic;
using System.Text;
using DTech.UtilityAI.Editor;
using NUnit.Framework;
using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class BrainConfigValidatorTests
	{
		private readonly List<BrainConfig> _configs = new();

		[TearDown]
		public void TearDown()
		{
			for (int i = 0; i < _configs.Count; i++)
			{
				Object.DestroyImmediate(_configs[i]);
			}

			_configs.Clear();
		}

		[Test]
		public void Validate_MissingScoreAggregator_ReportsError()
		{
			BrainConfig config = CreateConfig(CreateScoredAction());
			config.SetScoreAggregator(null);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.MissingScoreAggregator, BrainConfigIssueSeverity.Error);
		}

		[Test]
		public void Validate_EmptyActionList_ReportsWarning()
		{
			BrainConfig config = CreateConfig();

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.EmptyActionList, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_NullActionEntry_ReportsError()
		{
			BrainConfig config = CreateConfig(CreateScoredAction(), null);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.NullActionEntry, BrainConfigIssueSeverity.Error);
		}

		[Test]
		public void Validate_DuplicateActionType_ReportsWarning()
		{
			BrainConfig config = CreateConfig(CreateScoredAction(), CreateScoredAction());

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.DuplicateActionType, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_ActionWithoutScoreSource_ReportsWarning()
		{
			BrainConfig config = CreateConfig(new TestScorelessUtilityAction());

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.ActionWithoutScoreSource, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_EmptyScriptableActionReference_ReportsError()
		{
			BrainConfig config = CreateConfig(new ScriptableUtilityActionReference(null));

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.EmptyScriptableActionReference, BrainConfigIssueSeverity.Error);
		}

		[Test]
		public void Validate_CompositeWithoutChildren_ReportsWarning()
		{
			var action = new TestUtilityAction(1f);
			action.SetConsideration(new CompositeUtilityConsideration());
			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.CompositeWithoutChildren, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_CompositeWithSingleChild_ReportsInfo()
		{
			var action = new TestUtilityAction(1f);
			action.SetConsideration(new CompositeUtilityConsideration(
				ConsiderationCombineMode.Multiply,
				new TestUtilityConsideration(1f)));

			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.CompositeWithSingleChild, BrainConfigIssueSeverity.Info);
		}

		[Test]
		public void Validate_NullChildInComposite_ReportsWarning()
		{
			var action = new TestUtilityAction(1f);
			action.SetConsideration(new CompositeUtilityConsideration(
				ConsiderationCombineMode.Multiply,
				new TestUtilityConsideration(1f),
				null));

			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.NullManagedReferenceElement, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_ValidConfig_ReportsNothing()
		{
			BrainConfig config = CreateConfig(CreateScoredAction());

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			Assert.AreEqual(0, issues.Count, DescribeIssues(issues));
		}

		[Test]
		public void FixMissingScoreAggregator_AssignsMaxAggregator()
		{
			BrainConfig config = CreateConfig(CreateScoredAction());
			config.SetScoreAggregator(null);
			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			Assert.AreEqual(1, BrainConfigIssueFixer.FixAll(issues));
			Assert.IsInstanceOf<MaxUtilityScoreAggregator>(config.ScoreAggregator);
		}

		private static TestUtilityAction CreateScoredAction()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new TestUtilityConsideration(0.5f));
			return action;
		}

		private static void AssertHasIssue(
			List<BrainConfigIssue> issues,
			BrainConfigIssueCode code,
			BrainConfigIssueSeverity severity)
		{
			for (int i = 0; i < issues.Count; i++)
			{
				if (issues[i].Code != code)
				{
					continue;
				}

				Assert.AreEqual(severity, issues[i].Severity);
				return;
			}

			Assert.Fail($"Expected issue {code}. {DescribeIssues(issues)}");
		}

		private static string DescribeIssues(List<BrainConfigIssue> issues)
		{
			var builder = new StringBuilder("Reported issues: ");
			for (int i = 0; i < issues.Count; i++)
			{
				builder.Append(issues[i].Code).Append(" (").Append(issues[i].Message).Append("); ");
			}

			return builder.ToString();
		}

		private BrainConfig CreateConfig(params UtilityAction[] actions)
		{
			var config = ScriptableObject.CreateInstance<TestBrainConfig>();
			config.SetActions(actions);
			_configs.Add(config);
			return config;
		}
	}
}
