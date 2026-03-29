using System.Threading;
using System.Threading.Tasks;

namespace DTech.UtilityAI
{
	public interface IAsyncUtilityConsideration
	{
		ValueTask<float> EvaluateAsync(IUtilityContext context, CancellationToken token);
	}
}
