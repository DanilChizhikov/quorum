#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
using System;
using System.Collections.Generic;

namespace DTech.UtilityAI.Diagnostics
{
	internal static class UtilityBrainDebugRegistry
	{
		public const int InvalidToken = -1;

		private static readonly List<WeakReference<IUtilityBrainDebugInfo>> _brains = new();
		private static readonly Stack<int> _freeTokens = new();

		public static bool IsObserved => _observerCount > 0;

		private static int _observerCount;

		public static void AddObserver()
		{
			_observerCount++;
		}

		public static void RemoveObserver()
		{
			if (_observerCount <= 0)
			{
				return;
			}

			_observerCount--;
		}

		public static int Register(IUtilityBrainDebugInfo brain)
		{
			if (brain == null)
			{
				return InvalidToken;
			}

			var reference = new WeakReference<IUtilityBrainDebugInfo>(brain);
			if (_freeTokens.Count > 0)
			{
				int reusedToken = _freeTokens.Pop();
				_brains[reusedToken] = reference;
				return reusedToken;
			}

			_brains.Add(reference);
			return _brains.Count - 1;
		}

		public static void Unregister(int token)
		{
			if (token < 0 || token >= _brains.Count || _brains[token] == null)
			{
				return;
			}

			_brains[token] = null;
			_freeTokens.Push(token);
		}

		public static int GetBrainsNonAlloc(List<IUtilityBrainDebugInfo> buffer)
		{
			if (buffer == null)
			{
				return 0;
			}

			buffer.Clear();
			for (int i = 0; i < _brains.Count; i++)
			{
				WeakReference<IUtilityBrainDebugInfo> reference = _brains[i];
				if (reference == null)
				{
					continue;
				}

				if (!reference.TryGetTarget(out IUtilityBrainDebugInfo brain))
				{
					_brains[i] = null;
					_freeTokens.Push(i);
					continue;
				}

				buffer.Add(brain);
			}

			return buffer.Count;
		}

		public static void Clear()
		{
			_brains.Clear();
			_freeTokens.Clear();
		}

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		private static void Subscribe()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode)
			{
				return;
			}

			Clear();
		}
#endif
	}
}
#endif
