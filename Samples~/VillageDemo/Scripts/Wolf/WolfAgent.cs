using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum.Samples.Village
{
	/// <summary>
	/// Second brain type. Differences from the villager, on purpose:
	/// the config uses <see cref="BrainUpdateType.Custom"/>, so this component ticks the brain itself
	/// (pause it, slow it down, or tick it from a network loop - your call), and it never re-scores on a
	/// timer: the wolf commits to an action until it completes or fails. The config also uses the
	/// RandomScoreAggregator, so higher scores are only more likely, not certain.
	/// </summary>
	[RequireComponent(typeof(VillageMover))]
	public sealed class WolfAgent : MonoBehaviour, IQuorumAgent
	{
		[SerializeField]
		private WolfBrainConfig _config = default;

		[SerializeField]
		private VillageTint _tint = default;

		[SerializeField]
		private float _satiationPerSecond = 0.04f;

		[SerializeField, Tooltip("Stops ticking the brain. The config uses BrainUpdateType.Custom, so the agent decides when it runs.")]
		private bool _isPaused = false;

		private VillageWorld _world;
		private VillageMover _mover;
		private RuntimeBlackboard _blackboard;
		private WolfBrain _brain;
		private BlackboardVariable<float> _stamina;
		private BlackboardVariable<float> _satiation;
		private BlackboardVariable<float> _preyDistance;
		private bool _isStarted;

		public VillageMover Mover => _mover;
		public IQuorumBrain Brain => _brain;
		public string CurrentActionLabel { get; private set; } = "-";

		public float Stamina
		{
			get => _stamina.Value;
			set => _stamina.Value = Mathf.Clamp01(value);
		}

		public float Satiation
		{
			get => _satiation.Value;
			set => _satiation.Value = Mathf.Clamp01(value);
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

			var context = new WolfContext(this, _blackboard);
			_brain = new WolfBrain(_config, context, new VillageActionInjector(_world));
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
			Satiation -= _satiationPerSecond * Time.deltaTime;
			Sense();
			if (!_isPaused)
			{
				// BrainUpdateType.Custom: nothing ticks this brain unless we do.
				_brain.Update();
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
			_stamina = new BlackboardVariable<float> { Name = VillageKeys.Stamina, Value = 1f };
			_satiation = new BlackboardVariable<float> { Name = VillageKeys.Satiation, Value = 0.5f };
			_preyDistance = new BlackboardVariable<float> { Name = VillageKeys.PreyDistance, Value = VillageKeys.FarAway };

			var blackboard = new RuntimeBlackboard(3);
			blackboard.Add(_stamina);
			blackboard.Add(_satiation);
			blackboard.Add(_preyDistance);
			return blackboard;
		}

		private void Sense()
		{
			if (_world == null)
			{
				return;
			}

			Vector3 position = transform.position;
			_preyDistance.Value = _world.TryGetNearestVillager(position, out VillagerAgent villager)
				? Vector3.Distance(position, villager.transform.position)
				: VillageKeys.FarAway;
		}

		private void HandleActionChanged()
		{
			if (!_brain.TryGetCurrentAction(out IQuorumAction action))
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
