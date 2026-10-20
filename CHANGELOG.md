# Changelog

## [1.0.0]
### Added
- `UtilityAction`: plain C# action base stored in `BrainConfig` with `[SerializeReference]`, plus the typed `UtilityAction<TAgent, TContext>` variant.
- `ScriptableUtilityActionReference`: config entry that routes a brain to an existing `ScriptableUtilityAction` asset.
- `UtilityConsideration` and `UtilityScoreAggregator` as plain `[Serializable]` config entries; `CompositeUtilityConsideration` with `ConsiderationCombineMode` (multiply, min, max, average, sum).
- `MaxUtilityScoreAggregator` and `RandomUtilityScoreAggregator`; the random one clamps negative utilities to zero and returns `-1` when the total utility is zero.
- `UtilityBrain(config, context, injector)`: the brain clones its config (`Object.Instantiate`) and destroys the clone on `Dispose`, so action state is per brain instead of shared through the asset. `Disable` exits the current action and unregisters the update; a missing score aggregator logs an error.
- An action that completed or failed and wins the next selection again gets `Exit` and `Enter` again; a running action that wins a periodic re-score is left alone.
- `TryGetAction<T>`/`GetAction<T>` match base types and interfaces.
- `IUtilityActionInjector` and the optional `com.dtech.utilityai.zenject` / `com.dtech.utilityai.vcontainer` assemblies (Zenject, VContainer).
- `IUtilityAction.Initialize(IUtilityContext)`, called once per action after injection. `IUtilityAction.CalculateUtility` must be side-effect free: editor tools call it outside of the brain tick.
- `BrainUpdateType.Update`/`FixedUpdate` driven by the player loop, `BrainUpdateType.Custom` for brains ticked by your own code, and `IBrainUpdateProvider.RemoveUpdate`.
- Inspector type picker for actions, considerations and aggregators with `[UtilityMenu]` for grouping; types missing `[Serializable]` are collected in the Hidden Types window with a quick fix.
- Context menu on the Actions list and on consideration/aggregator headers: duplicate, copy, paste (whole `[SerializeReference]` subtree, across configs and projects), insert, move, remove, copy all, clear.
- `BrainConfig` validator window with inline issues in the inspector and automatic fixes.
- `UtilityFormerlySerializedTypeAttribute` plus a manual, confirmation-gated migration that rewrites asset files and keeps field values after a type rename or move; `Remap` button for a missing entry.
- Live brain debugger, `Utility AI` profiler module and a read-only Brain Map graph; the `BrainConfig` inspector lists live brains in play mode.
- `UtilityScoringProbe` and `UtilityScoringScenario`: score a config against `BlackboardAsset` snapshots from the Scoring Matrix window or from a test through `UtilityScoringMatrixRunner`.
- Diagnostics hooks on `UtilityBrain` (live brain registry, switch log, profiler counters), compiled only under `UNITY_EDITOR || DEVELOPMENT_BUILD`.
