using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityHiddenTypesWindow : EditorWindow
	{
		public const string MenuPath = "Window/DTech/Utility AI/Hidden Types";

		private const string WindowTitle = "Utility AI Hidden Types";
		private const string StyleSheetName = "UtilityHiddenTypesWindow";
		private const string RefreshLabel = "Refresh";
		private const string OpenScriptLabel = "Open";
		private const string FixLabel = "Add [Serializable]";
		private const string EmptyMessage = "No hidden types. Every derived type is visible in the picker.";
		private const string HintMessage = "These types derive from a Utility AI base type but are missing [Serializable], so the type picker hides them.";

		private ScrollView _list;
		private Label _summary;

		[MenuItem(MenuPath)]
		private static void Open()
		{
			var window = GetWindow<UtilityHiddenTypesWindow>();
			window.titleContent = new GUIContent(WindowTitle);
			window.Show();
		}

		private static void OpenScript(Type type)
		{
			MonoScript script = UtilityScriptLocator.Find(type);
			if (script == null)
			{
				return;
			}

			AssetDatabase.OpenAsset(script);
		}

		private static VisualElement CreateGroupHeader(Type baseType)
		{
			var header = new Label(baseType.Name);
			header.AddToClassList("uai-group");
			return header;
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
			toolbar.Add(new ToolbarButton(Rebuild) { text = RefreshLabel });
			rootVisualElement.Add(toolbar);

			_summary = new Label();
			_summary.AddToClassList("uai-summary");
			rootVisualElement.Add(_summary);

			var hint = new Label(HintMessage);
			hint.AddToClassList("uai-hint");
			rootVisualElement.Add(hint);

			_list = new ScrollView();
			_list.AddToClassList("uai-list");
			rootVisualElement.Add(_list);

			Rebuild();
		}

		private void OnEnable()
		{
			UtilityHiddenTypeReport.Changed += OnReportChanged;
		}

		private void OnDisable()
		{
			UtilityHiddenTypeReport.Changed -= OnReportChanged;
		}

		private void OnReportChanged()
		{
			Rebuild();
		}

		private void Rebuild()
		{
			if (_list == null)
			{
				return;
			}

			_list.Clear();
			int count = 0;
			foreach (KeyValuePair<Type, List<Type>> pair in UtilityHiddenTypeReport.GetHiddenTypes())
			{
				if (pair.Value.Count <= 0)
				{
					continue;
				}

				_list.Add(CreateGroupHeader(pair.Key));
				for (int i = 0; i < pair.Value.Count; i++)
				{
					_list.Add(CreateRow(pair.Value[i]));
					count++;
				}
			}

			if (count <= 0)
			{
				var empty = new Label(EmptyMessage);
				empty.AddToClassList("uai-empty");
				_list.Add(empty);
			}

			_summary.text = $"{count} hidden type(s).";
		}

		private VisualElement CreateRow(Type type)
		{
			var row = new VisualElement();
			row.AddToClassList("uai-row");

			var name = new Label(type.FullName);
			name.AddToClassList("uai-row__message");
			row.Add(name);

			var open = new Button(() => OpenScript(type)) { text = OpenScriptLabel };
			open.AddToClassList("uai-row__button");
			row.Add(open);
			if (!UtilitySerializableAttributeInserter.CanInsert(type))
			{
				return row;
			}

			var fix = new Button(() => Fix(type)) { text = FixLabel };
			fix.AddToClassList("uai-row__button");
			row.Add(fix);
			return row;
		}

		private void Fix(Type type)
		{
			if (!UtilitySerializableAttributeInserter.TryInsert(type))
			{
				return;
			}

			Rebuild();
		}
	}
}
