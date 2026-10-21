using System;
using System.Collections.Generic;
using DTech.Blackboard;
using DTech.Quorum.Diagnostics;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal static class ScoringMatrixRunner
	{
		private const string MissingScenarioError = "Scenario is missing.";
		private const string MissingConfigError = "Scenario has no BrainConfig.";
		private const string MissingProbeError = "Scenario has no ScoringProbe: a context cannot be built.";
		private const string UnnamedCaseLabel = "Unnamed case";

		public static ScoringMatrix Run(ScoringScenario scenario)
		{
			if (scenario == null)
			{
				return new ScoringMatrix(Array.Empty<string>(), MissingScenarioError);
			}

			return Run(scenario.Config, scenario.Probe, scenario.Cases);
		}

		public static ScoringMatrix Run(
			BrainConfig config,
			ScoringProbe probe,
			IReadOnlyList<ScoringCase> cases)
		{
			if (config == null)
			{
				return new ScoringMatrix(Array.Empty<string>(), MissingConfigError);
			}

			if (probe == null)
			{
				return new ScoringMatrix(Array.Empty<string>(), MissingProbeError);
			}

			ScoringActionInfo actionInfo = CollectActionInfo(config);
			var matrix = new ScoringMatrix(actionInfo.Names);
			for (int i = 0; i < (cases?.Count ?? 0); i++)
			{
				ScoringCase scoringCase = cases[i];
				if (scoringCase == null)
				{
					continue;
				}

				matrix.Add(RunCase(new ScoringRunContext(config, probe), scoringCase, actionInfo));
			}

			return matrix;
		}

		private static ScoringActionInfo CollectActionInfo(BrainConfig config)
		{
			var names = new List<string>();
			var typeNames = new List<string>();
			IReadOnlyList<QuorumAction> actions = config.Actions;
			for (int i = 0; i < (actions?.Count ?? 0); i++)
			{
				QuorumAction action = actions[i];
				if (action == null)
				{
					continue;
				}

				names.Add(ActionLabel.Get(action));
				typeNames.Add(action.GetType().FullName);
			}

			return new ScoringActionInfo(names, typeNames);
		}

		private static ScoringRow RunCase(
			in ScoringRunContext runContext,
			ScoringCase scoringCase,
			in ScoringActionInfo actionInfo)
		{
			int count = actionInfo.Names.Count;
			var scores = new float[count];
			var errors = new string[count];
			int winnerIndex = -1;
			RuntimeBlackboard blackboard = scoringCase.Blackboard != null
				? scoringCase.Blackboard.ToRuntimeBlackboard()
				: new RuntimeBlackboard();

			List<IQuorumAction> actions = null;
			try
			{
				IQuorumContext context = runContext.Probe.CreateContext(blackboard);
				actions = PrepareActions(runContext.Config, context, runContext.Probe.CreateInjector());
				for (int i = 0; i < actions.Count && i < count; i++)
				{
					scores[i] = ScoreSampler.Evaluate(actions[i], context, out string error);
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
			return new ScoringRow(label, scores, errors, winnerIndex, expectedIndex);
		}

		private static List<IQuorumAction> PrepareActions(BrainConfig config, IQuorumContext context, IQuorumActionInjector injector)
		{
			IQuorumActionInjector actionInjector = injector ?? new NullActionInjector();
			var actions = new List<IQuorumAction>();
			IReadOnlyList<QuorumAction> configActions = config.Actions;
			for (int i = 0; i < (configActions?.Count ?? 0); i++)
			{
				QuorumAction configAction = configActions[i];
				if (configAction == null)
				{
					continue;
				}

				QuorumAction action = configAction.Clone();
				actionInjector.Inject(action);
				action.Initialize(context);
				action.InitializeConsideration(context);
				actions.Add(action);
			}

			return actions;
		}

		private static int Aggregate(ScoreAggregator aggregator, List<IQuorumAction> actions, IQuorumContext context)
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

		private static void DisposeActions(List<IQuorumAction> actions)
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

		private static int FindExpectedIndex(in ScoringActionInfo actionInfo, string expectedActionType)
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

		private static string GetFallbackLabel(ScoringCase scoringCase) =>
			scoringCase.Blackboard != null ? scoringCase.Blackboard.name : UnnamedCaseLabel;
	}
}
