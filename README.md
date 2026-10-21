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
    - [Considerations](#considerations)
- [Editor Tools](#editor-tools)
    - [Config editing](#config-editing)
    - [Validator](#validator)
    - [Renamed and moved types](#renamed-and-moved-types)
    - [Hidden types](#hidden-types)
    - [Debugger](#debugger)
    - [Profiler](#profiler)
    - [Diagnostics cost](#diagnostics-cost)
    - [Brain Map](#brain-map)
    - [Scoring Matrix](#scoring-matrix)
- [Samples](#samples)
- [Benchmarks](#benchmarks)
    - [What the suite covers](#what-the-suite-covers)
    - [Results](#results)
    - [Reading the numbers](#reading-the-numbers)
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

### Considerations
A consideration turns world state into a score. `Evaluate` runs for every action on every
re-selection, so it is the hottest code in the package: a blackboard lookup by name measured
**747 ns on Android/IL2CPP**, which is more than the rest of a six-action selection put together.

Resolve everything you can in `Initialize` and keep the reference:

```csharp
public override void Initialize(IUtilityContext context)
{
    context.Blackboard.TryGetVariable(_key, out _variable);
}

public override float Evaluate(IUtilityContext context) =>
    _variable != null ? _curve.Evaluate(_variable.Value) : 0f;
```

The brain calls `Initialize` once per action, on its own cloned consideration tree, so caching
per-agent data there is safe. `CompositeUtilityConsideration` forwards the call to its children.

One exception: a consideration that lives on a `ScriptableUtilityAction` **asset** is shared by
every brain that references it, so it must not cache per-agent data. The brain does not call
`Initialize` on it for that reason - keep such considerations stateless, or move the action into the
config as a plain `[Serializable]` class.

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

The config asset itself is never copied. In its constructor the brain clones every action with
`Clone()` - a shallow field copy plus a copy of the consideration tree - then injects the clone and
calls `Initialize` on it, so two brains built from one config never share mutable state and the asset
stays untouched. `Enable` starts the selection, `Disable` exits the current action and stops the
updates, and `RequestSwitchAction(delay)` asks for a new selection.

`Clone()` is `virtual` on `UtilityAction`, `UtilityConsideration` and `UtilityScoreAggregator`, and
the default covers config data: value fields are duplicated, references to shared assets stay
shared. Override it only when a reference field holds per-agent state that two brains must not
share.

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
the switch log and the profiler counters) compile only under `UNITY_EDITOR || UTILITYAI_DIAGNOSTICS`,
so a player build carries none of it unless you ask - see [Diagnostics cost](#diagnostics-cost).

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

### Diagnostics cost
Everything above - the live brain registry, the switch log and the profiler module - is compiled into
the **Editor only**. A player build, development or not, carries none of it unless you add
`UTILITYAI_DIAGNOSTICS` to its Scripting Define Symbols.

That default is deliberate. Measured on Android/IL2CPP, the instrumentation costs more than the work
it measures: **890 ns on every action selection** (the profiler marker around it), and a brain's
`Enable`, `Disable` and `Dispose` get 5x to 25x more expensive, because each one registers with or
scans the brain registry. Turn it on to look at a player build with the debugger or the profiler
module, and leave it off to profile your game.

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

Brain diagnostics are compiled into the Editor only, so a player run measures the brain without
them. Add `UTILITYAI_DIAGNOSTICS` to the Scripting Define Symbols of the target platform to measure
a player build with them instead - see [Diagnostics cost](#diagnostics-cost) for what that costs.

Run it from **Test Runner** and read the numbers in **Window > Analysis > Performance Test Report**.
The assembly builds for every platform, so the same benchmarks run in a player as well, which is the
only way to get IL2CPP numbers - Editor numbers come from Mono and from a build that always carries
the diagnostics.

### What the suite covers
36 benchmarks in seven fixtures, all over a brain with 6 actions and 3 considerations per action:

- **Scoring** - the aggregator alone, then the full `RequestSwitchAction` path, across four kinds of
  consideration (none, constant, a composite of constants, a composite of blackboard lookups), both
  aggregators, and 1000 brains re-scoring in the same frame.
- **Lifecycle** - creating and disposing brains (inline actions against
  `ScriptableUtilityActionReference`), `Enable`/`Disable` at one and at a thousand brains.
- **Tick** - one brain, a thousand brains, and a thousand through the player loop.
- **Switch requests** - immediate, delayed, and a thousand pending requests in one dispatch.
- **Allocations** - four budgets asserted at zero bytes per call, so a regression fails the run
  instead of showing up as a number nobody reads.
- **Build configuration** - reports whether diagnostics are compiled in, so a result file always
  says which configuration produced it.

### Results
One Android device, IL2CPP, development player build, diagnostics off (the default). Median of three
runs. Per-call values; `GC` is allocations per call.

Scoring one brain, and what each layer adds:

| | time | GC |
|---|---|---|
| `Aggregate` over 6 actions, constant considerations | 357 ns | 0 |
| full selection, no considerations | 361 ns | 0 |
| full selection, composite of 3 constants per action | 494 ns | 0 |
| full selection, composite of 3 **cached** blackboard reads | 860 ns | 0 |
| full selection, composite of 3 blackboard **lookups by name** | 9.62 us | 0 |
| one `RuntimeBlackboard.TryGetVariable` by name | 414 ns | 0 |

A lookup by name costs more than everything else in a selection put together - 18 of them are 94% of
that 9.62 us. Resolve the variable in `Initialize` and the same work takes 860 ns. See
[Considerations](#considerations).

1000 brains re-scoring in one frame: **11.28 ms** with lookups by name, **1.38 ms** with cached
handles. The second one fits a 60 fps frame with room to spare, which is why the package has no
scoring budget or time-slicing: it does not need one.

Lifecycle:

| | time | GC |
|---|---|---|
| create 200 brains, inline actions | 17.28 ms | 43 / brain |
| create 200 brains, `ScriptableUtilityActionReference` | 5.38 ms | 13 / brain |
| dispose 200 brains | 89 us | 0 |
| `Enable` 1000 brains | 588 us | 0 |
| `Disable` 1000 brains | 588 us | 0 |
| `Enable` + `Disable` one brain | 789 ns | 0 |
| `RequestSwitchAction()` | 371 ns | 0 |
| `RequestSwitchAction(delay)`, armed and fired | 1.28 us | 0 |
| action completes and the brain re-selects | 402 ns | 0 |

Creation is the one place that still costs: 86 us and 43 allocations per brain, because every action
and consideration is cloned for that brain. Actions with no per-brain state belong in a
`ScriptableUtilityAction` asset referenced through `ScriptableUtilityActionReference` - the asset is
shared, so the same brain costs 27 us and 13 allocations.

Tick:

| | time | GC |
|---|---|---|
| one brain | 14.7 ns | 0 |
| 1000 brains, called directly | 25.6 us | 0 |
| 1000 brains, through the player loop | 44.4 us | 0 |
| dispatch with 1000 pending delayed requests | 79.5 us | 0 |

A brain that is not re-selecting costs ~25 ns per frame and allocates nothing, so the frame cost of
an idle population is noise. Driving 1000 brains through the package's player-loop hook costs 19 us
per frame more than calling `Update` yourself - use `BrainUpdateType.Custom` if you already have a
tick system and want that back.

### Reading the numbers
These are one device and one configuration. Treat them as the shape of the cost, not as a promise:
the ratios between rows hold, the absolute values do not travel. The Performance Testing package
reports the median of each measurement, so a per-call value is that median divided by
`IterationsPerMeasurement`, while the `Time.GC()` group is already per call.

Measurements under a microsecond are the ones to distrust first. Anything that adds a native call
per iteration - a profiler marker, `AnimationCurve.Evaluate` - also adds variance to it, so compare
pairs of benchmarks inside one run rather than medians across runs.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.