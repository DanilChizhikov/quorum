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

		private BlackboardAsset _schema;
		private Action<string> _onNameSelected;

		public static UtilityFormulaNameSearchProvider Create(BlackboardAsset schema, Action<string> onNameSelected)
		{
			var provider = CreateInstance<UtilityFormulaNameSearchProvider>();
			provider._schema = schema;
			provider._onNameSelected = onNameSelected;
			return provider;
		}

		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			var tree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent(Title), 0) };
			if (!UtilityFormulaSchema.HasNames(_schema))
			{
				return tree;
			}

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
					userData = variable.Name,
				});
			}

			return tree;
		}

		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			_onNameSelected?.Invoke(searchTreeEntry.userData as string);
			return true;
		}
	}
}
