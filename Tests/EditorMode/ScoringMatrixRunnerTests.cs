using System.Collections.Generic;
using DTech.Quorum.Editor;
using NUnit.Framework;
using UnityEngine;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class ScoringMatrixRunnerTests
	{
		private const string ExpectedActionType = "TestQuorumAction";

		private TestBrainConfig _config;
		private TestScoringProbe _probe;

		[SetUp]
		public void SetUp()
		{
			_config = ScriptableObject.CreateInstance<TestBrainConfig>();
			_probe = ScriptableObject.CreateInstance<TestScoringProbe>();
			_config.SetActions(new QuorumAction[]
			{
				new TestQuorumAction(0.75f),
				new TestScorelessQuorumAction(),
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
			var cases = new List<ScoringCase>
			{
				new("close target", null, ExpectedActionType),
			};

			ScoringMatrix matrix = ScoringMatrixRunner.Run(_config, _probe, cases);

			Assert.IsNull(matrix.Error);
			Assert.AreEqual(2, matrix.ActionNames.Count);
			Assert.AreEqual(1, matrix.Rows.Count);

			ScoringRow row = matrix.Rows[0];
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
			var cases = new List<ScoringCase>
			{
				new("wrong expectation", null, nameof(TestScorelessQuorumAction)),
			};

			ScoringMatrix matrix = ScoringMatrixRunner.Run(_config, _probe, cases);

			Assert.AreEqual(1, matrix.Rows.Count);
			Assert.IsTrue(matrix.Rows[0].HasMismatch);
		}

		[Test]
		public void Run_DoesNotLeakConfigInstances()
		{
			int before = CountConfigs();
			var cases = new List<ScoringCase>
			{
				new("first", null, null),
				new("second", null, null),
			};

			ScoringMatrixRunner.Run(_config, _probe, cases);

			Assert.AreEqual(before, CountConfigs());
		}

		[Test]
		public void Run_WithoutProbe_ReportsError()
		{
			ScoringMatrix matrix = ScoringMatrixRunner.Run(_config, null, null);

			Assert.IsNotNull(matrix.Error);
			Assert.AreEqual(0, matrix.Rows.Count);
		}

		private static int CountConfigs() => Resources.FindObjectsOfTypeAll<BrainConfig>().Length;
	}
}
