# Grand Theft Auto: San Andreas - The Definitive Edition on_mission Changer Tool

A Windows application built with WPF to toggle the `on_mission` value in the memory of *Grand Theft Auto: San Andreas - The Definitive Edition* (SanAndreas.exe) using hotkeys. The program supports multiple game versions by reading hardcoded offsets or user-defined offsets stored in a JSON file. It allows users to add new game versions and their corresponding memory offsets via a user-friendly interface.

## Features

- **Hotkey Support**: Press `F6` to toggle the `on_mission` value (0 or 1) in the game's memory.
- **Dynamic Process Attachment**: Automatically detects and attaches to the *SanAndreas* process.
- **Version Support**: Includes hardcoded memory offsets for common game versions and supports custom offsets via a JSON configuration.
- **User Interface**: A clean WPF interface displays the current `on_mission` value and attachment status.
- **Custom Version Addition**: Add new game versions and hex offsets (with or without `0x` prefix) through a dialog window with tooltips for guidance.

## Prerequisites

- **Operating System**: Windows (tested on Windows 10 and later).
- **.NET Framework**: .NET Framework 4.8 or later.
- **Grand Theft Auto: San Andreas - The Definitive Edition**: A running instance of the game (SanAndreas.exe).

## Usage

1. **Launch the Application**:
   - Download and run [`SA_DE_OM_Changer.exe`](https://github.com/SkipperSkipTR/SA_DE_OM_Changer/releases/download/1.0/SA_DE_OM_Changer.exe).
   - The application will automatically attempt to detect and attach to a running *SanAndreas* process every 500ms.

2. **Monitor Status**:
   - The main window displays:
     - **Status**: Indicates whether the program is attached to the game process or any errors (e.g., "Status: Attached. Use F6 to toggle on_mission.").
     - **on_mission Value**: Shows the current value of the `on_mission` (0 or 1).

3. **Toggle on_mission**:
   - Press `F6` while the game is running and the program is attached to toggle the `on_mission` value between 0 and 1.

4. **Add a New Game Version**:
   - Click the "Add Version" button to open a dialog.
   - Enter the game version (e.g., `1.0.0.14388`) and memory offset in hex (e.g., `5010878` or `0x5010878`).
   - Hover over the ⓘ icons for tooltips with example images and instructions.
   - Click "Add" to save the version and offset to `additional_addresses.json`.

5. **Troubleshooting**:
   - If the program fails to attach, ensure *Grand Theft Auto: San Andreas - The Definitive Edition* is running.
   - If the version is unsupported, add it via the "Add Version" dialog.

## Configuration

- **Hardcoded Offsets**: Predefined game versions and their memory offsets are stored in `AddressConfig.cs`.
- **Custom Offsets**: User-added versions are saved in `additional_addresses.json` in the application directory.

## Example JSON Configuration

`additional_addresses.json`:
```json
{
  "1.0.0.99999": "5010878"
}
```

## Notes

- **Hex Input**: The "Add Version" dialog accepts hex offsets with or without the `0x` prefix (e.g., `5010878` or `0x5010878`).
- **Error Handling**: The program displays error messages for unsupported versions, failed attachments, or invalid inputs.
