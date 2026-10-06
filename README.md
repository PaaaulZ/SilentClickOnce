# SilentClickOnce
[![GitHub license](https://img.shields.io/github/license/PaaaulZ/SilentClickOnce)](https://github.com/PaaaulZ/SilentClickOnce/blob/master/LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/PaaaulZ/SilentClickOnce)](https://github.com/PaaaulZ/SilentClickOnce/releases)
[![GitHub stars](https://img.shields.io/github/stars/PaaaulZ/SilentClickOnce)](https://github.com/PaaaulZ/SilentClickOnce/stargazers)

Silently install and uninstall ClickOnce applications from the command line.

Designed for unattended deployments, scripts, Active Directory environments and enterprise software distribution.

## Features

- Silent ClickOnce installation
- Silent ClickOnce uninstallation
- Command-line interface
- Works with `.application` deployment manifests
- Suitable for scripts and unattended deployments
- No user interaction required
- Lightweight standalone executable

## Requirements

- .NET Framework 4.5

## Usage

### Install ClickOnce application

```cmd
SilentClickOnce.exe -i "\\192.168.1.2\apps\MyApp\MyApp.application" > MyApp.log
```

### Uninstall ClickOnce application

```cmd
SilentClickOnce.exe -u MyApp > MyApp.log
```

## How it works

ClickOnce normally expects user interaction during installation and removal.

SilentClickOnce provides a small command-line wrapper around the ClickOnce deployment APIs, making it possible to perform these operations from scripts and automated deployment systems.

Typical use cases include:

- Enterprise software deployment
- Active Directory / domain environments
- Login scripts
- Software distribution systems
- Automated installation and removal

## References

[Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019)
