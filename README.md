# Utility AI
[![Unity Version](https://img.shields.io/badge/unity-6000.0+-000.svg)](https://unity3d.com/get-unity/download/archive)
![Unity Tests](https://github.com/DanilChizhikov/utilityai/actions/workflows/tests.yml/badge.svg?branch=master)

## Overview
Utility AI picks the action with the best score every time a brain is asked to switch. Actions are
configured in a `BrainConfig` asset and can be written either as plain C# classes (`UtilityAction`,
stored with `[SerializeReference]`) or as `ScriptableObject` assets (`ScriptableUtilityAction`).
Plain actions are cloned per brain, so every agent owns its action state, and they receive their
dependencies from any DI container - Zenject and VContainer adapters ship with the package.

## Table of Contents
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Manual Installation](#manual-installation)
    - [UPM Installation](#upm-installation)
- [Features](#features)
- [Usage](#usage)
- [Editor Tools](#editor-tools)
    - [Config editing](#config-editing)
    - [Validator](#validator)
    - [Renamed and moved types](#renamed-and-moved-types)
    - [Hidden types](#hidden-types)
    - [Debugger](#debugger)
    - [Profiler](#profiler)
    - [Brain Map](#brain-map)
    - [Scoring Matrix](#scoring-matrix)
- [Samples](#samples)
- [Benchmarks](#benchmarks)
- [License](#license)

## Getting Started

### Prerequisites
- [GIT](https://git-scm.com/downloads)
- [Unity](https://unity.com/releases/editor/archive) 6000.0+

### Manual Installation
1. Download the .unitypackage from the [releases](https://github.com/DanilChizhikov/utilityai/releases/) page.
2. Import com.dtech.utilityai.x.x.x.unitypackage into your project.

### UPM Installation
1. Open the manifest.json file in your project's Packages folder.
2. Add the following line to the dependencies section:
    ```json
    "com.dtech.utilityai": "https://github.com/DanilChizhikov/utilityai.git",
    ```
3. Unity will automatically import the package.

If you want to set a target version, Utility AI uses the `v*.*.*` release tag so you can specify a version like #v1.0.0.

For example `https://github.com/DanilChizhikov/utilityai.git#v1.0.0`.

## Features
- Score based action selection with pluggable aggregators (`MaxUtilityScoreAggregator`, `RandomUtilityScoreAggregator`).
- Actions as plain C# classes: no asset per behaviour, full `[SerializeReference]` support in the config.
- Type picker in the inspector for actions, considerations and aggregators, with grouping via `[UtilityMenu]`.
- One action instance per brain: the config is cloned at construction, so agents never share action state.
- Dependency injection through `IUtilityActionInjector`, with ready adapters for Zenject and VContainer.
- Composable considerations (`CompositeUtilityConsideration`: multiply, min, max, average, sum).
- `ScriptableUtilityAction` assets are referenced from the config through `ScriptableUtilityActionReference`.
- Update driven by the player loop (`Update`, `FixedUpdate`) or by your own code (`BrainUpdateType.Custom`).

## Usage

### Context and agent
```csharp
public sealed class EnemyAgent : IUtilityAgent
{
    public Transform Transform { get; }

    public EnemyAgent(Transform transform) => Transform = transform;
}

public sealed class EnemyContext : UtilityContext<EnemyAgent>
{
    public EnemyContext(EnemyAgent agent, RuntimeBlackboard blackboard) : base(agent, blackboard) { }
}
```

### Action as a plain class
```csharp
[Serializable]
[UtilityMenu("Enemy/Chase Target")]
public sealed class ChaseTargetAction : UtilityAction<EnemyAgent, EnemyContext>
{
    [SerializeField]
    private float _speed = 3f;

    [Inject]
    private IPathfinder _pathfinder;

    protected override void Enter(EnemyContext context) => _pathfinder.Begin(context.Agent.Transform);

    protected override UtilityActionState OnTick(EnemyContext context) =>
        _pathfinder.Move(_speed) ? UtilityActionState.Completed : UtilityActionState.Running;

    protected override float CalculateUtility(EnemyContext context) => 0.5f;

    protected override void Exit(EnemyContext context) => _pathfinder.Stop();
}
```
An action shows up in the type picker when it is `public`, marked `[Serializable]` and has a public
parameterless constructor. Types that are only missing `[Serializable]` are collected in the
[Hidden types](#hidden-types) window.

### Config and brain
Create a `BrainConfig` subclass, make an asset of it, then add actions with the `+` button of the
Actions list and pick the type in the search window. Considerations and the score aggregator are
picked the same way.

```csharp
[CreateAssetMenu(menuName = "Game/Enemy Brain Config")]
public sealed class EnemyBrainConfig : BrainConfig { }

public sealed class EnemyBrain : UtilityBrain<EnemyBrainConfig, EnemyContext>
{
    public EnemyBrain(EnemyBrainConfig config, EnemyContext context, IUtilityActionInjector injector)
        : base(config, context, injector) { }
}
```

The brain clones the config in its constructor, injects every action, calls `Initialize` on it and
destroys the clone on `Dispose`. `Enable` starts the selection, `Disable` exits the current action
and stops the updates, and `RequestSwitchAction(delay)` asks for a new selection.

### Dependency injection
Zenject:
```csharp
public override void InstallBindings()
{
    Container.BindUtilityActionInjector();
}
```

VContainer:
```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterUtilityActionInjector();
}
```

Without an injector the brain works as well - pass `null` and actions keep their serialized data only.

The integration assemblies compile only when their define is set. When the container comes from the
package manager (`com.svermeulen.extenject`, `com.modesttree.zenject`, `jp.hadashikick.vcontainer`)
the define is applied automatically. When Zenject is installed as plain files under `Assets/Plugins`,
add `UTILITYAI_ZENJECT_ENABLED` to Scripting Define Symbols manually (same for
`UTILITYAI_VCONTAINER_ENABLED`).

## Editor Tools

All tooling lives in `com.dtech.utilityai.editor`. The runtime hooks it needs (the live brain registry,
the switch log and the profiler counters) compile only under `UNITY_EDITOR || DEVELOPMENT_BUILD`, so a
release build carries none of it.

### Config editing
The Actions list has a context menu on every entry and on its header: Duplicate, Copy, Paste Values,
Paste As New Entry, Insert Above/Below, Move To Top/Bottom, Remove, Copy All, Clear. Copy and paste
carry the whole `[SerializeReference]` subtree (nested considerations included) through the system
clipboard, so entries move between configs and between projects. Considerations and the score
aggregator have the same Copy/Paste/Clear menu on their header.

### Validator
`Window/DTech/Utility AI/Validator` scans every `BrainConfig` in the project: missing score aggregator,
empty or missing action entries, `ScriptableUtilityActionReference` without an asset, duplicate action
types, empty or single-child composites and actions that can only ever score 0. Issues for the inspected config are also shown inline in its inspector. Fixable ones offer a Fix
button; `Fix All` applies every automatic fix.

### Renamed and moved types
Mark a type with `[UtilityFormerlySerializedType("Old.Namespace.OldName, OldAssembly")]` and press
`Migrate Types` in the validator. It lists the asset files that reference the former name, asks for
confirmation and rewrites them on disk, keeping every serialized field value. Nothing runs
automatically. A missing entry in the inspector also offers `Remap`, which picks a new type for that
slot - the entry's field values are lost there, so prefer the migration for a rename.

### Hidden types
A type derived from `UtilityAction`, `UtilityConsideration` or `UtilityScoreAggregator` without
`[Serializable]` never shows up in the picker. `Window/DTech/Utility AI/Hidden Types` lists those types,
opens their script and can insert the attribute (and `using System;`) after showing what it will write.

### Debugger
`Window/DTech/Utility AI/Debugger` lists the live brains, and for the selected one shows the score of
every action with a bar, the current action and its state, the consideration tree with the score of each
node, and the last switches with their timestamps. Sampling runs at 10 Hz and stops with `Pause`;
an exception from `CalculateUtility` is shown in the cell instead of being logged every frame. In play
mode a `BrainConfig` inspector also lists the live brains built from that asset and opens the debugger
on them.

### Profiler
The `Utility AI` profiler module counts active brains and action switches per frame and times action
selection, which is how a brain that re-selects every frame becomes visible.

### Brain Map
`Window/DTech/Utility AI/Brain Map` draws the selected config as a read-only graph: considerations feed
their action, actions feed the score aggregator. With a live brain for that config the nodes also show
their current score and the current action is highlighted.

### Scoring Matrix
`Window/DTech/Utility AI/Scoring Matrix` scores a config against a set of blackboard snapshots without
entering play mode. Create a `UtilityScoringProbe` subclass that builds your context (and, if the
actions need dependencies, an injector), then a `DTech/Utility AI/Scoring Scenario` asset holding the
probe, the config and the cases. Every case is a `BlackboardAsset` plus the action expected to win.
The result is a matrix of case against action, with the winner highlighted, mismatches in red and CSV
export. `UtilityScoringMatrixRunner.Run` has no UI, so the same cases can be asserted from a test.

The matrix evaluates `CalculateUtility` only: an action whose score depends on state built in `Enter`
is not reproduced. For the same reason `CalculateUtility` must stay free of side effects - the debugger,
the map and the matrix all call it outside of the brain tick.

## Samples

### Village Demo
Import it from **Package Manager > Utility AI > Samples > Village Demo**, then open
`Scenes/VillageDemo.unity` and press Play. Villagers eat, sleep, chop wood and run from wolves;
wolves patrol, hunt and rest. The sample's own `README.md` walks through every file.

What it shows:
- Sensors write into a per-agent `RuntimeBlackboard`; considerations only read it, so the
  `Scoring Matrix` scenario in `Scoring/` reproduces every villager score without a scene.
- Response curves (`BlackboardCurveConsideration`), gates (`BlackboardBoolConsideration`) and weights
  combined with `CompositeUtilityConsideration`.
- Interrupts: the villager re-scores every 0.5 s with `RequestSwitchAction()`; the wolf only switches
  when its action completes or fails, and picks with `RandomUtilityScoreAggregator`.
- `BrainUpdateType.Update` (villager) and `BrainUpdateType.Custom` (the wolf ticks its own brain).
- A custom `IUtilityActionInjector` that hands the world service to the actions.
- A `ScriptableUtilityAction` asset referenced through `ScriptableUtilityActionReference`.
- Commitment (hysteresis): a running `VillageAction` keeps a minimum score, so two close actions do not flip on every re-score.

Open `Window/DTech/Utility AI/Debugger` and `Window/DTech/Utility AI/Brain Map` in play mode to watch
the scores live.

## Benchmarks
`com.dtech.utilityai.performance-tests` (`Tests/Performance`) measures brain creation, the per-frame
tick, `Enable`/`Disable`, scoring and switch requests. The assembly is opt-in:

1. Add `com.unity.test-framework.performance` to your project.
2. Add `UNITY_INCLUDE_PERFORMANCE_TESTS` to the Scripting Define Symbols.

Run it from **Test Runner** and read the numbers in **Window > Analysis > Performance Test Report**.
The assembly builds for every platform, so the same benchmarks run in a player as well. IL2CPP
numbers differ from Editor ones, and an Editor or development build additionally pays for the brain
registry, the switch log and the profiler counters behind `UNITY_EDITOR || DEVELOPMENT_BUILD`.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.