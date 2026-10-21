using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace DTech.UtilityAI
{
	internal sealed class UtilityUpdateService : IDisposable
	{
		private static readonly object _lock = new();
		
		private readonly List<Action> _updateActions = new();
		private readonly List<Action> _fixedUpdateActions = new();
		private readonly HashSet<Action> _registeredUpdateActions = new();
		private readonly HashSet<Action> _registeredFixedUpdateActions = new();
		private readonly Action<Action> _invokeAction;

		public static UtilityUpdateService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						_instance = new UtilityUpdateService();
					}
				}
				
				return _instance;
			}
		}
		
		private static UtilityUpdateService _instance;

		public UtilityUpdateService()
		{
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<Update>();
				loopSystem.AddSystem(typeof(UtilityUpdateService), Update);
			});
			
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<FixedUpdate>();
				loopSystem.AddSystem(typeof(UtilityUpdateService), FixedUpdate);
			});
			
			_invokeAction = Application.isEditor ? InvokeEditorAction : InvokeAction;
		}
		
		~UtilityUpdateService()
		{
			Dispose();
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
				loopSystem.RemoveSystem(typeof(UtilityUpdateService), false);
			});
			
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				ref PlayerLoopSystem loopSystem = ref system.GetSystem<FixedUpdate>();
				loopSystem.RemoveSystem(typeof(UtilityUpdateService), false);
			});

			lock (_lock)
			{
				_instance = null;
			}

			GC.SuppressFinalize(this);
		}
		
		private void Update()
		{
			List<Action> actions = ListPool<Action>.Get();
			actions.Clear();
			actions.AddRange(_updateActions);
			for (int i = 0; i < actions.Count; i++)
			{
				Action updateAction = actions[i];
				_invokeAction.Invoke(updateAction);
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
				_invokeAction.Invoke(fixedUpdateAction);
			}
			
			actions.Clear();
			ListPool<Action>.Release(actions);
		}
		
		private void InvokeAction(Action action)
		{
			action();
		}
		
		private void InvokeEditorAction(Action action)
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
	}
}