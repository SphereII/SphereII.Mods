# Version Check Mod for 7 Days to Die by Yakov

This mod for 7 Days to Die checks for version mismatches between the game and the mod.

## Features

- Version mismatch detection: Compares the game version with the mod version and displays a warning if they don't match.
- Configurable error messages: Customize the title and description of the version mismatch warning. These can be Localization keys.

## Configuration

The mod is configured using the `versioncheck.xml` file. You can customize the following settings:

- `ModVersion`: Set this to the version of the game that the mod is designed for.
- `ErrorMessage`:
    - `Title`: Customize the title of the version mismatch warning.
    - `Description`: Customize the description of the version mismatch warning.

## Localization
The Title and Description can be localization keys. For example, the default localization is as follows:

  score_versioncheck_mismatchTitle,"Version Mismatch Detected"
  score_versioncheck_mismatchDesc,"The game version ({0}) does not match the mod version ({1}). This may cause issues."

Example configuration:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xml>
    <!-- Configuration for the Version Check -->
    <Settings>
        <!-- Set this to the version of the game you're modding -->
        <!-- This should match the version in the game's build version which is visible in the main menu (top right) -->
        <!-- Example: b316 would be <ModVersion>1.0.316</ModVersion> -->
        <ModVersion>1.0.316</ModVersion>

        <!-- Customize the version mismatch error message -->
        <ErrorMessage>
            <!-- Title of the error message box -->
            <Title>Version Mismatch Detected</Title>
            <!-- Description of the error. Use {0} for game version and {1} for mod version -->
            <Description>The game version ({0}) does not match the mod version ({1}). This may cause issues.</Description>
        </ErrorMessage>
    </Settings>
</xml>
```

## Usage

1. Start 7 Days to Die with the mod installed.
2. If there's a version mismatch between the game and the mod, you'll see a warning message when entering the main menu.
