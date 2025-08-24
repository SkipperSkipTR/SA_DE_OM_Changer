# SA_DE_OM_Changer

A Windows application built with WPF to toggle the `on_mission` value in the memory of *Grand Theft Auto: San Andreas* (SanAndreas.exe) using hotkeys. The program supports multiple game versions by reading hardcoded offsets or user-defined offsets stored in a JSON file. It allows users to add new game versions and their corresponding memory offsets via a user-friendly interface.

## Features

- **Hotkey Support**: Press `F6` to toggle the `on_mission` value (0 or 1) in the game's memory.
- **Dynamic Process Attachment**: Automatically detects and attaches to the *SanAndreas* process.
- **Version Support**: Includes hardcoded memory offsets for common game versions and supports custom offsets via a JSON configuration.
- **User Interface**: A clean WPF interface displays the current `on_mission` value and attachment status.
- **Custom Version Addition**: Add new game versions and hex offsets (with or without `0x` prefix) through a dialog window with tooltips for guidance.
- **Tooltip Enhancements**: Tooltips in the "Add Version" window include images and text for clarity, with customizable display delay.

## Prerequisites

- **Operating System**: Windows (tested on Windows 10 and later).
- **.NET Framework**: .NET Framework 4.8 or later.
- **Grand Theft Auto: San Andreas**: A running instance of the game (SanAndreas.exe).
- **Visual Studio**: For building the project (optional, if modifying the source code).
- **Image Resources**: Placeholder images (`version_example.png` and `address_example.png`) in the `Images` folder with the build action set to `Resource`.

## Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/SA_DE_OM_Changer.git
   cd SA_DE_OM_Changer
   ```

2. **Open in Visual Studio**:
   - Open the solution file (`SA_DE_OM_Changer.sln`) in Visual Studio.
   - Ensure the `Images` folder contains `version_example.png` and `address_example.png` with the build action set to `Resource`.

3. **Build the Project**:
   - Set the solution configuration to `Release` or `Debug`.
   - Build the solution (`Build > Build Solution`).

4. **Run the Application**:
   - Run the built executable (`SA_DE_OM_Changer.exe`) from the `bin/Release` or `bin/Debug` folder.

## Usage

1. **Launch the Application**:
   - Run `SA_DE_OM_Changer.exe`.
   - The application will automatically attempt to detect and attach to a running *SanAndreas* process every 500ms.

2. **Monitor Status**:
   - The main window displays:
     - **Status**: Indicates whether the program is attached to the game process or any errors (e.g., "Status: Attached. Use F6 to toggle on_mission.").
     - **on_mission Value**: Shows the current value of the `on_mission` byte (0 or 1).

3. **Toggle on_mission**:
   - Press `F6` while the game is running and the program is attached to toggle the `on_mission` value between 0 and 1.

4. **Add a New Game Version**:
   - Click the "Add Version" button to open a dialog.
   - Enter the game version (e.g., `1.0.0.14388`) and memory offset in hex (e.g., `5010878` or `0x5010878`).
   - Hover over the ⓘ icons for tooltips with example images and instructions.
   - Click "Add" to save the version and offset to `additional_addresses.json`.

5. **Troubleshooting**:
   - If the program fails to attach, ensure *Grand Theft Auto: San Andreas* is running.
   - If the version is unsupported, add it via the "Add Version" dialog.
   - Check the `Images` folder for missing resources if tooltips do not display correctly.

## Project Structure

- **MainWindow.xaml/cs**: The main WPF window, handling process attachment, hotkey registration, and UI updates.
- **AddVersionWindow.xaml/cs**: A dialog for adding new game versions and memory offsets.
- **AddressConfig.cs**: Manages hardcoded and custom memory offsets, stored in `additional_addresses.json`.
- **MemorySession.cs**: Handles process attachment and memory read/write operations.
- **Native.cs**: Defines P/Invoke methods for Windows API calls (e.g., `ReadProcessMemory`, `RegisterHotKey`).
- **Images/**: Contains `version_example.png` and `address_example.png` for tooltips.

## Configuration

- **Hardcoded Offsets**: Predefined game versions and their memory offsets are stored in `AddressConfig.cs`.
- **Custom Offsets**: User-added versions are saved in `additional_addresses.json` in the application directory.
- **Tooltip Images**: Ensure `version_example.png` and `address_example.png` are included in the `Images` folder with the correct build action (`Resource`).

## Example JSON Configuration

`additional_addresses.json`:
```json
{
  "1.0.0.99999": "5010878"
}
```

## Notes

- **Hex Input**: The "Add Version" dialog accepts hex offsets with or without the `0x` prefix (e.g., `5010878` or `0x5010878`).
- **Tooltip Delay**: Tooltips in the "Add Version" window appear after a 100ms delay for quick access.
- **Image Size**: Tooltips use the native dimensions of the images for clarity. Ensure images are appropriately sized.
- **Error Handling**: The program displays error messages for unsupported versions, failed attachments, or invalid inputs.

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/your-feature`).
3. Commit your changes (`git commit -m "Add your feature"`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a pull request.

Please include tests and update documentation as needed.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/) and .NET Framework.
- Inspired by the need to simplify memory editing for *Grand Theft Auto: San Andreas*.
