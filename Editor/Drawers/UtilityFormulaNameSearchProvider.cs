using System;
using System.Collections.Generic;
using DTech.Blackboard;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityFormulaNameSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		private const string Title = "Blackboard Variables";

		private static UtilityFormulaNameSearchProvider _provider;

		private BlackboardAsset _schema;
		private Action<string> _onNameSelected;
		private int _generation;

		public static UtilityFormulaNameSearchProvider Create(BlackboardAsset schema, Action<string> onNameSelected)
		{
			if (_provider == null)
			{
				_provider = CreateInstance<UtilityFormulaNameSearchProvider>();
				_provider.hideFlags = HideFlags.HideAndDontSave;
			}

			_provider._schema = schema;
			_provider._onNameSelected = onNameSelected;
			_provider._generation++;
			return _provider;
		}

		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			var tree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent(Title), 0) };
			if (!UtilityFormulaSchema.HasNames(_schema))
			{
				return tree;
			}

			int generation = _generation;
			IReadOnlyList<BlackboardVariable> variables = _schema.Variables;
			for (int i = 0; i < variables.Count; i++)
			{
				BlackboardVariable variable = variables[i];
				if (variable == null || string.IsNullOrWhiteSpace(variable.Name))
				{
					continue;
				}

				tree.Add(new SearchTreeEntry(new GUIContent(variable.Name))
				{
					level = 1,
					userData = new Entry(generation, variable.Name),
				});
			}

			return tree;
		}

		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			if (searchTreeEntry.userData is not Entry entry || entry.Generation != _generation)
			{
				return true;
			}

			Action<string> onNameSelected = _onNameSelected;
			_schema = null;
			_onNameSelected = null;
			onNameSelected?.Invoke(entry.Name);
			return true;
		}

		private void OnDisable()
		{
			_schema = null;
			_onNameSelected = null;
		}

		private readonly struct Entry
		{
			public int Generation { get; }
			public string Name { get; }

			public Entry(int generation, string name)
			{
				Generation = generation;
				Name = name;
			}
		}
	}
}
