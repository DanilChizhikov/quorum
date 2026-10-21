using NUnit.Framework;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class EnumTests
	{
		[Test]
		public void UtilityActionState_Running_HasCorrectValue()
		{
			Assert.That((int)UtilityActionState.Running, Is.EqualTo(0));
		}
		
		[Test]
		public void UtilityActionState_Completed_HasCorrectValue()
		{
			Assert.That((int)UtilityActionState.Completed, Is.EqualTo(1));
		}
		
		[Test]
		public void UtilityActionState_Failed_HasCorrectValue()
		{
			Assert.That((int)UtilityActionState.Failed, Is.EqualTo(2));
		}
		
		[Test]
		public void BrainUpdateType_Update_HasCorrectValue()
		{
			Assert.That((byte)BrainUpdateType.Update, Is.EqualTo(0));
		}
		
		[Test]
		public void BrainUpdateType_FixedUpdate_HasCorrectValue()
		{
			Assert.That((byte)BrainUpdateType.FixedUpdate, Is.EqualTo(1));
		}
		
		[Test]
		public void BrainUpdateType_Custom_HasCorrectValue()
		{
			Assert.That((byte)BrainUpdateType.Custom, Is.EqualTo(2));
		}
		
		[Test]
		public void SwitchActionType_OnActionFinished_HasCorrectValue()
		{
			Assert.That((byte)SwitchActionType.OnActionFinished, Is.EqualTo(0));
		}
		
		[Test]
		public void SwitchActionType_OnlyRequest_HasCorrectValue()
		{
			Assert.That((byte)SwitchActionType.OnlyRequest, Is.EqualTo(1));
		}
		
		[Test]
		public void UtilityActionState_CanBeUsedInSwitchStatement()
		{
			UtilityActionState state = UtilityActionState.Running;
			int result = -1;
			
			switch (state)
			{
				case UtilityActionState.Running:
					result = 0;
					break;
				case UtilityActionState.Completed:
					result = 1;
					break;
				case UtilityActionState.Failed:
					result = 2;
					break;
			}
			
			Assert.That(result, Is.EqualTo(0));
		}
		
		[Test]
		public void BrainUpdateType_CanBeUsedInSwitchStatement()
		{
			BrainUpdateType type = BrainUpdateType.FixedUpdate;
			int result = -1;
			
			switch (type)
			{
				case BrainUpdateType.Update:
					result = 0;
					break;
				case BrainUpdateType.FixedUpdate:
					result = 1;
					break;
				case BrainUpdateType.Custom:
					result = 2;
					break;
			}
			
			Assert.That(result, Is.EqualTo(1));
		}
		
		[Test]
		public void UtilityActionState_AllValues_AreUnique()
		{
			var values = new[]
			{
				(int)UtilityActionState.Running,
				(int)UtilityActionState.Completed,
				(int)UtilityActionState.Failed
			};
			
			var uniqueValues = new System.Collections.Generic.HashSet<int>(values);
			
			Assert.That(uniqueValues.Count, Is.EqualTo(values.Length));
		}
		
		[Test]
		public void BrainUpdateType_AllValues_AreUnique()
		{
			var values = new[]
			{
				(byte)BrainUpdateType.Update,
				(byte)BrainUpdateType.FixedUpdate,
				(byte)BrainUpdateType.Custom
			};
			
			var uniqueValues = new System.Collections.Generic.HashSet<byte>(values);
			
			Assert.That(uniqueValues.Count, Is.EqualTo(values.Length));
		}
	}
}
