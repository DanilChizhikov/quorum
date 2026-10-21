using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class EnumTests
	{
		[Test]
		public void QuorumActionState_Running_HasCorrectValue()
		{
			Assert.That((int)QuorumActionState.Running, Is.EqualTo(0));
		}
		
		[Test]
		public void QuorumActionState_Completed_HasCorrectValue()
		{
			Assert.That((int)QuorumActionState.Completed, Is.EqualTo(1));
		}
		
		[Test]
		public void QuorumActionState_Failed_HasCorrectValue()
		{
			Assert.That((int)QuorumActionState.Failed, Is.EqualTo(2));
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
		public void QuorumActionState_CanBeUsedInSwitchStatement()
		{
			QuorumActionState state = QuorumActionState.Running;
			int result = -1;
			
			switch (state)
			{
				case QuorumActionState.Running:
					result = 0;
					break;
				case QuorumActionState.Completed:
					result = 1;
					break;
				case QuorumActionState.Failed:
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
		public void QuorumActionState_AllValues_AreUnique()
		{
			var values = new[]
			{
				(int)QuorumActionState.Running,
				(int)QuorumActionState.Completed,
				(int)QuorumActionState.Failed
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
