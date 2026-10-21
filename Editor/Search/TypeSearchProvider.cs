using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal sealed class TypeSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		private const string NoneEntryName = "None";
		
		private static TypeSearchProvider _provider;
		
		private string _title;
		private IReadOnlyList<TypeOption> _options;
		private Action<Type> _onTypeSelected;
		private int _generation;
		
		public static TypeSearchProvider Create(string title, IReadOnlyList<TypeOption> options, Action<Type> onTypeSelected)
		{
			if (_provider == null)
			{
				_provider = CreateInstance<TypeSearchProvider>();
				_provider.hideFlags = HideFlags.HideAndDontSave;
			}
			
			_provider._title = title;
			_provider._options = options;
			_provider._onTypeSelected = onTypeSelected;
			_provider._generation++;
			return _provider;
		}
		
		private static GUIContent GetEntryContent(TypeOption option)
		{
			bool hasIcon = !string.IsNullOrEmpty(option.IconName);
			Texture icon = hasIcon ? EditorGUIUtility.IconContent(option.IconName).image : null;
			return hasIcon ? new GUIContent(option.Name, icon) : new GUIContent(option.Name);
		}
		
		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			int generation = _generation;
			var tree = new List<SearchTreeEntry>
			{
				new SearchTreeGroupEntry(new GUIContent(_title), 0),
				new SearchTreeEntry(new GUIContent(NoneEntryName))
				{
					level = 1,
					userData = new Entry(generation, null),
				},
			};
			
			var pathSet = new HashSet<string>();
			for (int i = 0; i < _options.Count; i++)
			{
				TypeOption option = _options[i];
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
							userData = new Entry(generation, option),
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
			if (searchTreeEntry.userData is not Entry entry || entry.Generation != _generation)
			{
				return true;
			}
			
			Action<Type> onTypeSelected = _onTypeSelected;
			_options = null;
			_onTypeSelected = null;
			onTypeSelected?.Invoke(entry.Option?.Type);
			return true;
		}
		
		private void OnDisable()
		{
			_options = null;
			_onTypeSelected = null;
		}
		
		private readonly struct Entry
		{
			public int Generation { get; }
			public TypeOption Option { get; }
			
			public Entry(int generation, TypeOption option)
			{
				Generation = generation;
				Option = option;
			}
		}
	}
}
