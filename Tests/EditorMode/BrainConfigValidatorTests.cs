using System.Collections.Generic;
using System.Text;
using DTech.Blackboard;
using DTech.UtilityAI.Editor;
using NUnit.Framework;
using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class BrainConfigValidatorTests
	{
		private readonly List<BrainConfig> _configs = new();
		private readonly List<BlackboardAsset> _blackboards = new();
		private readonly List<ScriptableUtilityAction> _actionAssets = new();

		[TearDown]
		public void TearDown()
		{
			for (int i = 0; i < _configs.Count; i++)
			{
				Object.DestroyImmediate(_configs[i]);
			}

			_configs.Clear();
			for (int i = 0; i < _blackboards.Count; i++)
			{
				Object.DestroyImmediate(_blackboards[i]);
			}

			_blackboards.Clear();
			for (int i = 0; i < _actionAssets.Count; i++)
			{
				Object.DestroyImmediate(_actionAssets[i]);
			}

			_actionAssets.Clear();
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
		public void Validate_ReferenceToScorelessScriptableAction_ReportsWarning()
		{
			TestScorelessScriptableAction asset = CreateActionAsset<TestScorelessScriptableAction>();
			BrainConfig config = CreateConfig(new ScriptableUtilityActionReference(asset));

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.ActionWithoutScoreSource, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_ReferenceToScoredScriptableAction_ReportsNoWarning()
		{
			TestScriptableAction asset = CreateActionAsset<TestScriptableAction>();
			asset.SetUtility(0.5f);
			BrainConfig config = CreateConfig(new ScriptableUtilityActionReference(asset));

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasNoIssue(issues, BrainConfigIssueCode.ActionWithoutScoreSource);
		}

		[Test]
		public void Validate_BaseAndDerivedContextActions_ReportsNoMismatch()
		{
			BrainConfig config = CreateConfig(
				new TestBaseContextUtilityAction(),
				new TestDerivedContextUtilityAction());

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasNoIssue(issues, BrainConfigIssueCode.ContextTypeMismatch);
		}

		[Test]
		public void Validate_UnrelatedContextActions_ReportsError()
		{
			BrainConfig config = CreateConfig(
				new TestBaseContextUtilityAction(),
				new TestOtherContextUtilityAction());

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.ContextTypeMismatch, BrainConfigIssueSeverity.Error);
		}

		[Test]
		public void Validate_FormulaOutsideConsiderationOfSharedAsset_ReportsError()
		{
			TestSharedFormulaScriptableAction asset = CreateActionAsset<TestSharedFormulaScriptableAction>();
			asset.SetFallbackConsideration(new FormulaUtilityConsideration("clamp01(Hunger / 100)"));
			BrainConfig config = CreateConfig(new ScriptableUtilityActionReference(asset));

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.FormulaOnSharedAsset, BrainConfigIssueSeverity.Error);
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

		[Test]
		public void Validate_InvalidFormula_ReportsError()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new FormulaUtilityConsideration("1 +"));
			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.InvalidFormula, BrainConfigIssueSeverity.Error);
		}

		[Test]
		public void Validate_InvalidFormulaInsideComposite_ReportsError()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new CompositeUtilityConsideration(
				ConsiderationCombineMode.Multiply,
				new TestUtilityConsideration(0.5f),
				new FormulaUtilityConsideration("clamp01(")));

			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.InvalidFormula, BrainConfigIssueSeverity.Error);
		}

		[Test]
		public void Validate_ValidFormula_ReportsNoFormulaIssue()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new FormulaUtilityConsideration("clamp01(Hunger / 100)"));
			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasNoIssue(issues, BrainConfigIssueCode.InvalidFormula);
		}

		[Test]
		public void Validate_InvalidFormula_HasNoAutomaticFix()
		{
			Assert.IsFalse(BrainConfigIssueFixer.CanFix(BrainConfigIssueCode.InvalidFormula));
		}

		[Test]
		public void Validate_FormulaNameMissingFromSchema_ReportsWarning()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new FormulaUtilityConsideration("clamp01(Hungr / 100)"));
			BrainConfig config = CreateConfig(action);
			config.SetBlackboardSchema(CreateSchema("Hunger"));

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasIssue(issues, BrainConfigIssueCode.UnknownFormulaVariable, BrainConfigIssueSeverity.Warning);
		}

		[Test]
		public void Validate_FormulaNameOnSchema_ReportsNoWarning()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new FormulaUtilityConsideration("clamp01(hunger / 100)"));
			BrainConfig config = CreateConfig(action);
			config.SetBlackboardSchema(CreateSchema("Hunger"));

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasNoIssue(issues, BrainConfigIssueCode.UnknownFormulaVariable);
		}

		[Test]
		public void Validate_WithoutSchema_ReportsNoUnknownNameWarning()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new FormulaUtilityConsideration("clamp01(Hungr / 100)"));
			BrainConfig config = CreateConfig(action);

			List<BrainConfigIssue> issues = BrainConfigValidationScanner.Scan(config);

			AssertHasNoIssue(issues, BrainConfigIssueCode.UnknownFormulaVariable);
		}

		[Test]
		public void Validate_UnknownFormulaName_HasNoAutomaticFix()
		{
			Assert.IsFalse(BrainConfigIssueFixer.CanFix(BrainConfigIssueCode.UnknownFormulaVariable));
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

		private static void AssertHasNoIssue(List<BrainConfigIssue> issues, BrainConfigIssueCode code)
		{
			for (int i = 0; i < issues.Count; i++)
			{
				if (issues[i].Code != code)
				{
					continue;
				}

				Assert.Fail($"Unexpected issue {code}. {DescribeIssues(issues)}");
			}
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

		private BlackboardAsset CreateSchema(params string[] names)
		{
			var schema = ScriptableObject.CreateInstance<BlackboardAsset>();
			for (int i = 0; i < names.Length; i++)
			{
				schema.AddVariable(names[i], typeof(float));
			}

			_blackboards.Add(schema);
			return schema;
		}

		private T CreateActionAsset<T>()
			where T : ScriptableUtilityAction
		{
			var asset = ScriptableObject.CreateInstance<T>();
			_actionAssets.Add(asset);
			return asset;
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
