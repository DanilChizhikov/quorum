using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DTech.Quorum.Editor.Update
{
	internal static class GitHubApi
	{
		public const string ReleasesPage = "https://github.com/DanilChizhikov/quorum/releases";

		private const string RepositoryUrl = "https://api.github.com/repos/DanilChizhikov/quorum";
		private const int TimeoutSeconds = 15;

		public static async Task<string> GetAsync(string path, CancellationToken cancellationToken)
		{
			using UnityWebRequest request = UnityWebRequest.Get(RepositoryUrl + path);
			request.timeout = TimeoutSeconds;
			request.SetRequestHeader("Accept", "application/vnd.github+json");
			request.SetRequestHeader("User-Agent", "DTech.Quorum");
			await EditorRequests.SendAsync(request, cancellationToken);
			if (request.result != UnityWebRequest.Result.Success)
			{
				throw new InvalidOperationException($"GET {path} failed: {request.error}");
			}

			return request.downloadHandler.text;
		}

		public static T[] ParseArray<T>(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return Array.Empty<T>();
			}

			JsonArray<T> array = JsonUtility.FromJson<JsonArray<T>>($"{{\"items\":{json}}}");
			return array?.items ?? Array.Empty<T>();
		}
	}
}
