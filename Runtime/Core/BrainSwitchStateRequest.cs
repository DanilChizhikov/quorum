using System;
using System.Threading;
using UnityEngine;

namespace DTech.UtilityAI
{
	internal sealed class BrainSwitchStateRequest
	{
		private readonly Action _completeCallback;
		private readonly Action _cachedUpdate;

		private CancellationToken _token;
		private float _completeTime;
		private bool _isActive;

		public BrainSwitchStateRequest(Action completeCallback)
		{
			_completeCallback = completeCallback;
			_cachedUpdate = Update;
		}

		public void Arm(float delay, CancellationToken token)
		{
			_token = token;
			_completeTime = Time.time + delay;
			if (_isActive)
			{
				return;
			}

			_isActive = true;
			UtilityUpdateService.Instance.RegisterUpdate(_cachedUpdate);
		}

		public void Cancel()
		{
			if (!_isActive)
			{
				return;
			}

			_isActive = false;
			_token = default;
			UtilityUpdateService.Instance.UnregisterUpdate(_cachedUpdate);
		}

		private void Update()
		{
			if (!_isActive)
			{
				return;
			}

			if (!_token.IsCancellationRequested && _completeTime > Time.time)
			{
				return;
			}

			Cancel();
			_completeCallback?.Invoke();
		}
	}
}
