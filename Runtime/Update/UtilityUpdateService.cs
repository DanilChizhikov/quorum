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
				PlayerLoopSystem loopSystem = system.GetSystem<Update>();
				loopSystem.AddSystem(typeof(UtilityUpdateService), Update);
			});
			
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				PlayerLoopSystem loopSystem = system.GetSystem<FixedUpdate>();
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
			if (!_updateActions.Contains(updateAction))
			{
				_updateActions.Add(updateAction);
			}
		}
		
		public void RegisterFixUpdate(Action updateAction)
		{
			if (!_fixedUpdateActions.Contains(updateAction))
			{
				_fixedUpdateActions.Add(updateAction);
			}
		}

		public void UnregisterUpdate(Action updateAction)
		{
			_updateActions.Remove(updateAction);
		}
		
		public void UnregisterFixUpdate(Action updateAction)
		{
			_fixedUpdateActions.Remove(updateAction);
		}
		
		public void Dispose()
		{
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				PlayerLoopSystem loopSystem = system.GetSystem<Update>();
				loopSystem.RemoveSystem(typeof(UtilityUpdateService), false);
			});
			
			PlayerLoopExtensions.ModifyPlayerLoop((ref PlayerLoopSystem system) =>
			{
				PlayerLoopSystem loopSystem = system.GetSystem<FixedUpdate>();
				loopSystem.RemoveSystem(typeof(UtilityUpdateService), false);
			});

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