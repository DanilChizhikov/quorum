using System.Threading;
using Cysharp.Threading.Tasks;

namespace DTech.UtilityAI
{
	public interface IUniTaskUtilityAction
	{
		UniTask InitializeAsync(IUtilityContext context, CancellationToken token);
		UniTask EnterAsync(IUtilityContext context, CancellationToken token);
		UniTask<UtilityActionState> TickAsync(IUtilityContext context, CancellationToken token);
		UniTask ExitAsync(IUtilityContext context, CancellationToken token);
		UniTask CancelAsync(IUtilityContext context, CancellationToken token);
	}
}
