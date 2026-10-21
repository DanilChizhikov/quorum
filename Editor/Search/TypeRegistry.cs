using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal static class TypeRegistry
	{
		private static readonly Dictionary<Type, List<TypeOption>> _options = new();
		
		public static IReadOnlyList<TypeOption> GetOptions(Type baseType)
		{
			if (_options.TryGetValue(baseType, out List<TypeOption> cachedOptions))
			{
				return cachedOptions;
			}

			var options = new List<TypeOption>();
			var skippedTypes = new List<Type>();
			TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(baseType);
			foreach (Type type in types)
			{
				if (!IsInstantiable(type))
				{
					continue;
				}
				
				if (type.GetCustomAttribute<SerializableAttribute>(false) == null)
				{
					skippedTypes.Add(type);
					continue;
				}
				
				options.Add(CreateOption(type));
			}
			
			options.Sort(CompareOptions);
			_options[baseType] = options;
			LogSkippedTypes(baseType, skippedTypes);
			return options;
		}
		
		public static string GetDisplayName(Type type)
		{
			var menuAttribute = type.GetCustomAttribute<QuorumMenuAttribute>(false);
			return menuAttribute != null && !string.IsNullOrEmpty(menuAttribute.Path)
				? System.IO.Path.GetFileName(menuAttribute.Path)
				: ObjectNames.NicifyVariableName(type.Name);
		}
		
		[InitializeOnLoadMethod]
		private static void ResetCache()
		{
			_options.Clear();
		}
		
		private static bool IsInstantiable(Type type) =>
			!type.IsAbstract &&
			!type.IsInterface &&
			!type.IsGenericTypeDefinition &&
			type.IsVisible &&
			type.GetConstructor(Type.EmptyTypes) != null;
		
		private static TypeOption CreateOption(Type type)
		{
			var menuAttribute = type.GetCustomAttribute<QuorumMenuAttribute>(false);
			bool hasMenuPath = menuAttribute != null && !string.IsNullOrEmpty(menuAttribute.Path);
			string path = hasMenuPath ? menuAttribute.Path : GetDefaultPath(type);
			return new TypeOption(type, path, menuAttribute?.IconName);
		}
		
		private static string GetDefaultPath(Type type)
		{
			string displayName = ObjectNames.NicifyVariableName(type.Name);
			if (string.IsNullOrEmpty(type.Namespace))
			{
				return displayName;
			}
			
			return string.Concat(type.Namespace.Replace('.', '/'), "/", displayName);
		}
		
		private static int CompareOptions(TypeOption first, TypeOption second) =>
			string.Compare(first.Path, second.Path, StringComparison.OrdinalIgnoreCase);
		
		private static void LogSkippedTypes(Type baseType, List<Type> skippedTypes)
		{
			HiddenTypeReport.Report(baseType, skippedTypes);
			if (skippedTypes.Count <= 0)
			{
				return;
			}
			
			Debug.LogWarning($"[Quorum] {skippedTypes.Count} type(s) derived from {baseType.Name} are hidden in the type picker because they are missing the [Serializable] attribute. Open {HiddenTypesWindow.MenuPath} to review and fix them.");
		}
	}
}
