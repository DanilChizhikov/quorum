using System;
using System.Collections.Generic;
using DTech.UtilityAI.Diagnostics;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityScoreSampler
	{
		private const double DefaultInterval = 0.1d;

		private readonly List<float> _scores = new();
		private readonly List<string> _errors = new();

		public bool IsPaused { get; set; }
		public double Interval { get; set; } = DefaultInterval;
		public int Count => _scores.Count;

		private double _lastSampleTime;

		public static float Evaluate(IUtilityAction action, IUtilityContext context, out string error)
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

		public static float Evaluate(UtilityConsideration consideration, IUtilityContext context, out string error)
		{
			error = null;
			if (consideration == null)
			{
				return 0f;
			}

			try
			{
				return consideration.Evaluate(context);
			}
			catch (Exception exception)
			{
				error = exception.Message;
				return float.NaN;
			}
		}

		public bool Sample(IUtilityBrainDebugInfo brain, bool force)
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
			IReadOnlyList<IUtilityAction> actions = brain.Actions;
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
	}
}
