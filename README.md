# Choose2Extract

A client-side BepInEx plugin for SPT that stops extraction from starting the moment you step into an exit zone. Instead you get an **Extract** prompt, the same kind transit zones use, and the extract only begins once you confirm it.

## Features

- Native interaction prompt in extract zones, no countdown until you choose to extract
- Confirm with a 2 second hold like transits (can be turned off, so selecting the prompt extracts right away)
- Hold countdown uses the extraction timer's green instead of the transit orange
- Transit zones show **Transit** on their prompt instead of a generic "Interact", so the two are easy to tell apart
- Where an extract zone and a transit zone overlap, one menu offers both options
- Exits with unmet requirements (missing key, switch, and so on) behave exactly as before

## Installation

1. Download the latest release archive.
2. Extract it into your SPT folder, so that this file ends up in place:
   `BepInEx/plugins/sirhroflstomp-Choose2Extract/sirhroflstomp-Choose2Extract.dll`
3. Start the game.

There is no server component.

## Usage

Walk into an extract zone. The **Extract** prompt appears like any other interaction.

- Use the interact key to select it. With the default settings, hold it for 2 seconds while standing still until the countdown finishes.
- If a transit is also available, scroll to switch between **Extract** and the transit option.
- Walking out of the zone cancels the prompt and any hold in progress.
- Other interactions (doors, containers) take priority over the prompt while you are looking at them.

## Configuration

Settings are in `BepInEx/config/com.sirhroflstomp.choose2extract.cfg` after the first launch.

| Setting | Default | Description |
| --- | --- | --- |
| Prompt name | `Extract` | Text shown on the prompt |
| Transit prompt name | `Transit` | Text shown on the transit prompt, leave empty to keep the game's own text |
| Require hold to confirm | `true` | Turn off to extract as soon as you select the prompt |
| Hold seconds | `2` | How long the key has to be held, `0` skips the hold |
| Countdown text | `Confirmation {0:F1}` | Text of the hold countdown, `{0:F1}` is the time left |
| Hold box color | `#7CB704DC` | Countdown box color as `#RRGGBB` or `#RRGGBBAA` |

## Compatibility

Developed and tested on SPT 4.1.3. It patches the exfiltration and interaction code of the game client, so other mods that change how extracts start may conflict.

## Uninstalling

Delete the `BepInEx/plugins/sirhroflstomp-Choose2Extract` folder. Extracts go back to starting automatically.

## Building

You need the .NET SDK and an SPT install, since the project references the game's assemblies and does not ship them.

Tell the build where SPT is, either on the command line:

```powershell
dotnet build -c Release -p:GameDir="C:\path\to\SPT"
```

or once, in a file called `GameDir.user.props` next to the project (it is git-ignored):

```xml
<Project>
  <PropertyGroup>
    <GameDir>C:\path\to\SPT</GameDir>
  </PropertyGroup>
</Project>
```

After that a plain `dotnet build -c Release` is enough. A build also copies the plugin into `BepInEx/plugins` of that folder.

To create the release archive:

```powershell
.\package.ps1
```

The zip ends up in `dist/`. Pass `-GameDir "C:\path\to\SPT"` if you have no `GameDir.user.props`.

## License

Public domain, released under the [Unlicense](LICENSE). Copy, change and redistribute it however you like.
