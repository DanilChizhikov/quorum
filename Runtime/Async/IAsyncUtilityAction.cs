using System.Threading;
using System.Threading.Tasks;

namespace DTech.UtilityAI
{
	public interface IAsyncUtilityAction
	{
		ValueTask InitializeAsync(IUtilityContext context, CancellationToken token);
		ValueTask EnterAsync(IUtilityContext context, CancellationToken token);
		ValueTask<UtilityActionState> TickAsync(IUtilityContext context, CancellationToken token);
		ValueTask ExitAsync(IUtilityContext context, CancellationToken token);
		ValueTask CancelAsync(IUtilityContext context, CancellationToken token);
	}
}
