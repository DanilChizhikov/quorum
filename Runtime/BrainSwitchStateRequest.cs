using System;
using System.Threading;
using UnityEngine;

namespace DTech.UtilityAI
{
	internal sealed class BrainSwitchStateRequest : IDisposable
	{
		private readonly Action _completeCallback;
		private readonly CancellationToken _token;
		private readonly CancellationTokenSource _tokenSource;
		private readonly float _completeTime;
		
		private bool _isDisposed;

		public BrainSwitchStateRequest(float delay, Action completeCallback, CancellationToken token)
		{
			_token = token;
			_completeCallback = completeCallback;
			_tokenSource = new CancellationTokenSource();
			_completeTime = Time.time + delay;
			UtilityUpdateService.Instance.RegisterUpdate(Update);
		}
		
		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}
			
			UtilityUpdateService.Instance.UnregisterUpdate(Update);
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_isDisposed = true;
		}

		private void Update()
		{
			if (_isDisposed)
			{
				return;
			}

			if (_tokenSource.IsCancellationRequested ||
				_token.IsCancellationRequested ||
				_completeTime <= Time.time)
			{
				_completeCallback?.Invoke();
				Dispose();
			}
		}
	}
}