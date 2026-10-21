using System;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Multi-phase action: walk to a tree, chop, carry the log to the stockpile.
	/// When interrupted (by a wolf, by hunger) the carried wood stays with the villager and the next
	/// Work resumes at the delivery phase - per-agent state lives on the cloned action and the agent.
	/// </summary>
	[Serializable]
	[QuorumMenu("Village/Villager/Work")]
	public sealed class WorkAction : VillageAction<VillagerAgent, VillageContext>
	{
		private enum Phase : byte
		{
			GoToTree = 0,
			Chop = 1,
			Deliver = 2,
		}

		[SerializeField]
		private float _speed = 1.6f;

		[SerializeField]
		private float _chopDuration = 2.5f;

		[SerializeField]
		private float _energyCostPerSecond = 0.05f;

		private Phase _phase;
		private VillagePoint _tree;
		private VillagePoint _stockpile;
		private float _chopTimer;

		public WorkAction() : base("Work", new Color(0.75f, 0.5f, 0.25f), 0.3f) { }

		protected override void Enter(VillageContext context)
		{
			Vector3 position = context.Agent.transform.position;
			World.TryGetNearestPoint(VillagePointKind.Tree, position, out _tree);
			World.TryGetNearestPoint(VillagePointKind.Stockpile, position, out _stockpile);
			_chopTimer = 0f;
			_phase = context.Agent.CarriedWood > 0 ? Phase.Deliver : Phase.GoToTree;
		}

		protected override QuorumActionState OnTick(VillageContext context)
		{
			if (_tree == null || _stockpile == null)
			{
				return QuorumActionState.Failed;
			}

			VillagerAgent agent = context.Agent;
			switch (_phase)
			{
				case Phase.GoToTree:
				{
					if (agent.Mover.MoveTo(_tree.Position, _speed, 0.9f))
					{
						_phase = Phase.Chop;
					}
				} break;
				case Phase.Chop:
				{
					_chopTimer += Time.deltaTime;
					agent.Energy -= _energyCostPerSecond * Time.deltaTime;
					agent.Mover.Face(_tree.Position - agent.transform.position);
					if (_chopTimer >= _chopDuration)
					{
						agent.CarriedWood++;
						_phase = Phase.Deliver;
					}
				} break;
				case Phase.Deliver:
				{
					if (agent.Mover.MoveTo(_stockpile.Position, _speed, 0.8f))
					{
						World.AddWood(agent.CarriedWood);
						agent.CarriedWood = 0;
						return QuorumActionState.Completed;
					}
				} break;
			}

			return QuorumActionState.Running;
		}

		protected override void Exit(VillageContext context)
		{
			_tree = null;
			_stockpile = null;
		}
	}
}
