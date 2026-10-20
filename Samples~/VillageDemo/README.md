# Village Demo

Villagers eat, sleep, chop wood and run from wolves. Wolves patrol, hunt and rest. Two brain types, one scene, no
input setup and no extra packages.

## Run it
1. Open `Scenes/VillageDemo.unity` and press Play.
2. Open `Window/DTech/Utility AI/Debugger` to see the score of every action live, and
   `Window/DTech/Utility AI/Brain Map` with `Configs/VillagerBrainConfig` selected to see the config as a graph.
3. Use the panel in the top-left corner: spawn villagers, food and wolves, change the time scale.
   Agents are tinted with the colour of their current action; the bars are hunger (orange), energy (blue) and
   wolf stamina (yellow).

## How a decision is made
```
VillagerAgent.Update ──► blackboard (Hunger, Energy, ThreatDistance, FoodAvailable)
                              │
                 considerations read it (curves, gates, constants)
                              │
          MaxUtilityScoreAggregator picks the highest action score
                              │
               UtilityBrain: Exit old action ─► Enter new ─► Tick every frame
```
Agents are the only writers of the blackboard and considerations are the only readers. Because of that the
`Scoring/VillagerScoringScenario` asset reproduces every villager score without a scene: open
`Window/DTech/Utility AI/Scoring Matrix`, pick the scenario and press Run. All five cases should match.

## Files
| File | What to look at |
|---|---|
| `Scripts/Villager/VillagerAgent.cs` | Brain lifecycle (build in `Awake`, `Enable`/`Disable`/`Dispose`), blackboard built in code, sensing, re-scoring every 0.5 s with `RequestSwitchAction()`. |
| `Scripts/Wolf/WolfAgent.cs` | `BrainUpdateType.Custom`: the agent calls `_brain.Update()` itself. No timer re-score - the wolf commits to an action until it completes or fails. |
| `Scripts/Core/VillageAction.cs` | Base of every sample action: HUD label/tint, injected world, commitment floor (hysteresis). |
| `Scripts/Villager/Actions/*` | One action per file. `WorkAction` has three phases and keeps carried wood when interrupted. `EatAction` fails when another villager eats the bush first. |
| `Scripts/Villager/Actions/IdleScriptableAction.cs` | The `ScriptableUtilityAction` flavour, added to the config through a *Scriptable Action Reference* entry. Shared by all brains, so it holds no per-agent state. |
| `Scripts/Considerations/*` | `BlackboardCurveConsideration` (response curve), `BlackboardBoolConsideration` (gate), `ConstantConsideration`. Combined with the package's `CompositeUtilityConsideration`. |
| `Scripts/Core/VillageActionInjector.cs` | A minimal `IUtilityActionInjector`. In a real project use `BindUtilityActionInjector()` (Zenject) or `RegisterUtilityActionInjector()` (VContainer). |
| `Scripts/Diagnostics/VillageScoringProbe.cs` | Builds a context for the Scoring Matrix (no agent, blackboard only). |
| `Configs/*.asset` | The two brain configs and the Idle action asset. Open them to edit actions, curves and weights. |

## Things worth trying
- Flatten the Flee curve in `VillagerBrainConfig` and spawn a wolf: villagers stop running.
- Set the commitment floor of Flee to 0 and watch the Debugger: villagers flip between Flee and Work on the edge
  of the wolf's range.
- Switch the wolf config to `MaxUtilityScoreAggregator`: it becomes predictable and hunts every time.
- Set `Reevaluate Interval` on the Villager prefab to 0: villagers only re-decide when an action ends, so a wolf
  can no longer interrupt them.

## Gotchas this sample works around
- `BlackboardAsset.ToRuntimeBlackboard()` shares the asset's variable instances. Agents build their blackboard in
  code so play mode never writes into an asset and two agents never share needs.
- `RequestSwitchAction(delay)` with a delay stays pending and blocks the switch a Manual brain makes when an action
  completes. The agent uses its own timer and calls `RequestSwitchAction()` without a delay; re-selecting the
  running action is a no-op.
- `CalculateUtility` must stay side-effect free: the Debugger, Brain Map and Scoring Matrix call it outside the
  brain tick. The commitment floor only reads a flag set in `Enter`/`Tick`/`Exit`.
