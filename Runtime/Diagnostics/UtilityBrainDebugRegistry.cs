#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

namespace DTech.UtilityAI.Diagnostics
{
	internal static class UtilityBrainDebugRegistry
	{
		public static event Action Changed;

		private static readonly List<WeakReference<IUtilityBrainDebugInfo>> _brains = new();

		public static void Register(IUtilityBrainDebugInfo brain)
		{
			if (brain == null)
			{
				return;
			}

			_brains.Add(new WeakReference<IUtilityBrainDebugInfo>(brain));
			Changed?.Invoke();
		}

		public static void Unregister(IUtilityBrainDebugInfo brain)
		{
			if (brain == null)
			{
				return;
			}

			for (int i = _brains.Count - 1; i >= 0; i--)
			{
				if (!_brains[i].TryGetTarget(out IUtilityBrainDebugInfo target) || ReferenceEquals(target, brain))
				{
					_brains.RemoveAt(i);
				}
			}

			Changed?.Invoke();
		}

		public static int GetBrainsNonAlloc(List<IUtilityBrainDebugInfo> buffer)
		{
			if (buffer == null)
			{
				return 0;
			}

			buffer.Clear();
			for (int i = _brains.Count - 1; i >= 0; i--)
			{
				if (!_brains[i].TryGetTarget(out IUtilityBrainDebugInfo brain))
				{
					_brains.RemoveAt(i);
					continue;
				}

				buffer.Add(brain);
			}

			buffer.Reverse();
			return buffer.Count;
		}

		public static void Clear()
		{
			if (_brains.Count <= 0)
			{
				return;
			}

			_brains.Clear();
			Changed?.Invoke();
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
