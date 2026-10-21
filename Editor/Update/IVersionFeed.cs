using System;
using System.Threading;
using System.Threading.Tasks;

namespace DTech.Quorum.Editor.Update
{
	internal interface IVersionFeed
	{
		Task<Version> FetchLatestAsync(CancellationToken cancellationToken);
	}
}
