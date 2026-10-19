# Changelog

## [1.0.0]
### Added
- `UtilityAction`: plain C# action base stored in `BrainConfig` with `[SerializeReference]`, plus the typed `UtilityAction<TAgent, TContext>` variant.
- `ScriptableUtilityActionReference`: config entry that routes a brain to an existing `ScriptableUtilityAction` asset.
- `IUtilityActionInjector` and the optional `com.dtech.utilityai.zenject` / `com.dtech.utilityai.vcontainer` assemblies (Zenject, VContainer).
- `IUtilityAction.Initialize(IUtilityContext)`, called once per action after injection.
- Inspector type picker for actions, considerations and aggregators, `[UtilityMenu]` for grouping, and an upgrade button for configs with legacy `ScriptableUtilityAction` arrays.
- `CompositeUtilityConsideration` with `ConsiderationCombineMode` (multiply, min, max, average, sum).
- `IBrainUpdateProvider.RemoveUpdate`.

### Changed
- `UtilityBrain` takes the context in its constructor (`UtilityBrain(config, context, injector)`); before, the context was never assigned.
- A brain clones its config (`Object.Instantiate`) and destroys the clone on `Dispose`, so action state is per brain instead of shared through the asset.
- `RandomUtilityScoreAggregator` clamps negative utilities to zero and returns `-1` when the total utility is zero.

### Fixed
- `Disable` now exits the current action, unregisters the update and resets `IsEnabled`.
- `TryGetAction<T>`/`GetAction<T>` matched types in the wrong direction and missed base types and interfaces.
- A missing score aggregator logs an error instead of throwing a `NullReferenceException`.

### Breaking
- `UtilityConsideration` and `UtilityScoreAggregator` are plain `[Serializable]` classes, not `ScriptableObject`s; existing consideration and aggregator assets must be recreated as config entries.
- `BrainConfig.Actions` is `IReadOnlyList<UtilityAction>`; old `ScriptableUtilityAction` arrays are kept for migration and upgraded from the inspector.
