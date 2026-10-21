using System;
using System.Collections.Generic;
using DTech.Quorum.Diagnostics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DTech.Quorum.Editor
{
	internal sealed class BrainMapWindow : EditorWindow
	{
		public const string MenuPath = "Window/DTech/Quorum/Brain Map";

		private const string WindowTitle = "Quorum Brain Map";
		private const string StyleSheetName = "BrainMapWindow";
		private const string ConfigLabel = "Config";
		private const string RebuildLabel = "Rebuild";
		private const string LiveMessage = "Live brain attached: scores update while playing.";
		private const string StaticMessage = "No live brain for this config: the map shows structure only.";
		private const string EmptyMessage = "Select a BrainConfig asset to map it.";
		private const int RefreshIntervalMs = 100;

		private readonly List<IBrainDebugInfo> _brains = new();
		private readonly List<IQuorumAction> _actions = new();

		private BrainMapGraphView _graphView;
		private ObjectField _configField;
		private Label _status;
		private BrainConfig _builtConfig;
		private WeakReference<IBrainDebugInfo> _builtBrain;
		private int _builtActionCount;

		[MenuItem(MenuPath)]
		private static void Open()
		{
			var window = GetWindow<BrainMapWindow>();
			window.titleContent = new GUIContent(WindowTitle);
			window.Show();
		}

		private static IBrainDebugInfo FindBrain(List<IBrainDebugInfo> brains, BrainConfig config)
		{
			if (config == null)
			{
				return null;
			}

			for (int i = 0; i < brains.Count; i++)
			{
				if (brains[i].SourceConfig != config)
				{
					continue;
				}

				return brains[i];
			}

			return null;
		}

		private void OnEnable()
		{
			BrainDebugRegistry.AddObserver();
		}

		private void OnDisable()
		{
			BrainDebugRegistry.RemoveObserver();
		}

		private void CreateGUI()
		{
			rootVisualElement.AddToClassList("uai-root");
			var styleSheet = Resources.Load<StyleSheet>(StyleSheetName);
			if (styleSheet != null)
			{
				rootVisualElement.styleSheets.Add(styleSheet);
			}

			var toolbar = new Toolbar();
			_configField = new ObjectField(ConfigLabel)
			{
				objectType = typeof(BrainConfig),
				allowSceneObjects = false,
			};

			_configField.AddToClassList("uai-map__config");
			_configField.RegisterValueChangedCallback(OnConfigChanged);
			toolbar.Add(_configField);
			toolbar.Add(new ToolbarButton(Rebuild) { text = RebuildLabel });
			rootVisualElement.Add(toolbar);

			_status = new Label(EmptyMessage);
			_status.AddToClassList("uai-map__status");
			rootVisualElement.Add(_status);

			_graphView = new BrainMapGraphView();
			rootVisualElement.Add(_graphView);

			OnSelectionChange();
			rootVisualElement.schedule.Execute(Refresh).Every(RefreshIntervalMs);
		}

		private void OnSelectionChange()
		{
			if (_configField == null || Selection.activeObject is not BrainConfig config)
			{
				return;
			}

			_configField.value = config;
		}

		private void OnConfigChanged(ChangeEvent<Object> changeEvent)
		{
			Rebuild();
		}

		private void Rebuild()
		{
			_builtConfig = null;
			_builtBrain = null;
			_builtActionCount = -1;
			Refresh();
		}

		private void Refresh()
		{
			if (_graphView == null)
			{
				return;
			}

			var config = _configField.value as BrainConfig;
			BrainDebugRegistry.GetBrainsNonAlloc(_brains);
			IBrainDebugInfo brain = FindBrain(_brains, config);
			CollectActions(config, brain);
			if (config != _builtConfig || !IsBuiltBrain(brain) || _actions.Count != _builtActionCount)
			{
				_builtConfig = config;
				_builtBrain = brain != null ? new WeakReference<IBrainDebugInfo>(brain) : null;
				_builtActionCount = _actions.Count;
				_graphView.Build(_actions, config != null ? config.ScoreAggregator : null);
			}

			_status.text = config == null ? EmptyMessage : brain != null ? LiveMessage : StaticMessage;
			_graphView.UpdateScores(brain?.Context, brain?.CurrentAction);
		}

		private bool IsBuiltBrain(IBrainDebugInfo brain)
		{
			if (_builtBrain == null)
			{
				return brain == null;
			}

			return _builtBrain.TryGetTarget(out IBrainDebugInfo builtBrain)
				? ReferenceEquals(builtBrain, brain)
				: brain == null;
		}

		private void CollectActions(BrainConfig config, IBrainDebugInfo brain)
		{
			_actions.Clear();
			if (brain != null)
			{
				IReadOnlyList<IQuorumAction> brainActions = brain.Actions;
				for (int i = 0; i < brainActions.Count; i++)
				{
					_actions.Add(brainActions[i]);
				}

				return;
			}

			IReadOnlyList<QuorumAction> configActions = config?.Actions;
			for (int i = 0; i < (configActions?.Count ?? 0); i++)
			{
				if (configActions[i] == null)
				{
					continue;
				}

				_actions.Add(configActions[i]);
			}
		}
	}
}
