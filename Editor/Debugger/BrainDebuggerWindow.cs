using System.Collections.Generic;
using DTech.Quorum.Diagnostics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DTech.Quorum.Editor
{
	internal sealed class BrainDebuggerWindow : EditorWindow
	{
		public const string MenuPath = "Window/DTech/Quorum/Debugger";

		private const string WindowTitle = "Quorum Debugger";
		private const string StyleSheetName = "QuorumDebuggerWindow";
		private const string PauseLabel = "Pause";
		private const string SelectConfigLabel = "Select Config";
		private const string ActionsTitle = "Actions";
		private const string ConsiderationsTitle = "Considerations Of Current Action";
		private const string SwitchLogTitle = "Switch Log";
		private const string NoBrainMessage = "Select a brain to inspect it.";
		private const int RefreshIntervalMs = 100;
		private const float ListPaneWidth = 220f;

		private readonly List<IBrainDebugInfo> _brains = new();
		private readonly ScoreSampler _sampler = new();

		private BrainListView _brainListView;
		private ActionTableView _actionTableView;
		private ConsiderationTreeView _considerationTreeView;
		private SwitchLogView _switchLogView;
		private VisualElement _details;
		private Label _header;
		private Label _placeholder;

		public static void Open(BrainConfig config)
		{
			var window = GetWindow<BrainDebuggerWindow>();
			window.titleContent = new GUIContent(WindowTitle);
			window.Show();
			window.SelectByConfig(config);
		}

		[MenuItem(MenuPath)]
		private static void Open()
		{
			var window = GetWindow<BrainDebuggerWindow>();
			window.titleContent = new GUIContent(WindowTitle);
			window.Show();
		}

		private static Label CreateSectionTitle(string text)
		{
			var title = new Label(text);
			title.AddToClassList("uai-section");
			return title;
		}

		private void OnEnable()
		{
			BrainDebugRegistry.AddObserver();
		}

		private void CreateGUI()
		{
			rootVisualElement.AddToClassList("uai-root");
			var styleSheet = Resources.Load<StyleSheet>(StyleSheetName);
			if (styleSheet != null)
			{
				rootVisualElement.styleSheets.Add(styleSheet);
			}

			rootVisualElement.Add(CreateToolbar());

			var split = new TwoPaneSplitView(0, ListPaneWidth, TwoPaneSplitViewOrientation.Horizontal);
			_brainListView = new BrainListView();
			_brainListView.SelectionChanged += OnSelectionChanged;
			split.Add(_brainListView);
			split.Add(CreateDetails());
			rootVisualElement.Add(split);

			rootVisualElement.schedule.Execute(Refresh).Every(RefreshIntervalMs);
			Refresh();
		}

		private void OnDisable()
		{
			BrainDebugRegistry.RemoveObserver();
			if (_brainListView == null)
			{
				return;
			}

			_brainListView.SelectionChanged -= OnSelectionChanged;
		}

		private VisualElement CreateToolbar()
		{
			var toolbar = new Toolbar();
			var pauseToggle = new ToolbarToggle { text = PauseLabel, value = _sampler.IsPaused };
			pauseToggle.RegisterValueChangedCallback(OnPauseChanged);
			toolbar.Add(pauseToggle);
			toolbar.Add(new ToolbarButton(SelectConfig) { text = SelectConfigLabel });
			return toolbar;
		}

		private VisualElement CreateDetails()
		{
			var scroll = new ScrollView();
			scroll.AddToClassList("uai-details");

			_header = new Label();
			_header.AddToClassList("uai-header");
			scroll.Add(_header);

			_placeholder = new Label(NoBrainMessage);
			_placeholder.AddToClassList("uai-empty");
			scroll.Add(_placeholder);

			_details = new VisualElement();
			_details.AddToClassList("uai-sections");
			scroll.Add(_details);

			_details.Add(CreateSectionTitle(ActionsTitle));
			_actionTableView = new ActionTableView();
			_details.Add(_actionTableView);

			_details.Add(CreateSectionTitle(ConsiderationsTitle));
			_considerationTreeView = new ConsiderationTreeView();
			_details.Add(_considerationTreeView);

			_details.Add(CreateSectionTitle(SwitchLogTitle));
			_switchLogView = new SwitchLogView();
			_details.Add(_switchLogView);
			return scroll;
		}

		private void OnPauseChanged(ChangeEvent<bool> changeEvent)
		{
			_sampler.IsPaused = changeEvent.newValue;
		}

		private void OnSelectionChanged()
		{
			_sampler.Clear();
			Refresh();
		}

		private void SelectConfig()
		{
			IBrainDebugInfo brain = _brainListView?.Selected;
			BrainConfig config = brain?.SourceConfig;
			if (config == null)
			{
				return;
			}

			Selection.activeObject = config;
			EditorGUIUtility.PingObject(config);
		}

		private void SelectByConfig(BrainConfig config)
		{
			if (config == null || _brainListView == null)
			{
				return;
			}

			BrainDebugRegistry.GetBrainsNonAlloc(_brains);
			_brainListView.Refresh(_brains);
			for (int i = 0; i < _brains.Count; i++)
			{
				if (_brains[i].SourceConfig != config)
				{
					continue;
				}

				_brainListView.TrySelect(_brains[i]);
				return;
			}
		}

		private void Refresh()
		{
			if (_brainListView == null)
			{
				return;
			}

			BrainDebugRegistry.GetBrainsNonAlloc(_brains);
			_brainListView.Refresh(_brains);
			IBrainDebugInfo brain = _brainListView.Selected;
			bool hasBrain = brain != null;
			_placeholder.style.display = hasBrain ? DisplayStyle.None : DisplayStyle.Flex;
			_details.style.display = hasBrain ? DisplayStyle.Flex : DisplayStyle.None;
			if (!hasBrain)
			{
				_header.text = string.Empty;
				return;
			}

			_header.text = $"{brain.ConfigName}    {brain.UpdateType}    {brain.SwitchActionType}    {(brain.IsEnabled ? "Enabled" : "Disabled")}";
			_sampler.Sample(brain, false);
			_actionTableView.Refresh(brain, _sampler);
			_considerationTreeView.Refresh(brain.CurrentAction, brain.Context);
			_switchLogView.Refresh(brain);
		}
	}
}
