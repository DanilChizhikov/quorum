using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace DTech.UtilityAI.Runtime.UniTask
{
	public sealed class UniTaskUtilityActionAdapter : IAsyncUtilityAction
	{
		private readonly IUniTaskUtilityAction _action;

		public UniTaskUtilityActionAdapter(IUniTaskUtilityAction action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public async ValueTask InitializeAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.InitializeAsync(context, token);
		}

		public async ValueTask EnterAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.EnterAsync(context, token);
		}

		public async ValueTask<UtilityActionState> TickAsync(IUtilityContext context, CancellationToken token)
		{
			return await _action.TickAsync(context, token);
		}

		public async ValueTask ExitAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.ExitAsync(context, token);
		}

		public async ValueTask CancelAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.CancelAsync(context, token);
		}
	}

	public sealed class AsyncUtilityActionUniTaskAdapter : IUniTaskUtilityAction
	{
		private readonly IAsyncUtilityAction _action;

		public AsyncUtilityActionUniTaskAdapter(IAsyncUtilityAction action)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		public async Cysharp.Threading.Tasks.UniTask InitializeAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.InitializeAsync(context, token);
		}

		public async Cysharp.Threading.Tasks.UniTask EnterAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.EnterAsync(context, token);
		}

		public async UniTask<UtilityActionState> TickAsync(IUtilityContext context, CancellationToken token)
		{
			return await _action.TickAsync(context, token);
		}

		public async Cysharp.Threading.Tasks.UniTask ExitAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.ExitAsync(context, token);
		}

		public async Cysharp.Threading.Tasks.UniTask CancelAsync(IUtilityContext context, CancellationToken token)
		{
			await _action.CancelAsync(context, token);
		}
	}
}
