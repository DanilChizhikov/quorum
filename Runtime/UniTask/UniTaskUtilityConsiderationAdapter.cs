using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace DTech.UtilityAI.Runtime.UniTask
{
	public sealed class UniTaskUtilityConsiderationAdapter : IAsyncUtilityConsideration
	{
		private readonly IUniTaskUtilityConsideration _consideration;

		public UniTaskUtilityConsiderationAdapter(IUniTaskUtilityConsideration consideration)
		{
			_consideration = consideration ?? throw new ArgumentNullException(nameof(consideration));
		}

		public async ValueTask<float> EvaluateAsync(IUtilityContext context, CancellationToken token)
		{
			return await _consideration.EvaluateAsync(context, token);
		}
	}

	public sealed class AsyncUtilityConsiderationUniTaskAdapter : IUniTaskUtilityConsideration
	{
		private readonly IAsyncUtilityConsideration _consideration;

		public AsyncUtilityConsiderationUniTaskAdapter(IAsyncUtilityConsideration consideration)
		{
			_consideration = consideration ?? throw new ArgumentNullException(nameof(consideration));
		}

		public async UniTask<float> EvaluateAsync(IUtilityContext context, CancellationToken token)
		{
			return await _consideration.EvaluateAsync(context, token);
		}
	}
}
