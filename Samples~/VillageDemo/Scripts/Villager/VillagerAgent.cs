using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Owns one villager brain. Shows the whole lifecycle:
	/// Awake - build blackboard, context and brain (the brain clones the config and injects every action);
	/// Start/OnEnable - Enable; OnDisable - Disable; OnDestroy - Dispose.
	/// Every frame the agent updates its needs, senses the world into the blackboard and, on a timer,
	/// asks the brain to re-score so a wolf can interrupt whatever the villager is doing.
	/// </summary>
	[RequireComponent(typeof(VillageMover))]
	public sealed class VillagerAgent : MonoBehaviour, IUtilityAgent
	{
		[SerializeField]
		private VillagerBrainConfig _config = default;

		[SerializeField]
		private VillageTint _tint = default;

		[SerializeField]
		private float _hungerPerSecond = 0.02f;

		[SerializeField]
		private float _energyPerSecond = 0.015f;

		[SerializeField, Tooltip("How often the brain re-scores while an action is still running. 0 = only when it completes.")]
		private float _reevaluateInterval = 0.5f;

		[SerializeField]
		private Vector2 _startNeedsRange = new(0.1f, 0.6f);

		private VillageWorld _world;
		private VillageMover _mover;
		private RuntimeBlackboard _blackboard;
		private VillagerBrain _brain;
		private BlackboardVariable<float> _hunger;
		private BlackboardVariable<float> _energy;
		private BlackboardVariable<float> _threatDistance;
		private BlackboardVariable<bool> _foodAvailable;
		private float _reevaluateTimer;
		private bool _isStarted;

		public VillageMover Mover => _mover;
		public IUtilityBrain Brain => _brain;
		public string CurrentActionLabel { get; private set; } = "-";
		public int CarriedWood { get; set; }

		public float Hunger
		{
			get => _hunger.Value;
			set => _hunger.Value = Mathf.Clamp01(value);
		}

		public float Energy
		{
			get => _energy.Value;
			set => _energy.Value = Mathf.Clamp01(value);
		}

		/// <summary>
		/// Called by a wolf that reached this villager: drop the wood and run home.
		/// </summary>
		public void OnCaught()
		{
			CarriedWood = 0;
			if (_world != null && _world.TryGetNearestPoint(VillagePointKind.Bed, transform.position, out VillagePoint bed))
			{
				_mover.Teleport(bed.Position);
			}

			_brain.RequestSwitchAction();
		}

		private void Awake()
		{
			if (_config == null)
			{
				Debug.LogError($"[{name}] Brain config is not assigned.", this);
				enabled = false;
				return;
			}

			_world = GetComponentInParent<VillageWorld>();
			_mover = GetComponent<VillageMover>();
			_blackboard = CreateBlackboard();
			Sense();

			var context = new VillageContext(this, _blackboard);
			_brain = new VillagerBrain(_config, context, new VillageActionInjector(_world));
			_brain.OnActionChanged += HandleActionChanged;
		}

		private void Start()
		{
			_isStarted = true;
			EnableBrain();
		}

		private void OnEnable()
		{
			if (_isStarted)
			{
				EnableBrain();
			}
		}

		private void OnDisable()
		{
			if (_brain == null)
			{
				return;
			}

			_brain.Disable();
			if (_world != null)
			{
				_world.Unregister(this);
			}
		}

		private void OnDestroy()
		{
			if (_brain == null)
			{
				return;
			}

			_brain.OnActionChanged -= HandleActionChanged;
			_brain.Dispose();
			_blackboard.Dispose();
		}

		private void Update()
		{
			Hunger += _hungerPerSecond * Time.deltaTime;
			Energy -= _energyPerSecond * Time.deltaTime;
			Sense();

			if (_reevaluateInterval <= 0f)
			{
				return;
			}

			_reevaluateTimer += Time.deltaTime;
			if (_reevaluateTimer >= _reevaluateInterval)
			{
				_reevaluateTimer = 0f;
				// No delay on purpose: a delayed request stays pending and blocks the switch the brain
				// makes by itself when an action completes. Re-selecting the running action is a no-op.
				_brain.RequestSwitchAction();
			}
		}

		private void EnableBrain()
		{
			if (_brain == null)
			{
				return;
			}

			if (_world != null)
			{
				_world.Register(this);
			}

			_brain.Enable();
		}

		private RuntimeBlackboard CreateBlackboard()
		{
			// Built in code, one per agent. BlackboardAsset.ToRuntimeBlackboard() shares the asset's
			// variable instances, so writing to it in play mode would change the asset for every agent.
			_hunger = new BlackboardVariable<float> { Name = VillageKeys.Hunger, Value = Random.Range(_startNeedsRange.x, _startNeedsRange.y) };
			_energy = new BlackboardVariable<float> { Name = VillageKeys.Energy, Value = 1f - Random.Range(_startNeedsRange.x, _startNeedsRange.y) };
			_threatDistance = new BlackboardVariable<float> { Name = VillageKeys.ThreatDistance, Value = VillageKeys.FarAway };
			_foodAvailable = new BlackboardVariable<bool> { Name = VillageKeys.FoodAvailable };

			var blackboard = new RuntimeBlackboard(4);
			blackboard.Add(_hunger);
			blackboard.Add(_energy);
			blackboard.Add(_threatDistance);
			blackboard.Add(_foodAvailable);
			return blackboard;
		}

		private void Sense()
		{
			if (_world == null)
			{
				return;
			}

			Vector3 position = transform.position;
			_threatDistance.Value = _world.TryGetNearestWolf(position, out WolfAgent wolf)
				? Vector3.Distance(position, wolf.transform.position)
				: VillageKeys.FarAway;
			_foodAvailable.Value = _world.FoodCount > 0;
		}

		private void HandleActionChanged()
		{
			if (!_brain.TryGetCurrentAction(out IUtilityAction action))
			{
				CurrentActionLabel = "-";
				return;
			}

			if (VillageActionNames.TryGetVisual(action, out IVillageActionVisual visual))
			{
				CurrentActionLabel = visual.Label;
				if (_tint != null)
				{
					_tint.SetColor(visual.Tint);
				}
			}
			else
			{
				CurrentActionLabel = action.GetType().Name;
			}
		}
	}
}
