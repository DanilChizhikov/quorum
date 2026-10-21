using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.TestTools;

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

		[Test]
		public void Instance_AfterDispose_CreatesNewService()
		{
			UtilityUpdateService first = UtilityUpdateService.Instance;
			first.Dispose();
			UtilityUpdateService second = UtilityUpdateService.Instance;
			try
			{
				Assert.AreNotSame(first, second);
			}
			finally
			{
				second.Dispose();
			}
		}

		[Test]
		public void Update_WhenActionThrows_LogsItAndTicksTheOthers()
		{
			UtilityUpdateService service = UtilityUpdateService.Instance;
			try
			{
				int ticks = 0;
				service.RegisterUpdate(() => throw new InvalidOperationException("update action failed"));
				service.RegisterUpdate(() => ticks++);
				PlayerLoopSystem.UpdateFunction dispatcher = GetDispatcher(typeof(Update));
				Assert.IsNotNull(dispatcher, "Update phase does not contain the service");
				LogAssert.Expect(LogType.Exception, new Regex("update action failed"));

				dispatcher();

				Assert.AreEqual(1, ticks, "An action that threw stopped the rest of the tick");
			}
			finally
			{
				service.Dispose();
			}
		}

		private static PlayerLoopSystem.UpdateFunction GetDispatcher(Type phaseType)
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
						return phase.subSystemList[j].updateDelegate;
					}
				}
			}

			return null;
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
