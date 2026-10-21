using System;
using UnityEngine.LowLevel;

namespace DTech.UtilityAI.Tests.Performance
{
	internal sealed class PerfUpdateServiceScope : IDisposable
	{
		private readonly PlayerLoopSystem _originalLoop;

		private bool _isDisposed;

		public PerfUpdateServiceScope()
		{
			_originalLoop = PlayerLoop.GetCurrentPlayerLoop();
			_ = UtilityUpdateService.Instance;
		}

		public static bool TryGetDispatcher(Type phaseType, out PlayerLoopSystem.UpdateFunction dispatcher)
		{
			dispatcher = null;
			PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
			if (loop.subSystemList == null)
			{
				return false;
			}

			for (int i = 0; i < loop.subSystemList.Length; i++)
			{
				PlayerLoopSystem phase = loop.subSystemList[i];
				if (phase.type != phaseType || phase.subSystemList == null)
				{
					continue;
				}

				for (int j = 0; j < phase.subSystemList.Length; j++)
				{
					PlayerLoopSystem system = phase.subSystemList[j];
					if (system.type != typeof(UtilityUpdateService))
					{
						continue;
					}

					dispatcher = system.updateDelegate;
					return dispatcher != null;
				}
			}

			return false;
		}

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;
			UtilityUpdateService.Instance.Dispose();
			PlayerLoop.SetPlayerLoop(_originalLoop);
		}
	}
}
