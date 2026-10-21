using System;
using System.Threading;
using DTech.Blackboard;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI.Tests.Performance
{
	internal sealed class PerfBrainSet : IDisposable
	{
		public const string HungerKey = "Hunger";
		public const string EnergyKey = "Energy";
		public const string ThreatDistanceKey = "ThreatDistance";
		public const string FoodAvailableKey = "FoodAvailable";

		private static readonly string[] _floatKeys = { HungerKey, EnergyKey, ThreatDistanceKey };

		private readonly PerfBrainConfig _config;
		private readonly PerfScriptableAction[] _scriptableActions;
		private readonly RuntimeBlackboard[] _blackboards;
		private readonly PerfUtilityContext[] _contexts;
		private readonly PerfUtilityBrain[] _brains;

		public PerfBrainConfig Config => _config;
		public int Count => _brains.Length;
		public PerfUtilityBrain First => _brains[0];
		public PerfUtilityBrain this[int index] => _brains[index];

		private bool _isDisposed;

		private PerfBrainSet(
			PerfBrainConfig config,
			PerfScriptableAction[] scriptableActions,
			RuntimeBlackboard[] blackboards,
			PerfUtilityContext[] contexts)
		{
			_config = config;
			_scriptableActions = scriptableActions;
			_blackboards = blackboards;
			_contexts = contexts;
			_brains = new PerfUtilityBrain[contexts.Length];
		}

		public static PerfBrainSet Create(in PerfBrainSetup setup)
		{
			PerfBrainSet set = CreateWithoutBrains(setup);
			set.BuildBrains();
			return set;
		}

		public static PerfBrainSet CreateWithoutBrains(in PerfBrainSetup setup)
		{
			PerfBrainConfig config = CreateConfig(setup, out PerfScriptableAction[] scriptableActions);
			var blackboards = new RuntimeBlackboard[setup.BrainCount];
			var contexts = new PerfUtilityContext[setup.BrainCount];
			for (int i = 0; i < setup.BrainCount; i++)
			{
				blackboards[i] = CreateBlackboard();
				contexts[i] = new PerfUtilityContext(new PerfUtilityAgent(), blackboards[i]);
			}

			return new PerfBrainSet(config, scriptableActions, blackboards, contexts);
		}

		public static RuntimeBlackboard CreateBlackboard()
		{
			var blackboard = new RuntimeBlackboard(4);
			blackboard.Add(new BlackboardVariable<float> { Name = HungerKey, Value = 0.4f });
			blackboard.Add(new BlackboardVariable<float> { Name = EnergyKey, Value = 0.6f });
			blackboard.Add(new BlackboardVariable<float> { Name = ThreatDistanceKey, Value = 0.5f });
			blackboard.Add(new BlackboardVariable<bool> { Name = FoodAvailableKey, Value = true });
			return blackboard;
		}

		public void BuildBrains()
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i] = new PerfUtilityBrain(_config, _contexts[i]);
			}
		}

		public void DisposeBrains()
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i]?.Dispose();
				_brains[i] = null;
			}
		}

		public void EnableAll()
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i].Enable();
			}
		}

		public void DisableAll()
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i].Disable();
			}
		}

		public void TickAll()
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i].Update();
			}
		}

		public void RequestSwitchAll()
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i].RequestSwitchAction();
			}
		}

		public void RequestSwitchAll(float delay, CancellationToken token)
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				_brains[i].RequestSwitchAction(delay, token);
			}
		}

		public void SetScoreAggregator(UtilityScoreAggregator aggregator)
		{
			_config.SetScoreAggregator(aggregator);
		}

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;
			DisposeBrains();
			for (int i = 0; i < _blackboards.Length; i++)
			{
				_blackboards[i]?.Dispose();
			}

			for (int i = 0; i < _scriptableActions.Length; i++)
			{
				DestroyObject(_scriptableActions[i]);
			}

			DestroyObject(_config);
		}

		private static PerfBrainConfig CreateConfig(in PerfBrainSetup setup, out PerfScriptableAction[] scriptableActions)
		{
			var config = ScriptableObject.CreateInstance<PerfBrainConfig>();
			config.SetScoreAggregator(new MaxUtilityScoreAggregator());
			var actions = new UtilityAction[setup.ActionCount];
			scriptableActions = setup.ActionKind == PerfActionKind.ScriptableReference
				? new PerfScriptableAction[setup.ActionCount]
				: Array.Empty<PerfScriptableAction>();

			for (int i = 0; i < setup.ActionCount; i++)
			{
				UtilityConsideration consideration = CreateConsideration(setup, i);
				if (setup.ActionKind == PerfActionKind.ScriptableReference)
				{
					var scriptableAction = ScriptableObject.CreateInstance<PerfScriptableAction>();
					scriptableAction.SetConsideration(consideration);
					scriptableActions[i] = scriptableAction;
					actions[i] = new ScriptableUtilityActionReference(scriptableAction);
					continue;
				}

				var action = new PerfUtilityAction();
				action.SetConsideration(consideration);
				actions[i] = action;
			}

			config.SetActions(actions);
			return config;
		}

		private static UtilityConsideration CreateConsideration(in PerfBrainSetup setup, int actionIndex)
		{
			switch (setup.ConsiderationKind)
			{
				case PerfConsiderationKind.None:
				{
					return null;
				}
				case PerfConsiderationKind.Constant:
				{
					return new PerfConstantConsideration(GetScore(actionIndex, setup.ActionCount));
				}
				case PerfConsiderationKind.CompositeConstants:
				{
					var children = new UtilityConsideration[setup.ConsiderationCount];
					for (int i = 0; i < children.Length; i++)
					{
						children[i] = new PerfConstantConsideration(GetScore(actionIndex + i, setup.ActionCount + children.Length));
					}

					return new CompositeUtilityConsideration(ConsiderationCombineMode.Multiply, children);
				}
				case PerfConsiderationKind.CompositeBlackboard:
				{
					var children = new UtilityConsideration[setup.ConsiderationCount];
					for (int i = 0; i < children.Length; i++)
					{
						children[i] = new PerfBlackboardConsideration(_floatKeys[(actionIndex + i) % _floatKeys.Length]);
					}

					return new CompositeUtilityConsideration(ConsiderationCombineMode.Multiply, children);
				}
				default:
				{
					throw new ArgumentOutOfRangeException(nameof(setup), setup.ConsiderationKind, null);
				}
			}
		}

		private static float GetScore(int index, int count) => (index + 1f) / (count + 1f);

		private static void DestroyObject(Object target)
		{
			if (target == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				Object.Destroy(target);
			}
			else
			{
				Object.DestroyImmediate(target);
			}
		}
	}
}
