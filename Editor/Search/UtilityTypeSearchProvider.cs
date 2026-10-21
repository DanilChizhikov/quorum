using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityTypeSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		private const string NoneEntryName = "None";
		
		private static UtilityTypeSearchProvider _provider;
		
		private string _title;
		private IReadOnlyList<UtilityTypeOption> _options;
		private Action<Type> _onTypeSelected;
		
		/// <summary>
		/// Reuses one instance: a provider created per picker leaks a ScriptableObject every time the
		/// window closes without a selection, and only one picker is open at a time.
		/// </summary>
		public static UtilityTypeSearchProvider Create(string title, IReadOnlyList<UtilityTypeOption> options, Action<Type> onTypeSelected)
		{
			if (_provider == null)
			{
				_provider = CreateInstance<UtilityTypeSearchProvider>();
				_provider.hideFlags = HideFlags.HideAndDontSave;
			}
			
			_provider._title = title;
			_provider._options = options;
			_provider._onTypeSelected = onTypeSelected;
			return _provider;
		}
		
		private static GUIContent GetEntryContent(UtilityTypeOption option)
		{
			bool hasIcon = !string.IsNullOrEmpty(option.IconName);
			Texture icon = hasIcon ? EditorGUIUtility.IconContent(option.IconName).image : null;
			return hasIcon ? new GUIContent(option.Name, icon) : new GUIContent(option.Name);
		}
		
		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			var tree = new List<SearchTreeEntry>
			{
				new SearchTreeGroupEntry(new GUIContent(_title), 0),
				new SearchTreeEntry(new GUIContent(NoneEntryName))
				{
					level = 1,
					userData = null,
				},
			};
			
			var pathSet = new HashSet<string>();
			for (int i = 0; i < _options.Count; i++)
			{
				UtilityTypeOption option = _options[i];
				string[] pathParts = option.Path.Split('/');
				string currentPath = string.Empty;
				for (int j = 0; j < pathParts.Length; j++)
				{
					string pathPart = pathParts[j];
					currentPath = string.IsNullOrEmpty(currentPath)
						? pathPart
						: string.Concat(currentPath, "/", pathPart);
					
					int level = j + 1;
					bool isLast = level >= pathParts.Length;
					if (isLast)
					{
						tree.Add(new SearchTreeEntry(GetEntryContent(option))
						{
							level = level,
							userData = option,
						});
						
						continue;
					}
					
					if (pathSet.Add(currentPath))
					{
						tree.Add(new SearchTreeGroupEntry(new GUIContent(pathPart), level));
					}
				}
			}
			
			return tree;
		}
		
		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			var option = searchTreeEntry.userData as UtilityTypeOption;
			Action<Type> onTypeSelected = _onTypeSelected;
			_options = null;
			_onTypeSelected = null;
			onTypeSelected?.Invoke(option?.Type);
			return true;
		}
	}
}
