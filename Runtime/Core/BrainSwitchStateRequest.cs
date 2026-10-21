using System;
using System.Threading;
using UnityEngine;

namespace DTech.Quorum
{
	/// <summary>
	/// Delay timer of one pending switch request. The brain that owns it ticks it from its own
	/// update, so the delay runs in the phase the brain runs in and a brain ticked by user code
	/// carries no dependency on the player loop.
	/// </summary>
	internal sealed class BrainSwitchStateRequest
	{
		private readonly Action _completeCallback;
		private readonly Action _cancelCallback;

		private CancellationToken _token;
		private float _completeTime;
		private bool _isActive;

		public bool IsActive => _isActive;

		public BrainSwitchStateRequest(Action completeCallback, Action cancelCallback)
		{
			_completeCallback = completeCallback;
			_cancelCallback = cancelCallback;
		}

		public void Arm(float delay, CancellationToken token)
		{
			_token = token;
			_completeTime = Time.time + delay;
			_isActive = true;
		}

		public void Cancel()
		{
			if (!_isActive)
			{
				return;
			}

			_isActive = false;
			_token = default;
		}

		public void Tick()
		{
			if (!_isActive)
			{
				return;
			}

			if (_token.IsCancellationRequested)
			{
				Cancel();
				_cancelCallback?.Invoke();
				return;
			}

			if (_completeTime > Time.time)
			{
				return;
			}

			Cancel();
			_completeCallback?.Invoke();
		}
	}
}
