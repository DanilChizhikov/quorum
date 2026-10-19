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
- [License](#license)

## Getting Started

### Prerequisites
- [GIT](https://git-scm.com/downloads)
- [Unity](https://unity.com/releases/editor/archive) 2022.3+

### Manual Installation
1. Download the .unitypackage from the [releases](https://github.com/DanilChizhikov/utilityai/releases/) page.
2. Import com.dtech.utilityai.x.x.x.unitypackage into your project.

### UPM Installation
1. Open the manifest.json file in your project's Packages folder.
2. Add the following line to the dependencies section:
    ```json
    "com.dtech.logging": "https://github.com/DanilChizhikov/utilityai.git",
    ```
3. Unity will automatically import the package.

If you want to set a target version, Logging uses the `v*.*.*` release tag so you can specify a version like #v1.0.0.

For example `https://github.com/DanilChizhikov/utilityai.git#v1.0.0`.

## Features
- Score based action selection with pluggable aggregators (`MaxUtilityScoreAggregator`, `RandomUtilityScoreAggregator`).
- Actions as plain C# classes: no asset per behaviour, full `[SerializeReference]` support in the config.
- Type picker in the inspector for actions, considerations and aggregators, with grouping via `[UtilityMenu]`.
- One action instance per brain: the config is cloned at construction, so agents never share action state.
- Dependency injection through `IUtilityActionInjector`, with ready adapters for Zenject and VContainer.
- Composable considerations (`CompositeUtilityConsideration`: multiply, min, max, average, sum).
- `ScriptableUtilityAction` assets still work and are referenced from the config through `ScriptableUtilityActionReference`.
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
parameterless constructor. Types that are only missing `[Serializable]` are listed in a console
warning when the picker is built.

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

### Migrating existing configs
Configs that stored `ScriptableUtilityAction` assets show an "Upgrade Legacy Actions" button in the
inspector. It wraps every asset into a `ScriptableUtilityActionReference` entry, so the behaviour
stays the same while the config moves to the new list.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.