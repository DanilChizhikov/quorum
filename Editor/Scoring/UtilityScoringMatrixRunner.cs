using System;
using System.Collections.Generic;
using DTech.Blackboard;
using DTech.UtilityAI.Diagnostics;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityScoringMatrixRunner
	{
		private const string MissingScenarioError = "Scenario is missing.";
		private const string MissingConfigError = "Scenario has no BrainConfig.";
		private const string MissingProbeError = "Scenario has no UtilityScoringProbe: a context cannot be built.";
		private const string UnnamedCaseLabel = "Unnamed case";

		public static UtilityScoringMatrix Run(UtilityScoringScenario scenario)
		{
			if (scenario == null)
			{
				return new UtilityScoringMatrix(Array.Empty<string>(), MissingScenarioError);
			}

			return Run(scenario.Config, scenario.Probe, scenario.Cases);
		}

		public static UtilityScoringMatrix Run(
			BrainConfig config,
			UtilityScoringProbe probe,
			IReadOnlyList<UtilityScoringCase> cases)
		{
			if (config == null)
			{
				return new UtilityScoringMatrix(Array.Empty<string>(), MissingConfigError);
			}

			if (probe == null)
			{
				return new UtilityScoringMatrix(Array.Empty<string>(), MissingProbeError);
			}

			UtilityScoringActionInfo actionInfo = CollectActionInfo(config);
			var matrix = new UtilityScoringMatrix(actionInfo.Names);
			for (int i = 0; i < (cases?.Count ?? 0); i++)
			{
				UtilityScoringCase scoringCase = cases[i];
				if (scoringCase == null)
				{
					continue;
				}

				matrix.Add(RunCase(new UtilityScoringRunContext(config, probe), scoringCase, actionInfo));
			}

			return matrix;
		}

		private static UtilityScoringActionInfo CollectActionInfo(BrainConfig config)
		{
			var names = new List<string>();
			var typeNames = new List<string>();
			IReadOnlyList<UtilityAction> actions = config.Actions;
			for (int i = 0; i < (actions?.Count ?? 0); i++)
			{
				UtilityAction action = actions[i];
				if (action == null)
				{
					continue;
				}

				names.Add(UtilityActionLabel.Get(action));
				typeNames.Add(action.GetType().FullName);
			}

			return new UtilityScoringActionInfo(names, typeNames);
		}

		private static UtilityScoringRow RunCase(
			in UtilityScoringRunContext runContext,
			UtilityScoringCase scoringCase,
			in UtilityScoringActionInfo actionInfo)
		{
			int count = actionInfo.Names.Count;
			var scores = new float[count];
			var errors = new string[count];
			int winnerIndex = -1;
			RuntimeBlackboard blackboard = scoringCase.Blackboard != null
				? scoringCase.Blackboard.ToRuntimeBlackboard()
				: new RuntimeBlackboard();

			List<IUtilityAction> actions = null;
			try
			{
				IUtilityContext context = runContext.Probe.CreateContext(blackboard);
				actions = PrepareActions(runContext.Config, context, runContext.Probe.CreateInjector());
				for (int i = 0; i < actions.Count && i < count; i++)
				{
					scores[i] = UtilityScoreSampler.Evaluate(actions[i], context, out string error);
					errors[i] = error;
				}

				winnerIndex = Aggregate(runContext.Config.ScoreAggregator, actions, context);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				if (actions != null)
				{
					DisposeActions(actions);
				}

				blackboard.Dispose();
			}

			string label = string.IsNullOrEmpty(scoringCase.Label)
				? GetFallbackLabel(scoringCase)
				: scoringCase.Label;

			int expectedIndex = FindExpectedIndex(actionInfo, scoringCase.ExpectedActionType);
			return new UtilityScoringRow(label, scores, errors, winnerIndex, expectedIndex);
		}

		private static List<IUtilityAction> PrepareActions(BrainConfig config, IUtilityContext context, IUtilityActionInjector injector)
		{
			IUtilityActionInjector actionInjector = injector ?? new NullUtilityActionInjector();
			var actions = new List<IUtilityAction>();
			IReadOnlyList<UtilityAction> configActions = config.Actions;
			for (int i = 0; i < (configActions?.Count ?? 0); i++)
			{
				UtilityAction configAction = configActions[i];
				if (configAction == null)
				{
					continue;
				}

				UtilityAction action = configAction.Clone();
				actionInjector.Inject(action);
				action.Initialize(context);
				action.InitializeConsideration(context);
				actions.Add(action);
			}

			return actions;
		}

		private static int Aggregate(UtilityScoreAggregator aggregator, List<IUtilityAction> actions, IUtilityContext context)
		{
			if (aggregator == null || actions.Count <= 0)
			{
				return -1;
			}

			try
			{
				return aggregator.Aggregate(actions, context);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return -1;
			}
		}

		private static void DisposeActions(List<IUtilityAction> actions)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i] is not IDisposable disposable)
				{
					continue;
				}

				disposable.Dispose();
			}
		}

		private static int FindExpectedIndex(in UtilityScoringActionInfo actionInfo, string expectedActionType)
		{
			if (string.IsNullOrWhiteSpace(expectedActionType))
			{
				return -1;
			}

			string expected = expectedActionType.Trim();
			for (int i = 0; i < actionInfo.TypeNames.Count; i++)
			{
				string typeName = actionInfo.TypeNames[i];
				if (!string.Equals(typeName, expected, StringComparison.OrdinalIgnoreCase) &&
					!typeName.EndsWith(string.Concat(".", expected), StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(actionInfo.Names[i], expected, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				return i;
			}

			return -1;
		}

		private static string GetFallbackLabel(UtilityScoringCase scoringCase) =>
			scoringCase.Blackboard != null ? scoringCase.Blackboard.name : UnnamedCaseLabel;
	}
}
