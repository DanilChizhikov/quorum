# Changelog

## [1.0.0]
### Added
- `UtilityAction`: plain C# action base stored in `BrainConfig` with `[SerializeReference]`, plus the typed `UtilityAction<TAgent, TContext>` variant.
- `ScriptableUtilityActionReference`: config entry that routes a brain to an existing `ScriptableUtilityAction` asset.
- `UtilityConsideration` and `UtilityScoreAggregator` as plain `[Serializable]` config entries; `CompositeUtilityConsideration` with `ConsiderationCombineMode` (multiply, min, max, average, sum).
- `MaxUtilityScoreAggregator` and `RandomUtilityScoreAggregator`; the random one clamps negative utilities to zero and returns `-1` when the total utility is zero.
- `UtilityBrain(config, context, injector)`: the config asset is never copied; the brain clones each action with `Clone()` and keeps the clones, so action state is per brain instead of shared through the asset. `Disable` exits the current action and unregisters the update; a missing score aggregator logs an error.
- `Clone()` on `UtilityAction`, `UtilityConsideration` and `UtilityScoreAggregator`: `virtual`, defaults to a shallow field copy (plus the consideration tree for an action, plus the children for a composite). Override it only when a reference field holds per-agent state.
- `UtilityConsideration.Initialize(IUtilityContext)`, called once per brain before the first evaluation, so a consideration can resolve and cache what it reads instead of looking it up in `Evaluate`. Not called for considerations on a `ScriptableUtilityAction` asset, which is shared by every brain.
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
- Diagnostics hooks on `UtilityBrain` (live brain registry, switch log, profiler counters), compiled under `UNITY_EDITOR || UTILITYAI_DIAGNOSTICS`. A player build carries none of it unless you ask for it: measured on Android/IL2CPP the instrumentation costs 890 ns on every action selection and makes `Enable`, `Disable` and `Dispose` 5x to 25x more expensive.
- `com.dtech.utilityai.performance-tests` (`Tests/Performance`): 36 benchmarks over scoring, lifecycle, tick and switch requests, plus four allocation budgets asserted at zero. Opt-in behind `UNITY_INCLUDE_PERFORMANCE_TESTS`; builds for every platform so the numbers can come from a device. Results in the readme.
- `Village Demo` sample (`Samples~/VillageDemo`): two brain types, blackboard sensing, response-curve and gate considerations, a custom `IUtilityActionInjector`, `BrainUpdateType.Custom`, a `ScriptableUtilityAction` entry and a Scoring Matrix scenario.
