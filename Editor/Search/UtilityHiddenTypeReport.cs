using System;
using System.Collections.Generic;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityHiddenTypeReport
	{
		public static event Action Changed;

		private static readonly Dictionary<Type, List<Type>> _hiddenTypes = new();

		public static int Count { get; private set; }

		public static void Report(Type baseType, IReadOnlyList<Type> hiddenTypes)
		{
			if (baseType == null || hiddenTypes == null)
			{
				return;
			}

			if (!_hiddenTypes.TryGetValue(baseType, out List<Type> types))
			{
				types = new List<Type>();
				_hiddenTypes[baseType] = types;
			}

			types.Clear();
			types.AddRange(hiddenTypes);
			Count = CountTypes();
			Changed?.Invoke();
		}

		public static IReadOnlyDictionary<Type, List<Type>> GetHiddenTypes() => _hiddenTypes;

		public static void Clear()
		{
			if (_hiddenTypes.Count <= 0)
			{
				return;
			}

			_hiddenTypes.Clear();
			Count = 0;
			Changed?.Invoke();
		}

		[InitializeOnLoadMethod]
		private static void ResetReport()
		{
			_hiddenTypes.Clear();
			Count = 0;
		}

		private static int CountTypes()
		{
			int count = 0;
			foreach (KeyValuePair<Type, List<Type>> pair in _hiddenTypes)
			{
				count += pair.Value.Count;
			}

			return count;
		}
	}
}
