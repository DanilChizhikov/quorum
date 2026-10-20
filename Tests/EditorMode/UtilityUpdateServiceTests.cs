using System;
using NUnit.Framework;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class UtilityUpdateServiceTests
	{
		private PlayerLoopSystem _originalLoop;

		[SetUp]
		public void Setup()
		{
			_originalLoop = PlayerLoop.GetCurrentPlayerLoop();
		}

		[TearDown]
		public void TearDown()
		{
			PlayerLoop.SetPlayerLoop(_originalLoop);
		}

		[Test]
		public void Instance_InsertsItselfIntoUpdateAndFixedUpdate()
		{
			UtilityUpdateService service = UtilityUpdateService.Instance;
			try
			{
				Assert.IsTrue(ContainsService(typeof(Update)), "Update phase does not contain the service");
				Assert.IsTrue(ContainsService(typeof(FixedUpdate)), "FixedUpdate phase does not contain the service");
			}
			finally
			{
				service.Dispose();
			}
		}

		[Test]
		public void Dispose_RemovesItselfFromPlayerLoop()
		{
			UtilityUpdateService.Instance.Dispose();

			Assert.IsFalse(ContainsService(typeof(Update)));
			Assert.IsFalse(ContainsService(typeof(FixedUpdate)));
		}

		private static bool ContainsService(Type phaseType)
		{
			PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
			for (int i = 0; i < loop.subSystemList.Length; i++)
			{
				PlayerLoopSystem phase = loop.subSystemList[i];
				if (phase.type != phaseType || phase.subSystemList == null)
				{
					continue;
				}

				for (int j = 0; j < phase.subSystemList.Length; j++)
				{
					if (phase.subSystemList[j].type == typeof(UtilityUpdateService))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
