using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace DTech.Quorum
{
	internal sealed class BrainUpdateService : IDisposable
	{
		private static readonly object _lock = new();
		
		private readonly List<Action> _updateActions = new();
		private readonly List<Action> _fixedUpdateActions = new();
		private readonly HashSet<Action> _registeredUpdateActions = new();
		private readonly HashSet<Action> _registeredFixedUpdateActions = new();

		public static BrainUpdateService Instance
		{
			get
			{
				BrainUpdateService instance = _instance;
				if (instance != null)
				{
					return instance;
				}

				lock (_lock)
				{
					return _instance ??= new BrainUpdateService();
				}
			}
		}
		
		private static volatile BrainUpdateService _instance;

		public BrainUpdateService()
		{
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<Update>();
				loopSystem.AddSystem(typeof(BrainUpdateService), Update);
			});
			
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<FixedUpdate>();
				loopSystem.AddSystem(typeof(BrainUpdateService), FixedUpdate);
			});
		}
		
		public void RegisterUpdate(Action updateAction)
		{
			if (_registeredUpdateActions.Add(updateAction))
			{
				_updateActions.Add(updateAction);
			}
		}

		public void RegisterFixUpdate(Action updateAction)
		{
			if (_registeredFixedUpdateActions.Add(updateAction))
			{
				_fixedUpdateActions.Add(updateAction);
			}
		}

		public void UnregisterUpdate(Action updateAction)
		{
			if (_registeredUpdateActions.Remove(updateAction))
			{
				_updateActions.Remove(updateAction);
			}
		}

		public void UnregisterFixUpdate(Action updateAction)
		{
			if (_registeredFixedUpdateActions.Remove(updateAction))
			{
				_fixedUpdateActions.Remove(updateAction);
			}
		}
		
		public void Dispose()
		{
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<Update>();
				loopSystem.RemoveSystem(typeof(BrainUpdateService), false);
			});
			
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<FixedUpdate>();
				loopSystem.RemoveSystem(typeof(BrainUpdateService), false);
			});

			_registeredUpdateActions.Clear();
			_registeredFixedUpdateActions.Clear();
			_updateActions.Clear();
			_fixedUpdateActions.Clear();
			lock (_lock)
			{
				if (ReferenceEquals(_instance, this))
				{
					_instance = null;
				}
			}
		}

		/// <summary>
		/// Drops the service of the previous session. Unity restores the default player loop on its own,
		/// so the loop needs no cleanup here, but with Domain Reload disabled the static instance would
		/// otherwise keep ticking last session's delegates.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetInstance()
		{
			lock (_lock)
			{
				_instance = null;
			}
		}
		
		private void Update()
		{
			List<Action> actions = ListPool<Action>.Get();
			actions.Clear();
			actions.AddRange(_updateActions);
			for (int i = 0; i < actions.Count; i++)
			{
				Action updateAction = actions[i];
				InvokeAction(updateAction);
			}
			
			actions.Clear();
			ListPool<Action>.Release(actions);
		}
		
		private void FixedUpdate()
		{
			List<Action> actions = ListPool<Action>.Get();
			actions.Clear();
			actions.AddRange(_fixedUpdateActions);
			for (int i = 0; i < actions.Count; i++)
			{
				Action fixedUpdateAction = actions[i];
				InvokeAction(fixedUpdateAction);
			}
			
			actions.Clear();
			ListPool<Action>.Release(actions);
		}
		
		/// <summary>
		/// One throwing brain must not abort the tick of every other brain in this frame, in a player
		/// build as much as in the editor.
		/// </summary>
		private static void InvokeAction(Action action)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
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

			_instance?.Dispose();
		}
#endif
	}
}
