# Utility AI
[![Unity Version](https://img.shields.io/badge/unity-6000.0+-000.svg)](https://unity3d.com/get-unity/download/archive)
![Unity Tests](https://github.com/DanilChizhikov/utilityai/actions/workflows/tests.yml/badge.svg?branch=master)

## Overview

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

## Usage

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.