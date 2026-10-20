using System.Collections.Generic;
using DTech.UtilityAI.Editor;
using NUnit.Framework;
using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class UtilityScoringMatrixRunnerTests
	{
		private const string ExpectedActionType = "TestUtilityAction";

		private TestBrainConfig _config;
		private TestScoringProbe _probe;

		[SetUp]
		public void SetUp()
		{
			_config = ScriptableObject.CreateInstance<TestBrainConfig>();
			_probe = ScriptableObject.CreateInstance<TestScoringProbe>();
			_config.SetActions(new UtilityAction[]
			{
				new TestUtilityAction(0.75f),
				new TestScorelessUtilityAction(),
			});
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_config);
			Object.DestroyImmediate(_probe);
		}

		[Test]
		public void Run_ScoresEveryActionAndPicksWinner()
		{
			var cases = new List<UtilityScoringCase>
			{
				new("close target", null, ExpectedActionType),
			};

			UtilityScoringMatrix matrix = UtilityScoringMatrixRunner.Run(_config, _probe, cases);

			Assert.IsNull(matrix.Error);
			Assert.AreEqual(2, matrix.ActionNames.Count);
			Assert.AreEqual(1, matrix.Rows.Count);

			UtilityScoringRow row = matrix.Rows[0];
			Assert.AreEqual("close target", row.Label);
			Assert.AreEqual(0.75f, row.Scores[0]);
			Assert.AreEqual(0f, row.Scores[1]);
			Assert.AreEqual(0, row.WinnerIndex);
			Assert.AreEqual(0, row.ExpectedIndex);
			Assert.IsFalse(row.HasMismatch);
		}

		[Test]
		public void Run_ExpectedMismatch_IsReported()
		{
			var cases = new List<UtilityScoringCase>
			{
				new("wrong expectation", null, nameof(TestScorelessUtilityAction)),
			};

			UtilityScoringMatrix matrix = UtilityScoringMatrixRunner.Run(_config, _probe, cases);

			Assert.AreEqual(1, matrix.Rows.Count);
			Assert.IsTrue(matrix.Rows[0].HasMismatch);
		}

		[Test]
		public void Run_DoesNotLeakRuntimeConfigClones()
		{
			int before = CountConfigs();
			var cases = new List<UtilityScoringCase>
			{
				new("first", null, null),
				new("second", null, null),
			};

			UtilityScoringMatrixRunner.Run(_config, _probe, cases);

			Assert.AreEqual(before, CountConfigs());
		}

		[Test]
		public void Run_WithoutProbe_ReportsError()
		{
			UtilityScoringMatrix matrix = UtilityScoringMatrixRunner.Run(_config, null, null);

			Assert.IsNotNull(matrix.Error);
			Assert.AreEqual(0, matrix.Rows.Count);
		}

		private static int CountConfigs() => Resources.FindObjectsOfTypeAll<BrainConfig>().Length;
	}
}
