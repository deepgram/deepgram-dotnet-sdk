# Development Guide

- [Development Guide](#development-guide)
  - [Welcome](#welcome)
  - [Preparing Your Local Operating System](#preparing-your-local-operating-system)
    - [Setting Up macOS](#setting-up-macos)
    - [Setting Up Windows](#optional-setting-up-windows)
  - [Installing Required Software](#installing-required-software)
    - [Installing on Windows](#installing-on-windows)
    - [Installing on macOS](#installing-on-macos)
    - [Installing on Linux](#installing-on-linux)
  - [Installing Docker](#installing-docker)
  - [GitHub Workflow](#github-workflow)

## Welcome

This document is the canonical source of truth for building and contributing to the [.NET SDK][project].

Please submit an [issue] on GitHub if you:

- Notice a requirement that this doc does not capture.
- Find a different doc that specifies requirements (the doc should instead link here).

## Preparing Your Local Operating System

Where needed, each piece of required software will have separate instructions for Linux, Windows, or macOS.

### Setting Up macOS

Parts of this project assume you are using GNU command line tools; you will need to install those tools on your system. [Follow these directions to install the tools](https://ryanparman.com/posts/2019/using-gnu-command-line-tools-in-macos-instead-of-freebsd-tools/).

In particular, this command installs the necessary packages:

```bash
brew install coreutils ed findutils gawk gnu-sed gnu-tar grep make jq
```

You will want to include this block or something similar at the end of your `.bashrc` or shell init script:

```bash
GNUBINS="$(find `brew --prefix`/opt -type d -follow -name gnubin -print)"

for bindir in ${GNUBINS[@]}
do
  export PATH=$bindir:$PATH
done

export PATH
```

This ensures that the GNU tools are found first in your path. Note that shell init scripts work a little differently for macOS. [This article can help you figure out what changes to make.](https://scriptingosx.com/2017/04/about-bash_profile-and-bashrc-on-macos/)

### (Optional) Setting Up Windows

If you are running Windows, you can contribute to the SDK without requiring Linux-based components like a terminal/shell. Is this step essential? No. Will this help out some time down the road? Yes!

- If you're using Windows 10, Version 2004, Build 19041 or higher, you can use Windows Subsystem for Linux (WSL) to perform various tasks. [Follow these instructions to install WSL2](https://docs.microsoft.com/en-us/windows/wsl/install-win10).

## Installing Required Software

After setting up your operating system, you will be required to install software dependencies required to run examples, perform static checks, linters, execute tests, etc.

We will attempt to provide bootstrapping instructions for various operating systems and development environments; however, we cannot document them all. Therefore, we will select a few common combinations and provide support for them.

### Installing on Windows

It is **HIGHLY** recommended that if you are running windows, that you use [Visual Studio](https://visualstudio.microsoft.com/) (and not [Visual Studio Code](https://code.visualstudio.com/)). This project takes advantage of numerous features only available on Visual Studio.

Setting up the environment with Visual Studio is as easy as installing Visual Studio, opening the [Deepgram.Dev.sln](https://github.com/deepgram/deepgram-dotnet-sdk/blob/main/Deepgram.Dev.sln) solution file and attempting to build all the components. Visual Studio will prompt for any missing dependencies required, download them for you, and take care of any installation required.

This is literally the "easy button" option.

#### (Alterative) Visual Studio Code

If Visual Studio is not an option, cost being a very valid example, an alterative is to use [Visual Studio Code](https://code.visualstudio.com/).

1. You can download and install Visual Studio Code here: [https://code.visualstudio.com/Download](https://code.visualstudio.com/Download).

1. Download and install the latest .NET SDK and Run-time, which can be found at [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

1. To configure Visual Studio Code to work with C#, download and install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) from Microsoft.

**NOTE:** By default, we target the oldest supported .NET in the projects contained within this repo. This is done to ensure we aren't using functionality that is supported at a later version and onward. Using later versions of the .NET SDK is friendlier when it comes to installation. If you use .NET 8, don't forget to update the individual projects to use .NET 8.

### Installing on macOS

### Xcode

Some build tools were installed when you prepared your system with the GNU command line tools earlier. However, you will also need to install the [Command Line Tools for Xcode](https://developer.apple.com/library/archive/technotes/tn2339/_index.html).

#### Running and Developing .NET on macOS

For macOS, there are two options. The recommended option is [Visual Studio Code](https://code.visualstudio.com/) and the alternative is [Rider by JetBrains](https://www.jetbrains.com/rider/)

#### Visual Studio Code

1. You can download and install Visual Studio Code here: [https://code.visualstudio.com/Download](https://code.visualstudio.com/Download).

1. Download and install the latest .NET SDK and Run-time, which can be found at [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

1. To configure Visual Studio Code to work with C#, download and install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) from Microsoft.

**NOTE:** By default, we target the oldest support .NET for the projects contained within this repo. This is done to ensure we are using something other than functionality that is supported at a later version and onward. To target the specific .NET that you downloaded (for example, .NET 8), run the following commands from the root of the repo:

```bash
find ./ -type f -iname "*.csproj" -not -path "./.git" -exec sed -i.bak 's/<TargetFramework>net6.0<\/TargetFramework>/<TargetFramework>net8.0<\/TargetFramework>/g' "{}" +;
# if you are comfortable with what was changed, you can delete the backup files by running this command
# find ./ -type f -iname "*.bak" -not -path "./.git" -exec rm -rf "{}" +;
```

#### (Alterative) Jetbrains Rider on macOS

For macOS, this is probably by far the easiest option, but this is a paid product with a 30-day trial. Donwload and install Rider at the link below:
[https://www.jetbrains.com/rider/](https://www.jetbrains.com/rider/)

It will download any required .NET components needed to run the examples. Just open up the `Deepgram.Dev.sln` at the root of the repo. It's that easy.

### Installing on Linux

For Linux, there are two options. The recommended option is [Visual Studio Code](https://code.visualstudio.com/) and the alternative is [Rider by JetBrains](https://www.jetbrains.com/rider/)

#### Linux Build Tools

All Linux distributions have the GNU tools available. The most popular distributions and commands used to install these tools are below.

- Debian/Ubuntu

  ```bash
  sudo apt update
  sudo apt install build-essential
  ```

- Fedora/RHEL/CentOS

  ```bash
  sudo yum update
  sudo yum groupinstall "Development Tools"
  ```

- OpenSUSE

  ```bash
  sudo zypper update
  sudo zypper install -t pattern devel_C_C++
  ```

- Arch

  ```bash
  sudo pacman -Sy base-devel
  ```

#### Running and Developing .NET on Linux

1. You can download and install Visual Studio Code here: [https://code.visualstudio.com/Download](https://code.visualstudio.com/Download).

1. Download and install the latest .NET SDK and Run-time, which can be found at [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

1. To configure Visual Studio Code to work with C#, download and install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) from Microsoft.

**NOTE:** By default we target the oldest support .NET in the projects contained within this repo. This is done to ensure we are using something other than functionality supported at a later version and onward. To target the specific .NET that you downloaded (for example, .NET 8), run the following commands from the root of the repo:

```bash
find ./ -type f -iname "*.csproj" -not -path "./.git" -exec sed -i.bak 's/<TargetFramework>net6.0<\/TargetFramework>/<TargetFramework>net8.0<\/TargetFramework>/g' "{}" +;
# if you are comfortable with what was changed, you can delete the backup files by running this command
# find ./ -type f -iname "*.bak" -not -path "./.git" -exec rm -rf "{}" +;
```

#### (Alterative) Jetbrains Rider on Linux

For Linux, this is probably by far the easiest option, but this is a paid product with a 30-day trial. Download and install Rider at the link below:
[https://www.jetbrains.com/rider/](https://www.jetbrains.com/rider/)

It will download any required .NET components needed to run the examples. Just open up the `Deepgram.Dev.sln` at the root of the repo. It's that easy.

### Installing Docker

Some aspects of development require Docker. To install Docker in your development environment, [follow the instructions from the Docker website](https://docs.docker.com/get-docker/).

**Note:** If you are running macOS, ensure that `/usr/local/bin` is in your `PATH`.

## GitHub Workflow

To check out code to work on, please refer to [this guide][github_workflow].

> Attribution: This was in part borrowed from this [document](https://github.com/kubernetes/community/blob/master/contributors/devel/development.md) but tailored for our use case.

[project]: https://github.com/deepgram/deepgram-dotnet-sdk
[issue]: https://github.com/deepgram/deepgram-dotnet-sdk/issues
[github_workflow]: https://github.com/deepgram/deepgram-dotnet-sdk/.github/GITHUB_WORKFLOW.md
