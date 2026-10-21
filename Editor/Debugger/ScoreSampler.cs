using System;
using System.Collections.Generic;
using DTech.Quorum.Diagnostics;
using UnityEditor;

namespace DTech.Quorum.Editor
{
	internal sealed class ScoreSampler
	{
		private const double DefaultInterval = 0.1d;
		private const string MissingVariablesPrefix = "missing: ";

		private readonly List<float> _scores = new();
		private readonly List<string> _errors = new();

		public bool IsPaused { get; set; }
		public double Interval { get; set; } = DefaultInterval;
		public int Count => _scores.Count;

		private double _lastSampleTime;

		public static float Evaluate(IQuorumAction action, IQuorumContext context, out string error)
		{
			error = null;
			if (action == null)
			{
				return 0f;
			}

			try
			{
				return action.CalculateUtility(context);
			}
			catch (Exception exception)
			{
				error = exception.Message;
				return float.NaN;
			}
		}

		public static float Evaluate(Consideration consideration, IQuorumContext context, out string error)
		{
			error = null;
			if (consideration == null)
			{
				return 0f;
			}

			float score;
			try
			{
				score = consideration.Evaluate(context);
			}
			catch (Exception exception)
			{
				error = exception.Message;
				return float.NaN;
			}

			error = GetUnresolvedVariablesMessage(consideration);
			return score;
		}

		public bool Sample(IBrainDebugInfo brain, bool force)
		{
			if (brain == null)
			{
				Clear();
				return false;
			}

			double time = EditorApplication.timeSinceStartup;
			if (!force && (IsPaused || time - _lastSampleTime < Interval))
			{
				return false;
			}

			_lastSampleTime = time;
			_scores.Clear();
			_errors.Clear();
			IReadOnlyList<IQuorumAction> actions = brain.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				float score = Evaluate(actions[i], brain.Context, out string error);
				_scores.Add(score);
				_errors.Add(error);
			}

			return true;
		}

		public float GetScore(int index) =>
			index >= 0 && index < _scores.Count ? _scores[index] : 0f;

		public string GetError(int index) =>
			index >= 0 && index < _errors.Count ? _errors[index] : null;

		public float GetMaxScore()
		{
			float max = 0f;
			for (int i = 0; i < _scores.Count; i++)
			{
				float score = _scores[i];
				if (float.IsNaN(score) || score <= max)
				{
					continue;
				}

				max = score;
			}

			return max;
		}

		public void Clear()
		{
			_scores.Clear();
			_errors.Clear();
		}

		private static string GetUnresolvedVariablesMessage(Consideration consideration)
		{
			if (consideration is not FormulaConsideration formula)
			{
				return null;
			}

			string unresolved = formula.UnresolvedVariables;
			return string.IsNullOrEmpty(unresolved) ? null : $"{MissingVariablesPrefix}{unresolved}";
		}
	}
}
