using System.Threading;
using Cysharp.Threading.Tasks;

namespace DTech.UtilityAI.Runtime.UniTask
{
	public interface IUniTaskUtilityConsideration
	{
		UniTask<float> EvaluateAsync(IUtilityContext context, CancellationToken token);
	}
}
