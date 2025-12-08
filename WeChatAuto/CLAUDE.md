# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WeChatAuto is a C# Windows Forms application that automates WeChat messaging using Windows API calls. The application provides a GUI interface for sending messages to multiple WeChat contacts automatically.

## Build and Development

### Build Commands
- **Debug Build**: `msbuild WeChatAuto.csproj /p:Configuration=Debug`
- **Release Build**: `msbuild WeChatAuto.csproj /p:Configuration=Release`
- **Run Debug**: `bin\Debug\WeChatAuto.exe`
- **Run Release**: `bin\Release\WeChatAuto.exe`

### Development Environment
- **Target Framework**: .NET Framework 4.7.2
- **Platform**: Windows (uses Windows API)
- **Project Type**: Windows Forms Application (WinExe)

## Code Architecture

### Core Components

#### Main Form
1. **Form1.cs** (`WeChatAuto` namespace) - Primary application interface with contact management and UDP communication functionality

#### Key Classes
1. **ContactItem** - Data model representing a contact with name and amount properties

#### Core Functionality
- **Contact Management**:
  - Add/remove contacts with name and amount information
  - Save/load contacts to/from JSON file (contacts.json)
  - Real-time contact list display with amount summation
- **UDP Communication**:
  - UDP server listening for external messages
  - Configurable port (default 9000)
  - Real-time message logging with automatic log limiting
  - Message processing and WeChat automation integration
- **WeChat Process Detection**: Uses `Process.GetProcessesByName("WeiXin")` to find running WeChat instances
- **Window Management**: Windows API calls to find, activate, and manipulate WeChat windows
- **Message Sending**: Automated keyboard/mouse simulation to:
  - Open WeChat search (Ctrl+F)
  - Search for contacts by name
  - Navigate to message input area
  - Type and send messages

### Windows API Dependencies
The application heavily relies on Windows API calls via P/Invoke:
- `user32.dll` functions: `FindWindow`, `SetForegroundWindow`, `ShowWindow`, `SetCursorPos`, `keybd_event`, `mouse_event`
- Window rectangle manipulation using `GetWindowRect`

### Data Persistence
- **JSON Storage**: Contacts are persisted to `contacts.json` in the application directory
- **Automatic Saving**: Contact list is saved on add/remove operations
- **Startup Loading**: Contacts are automatically loaded when application starts

### Key Methods
- `SendWeChatMessage()` - Main message sending orchestrator
- `SendMessageToWeChatContact()` - Core automation logic
- `ClickInputArea()` - Intelligent input field positioning based on window dimensions
- `IsWeChatRunning()` - WeChat process detection
- `StartUdpListening()` - Initialize UDP server for external communication
- `LoadContactsFromFile()` / `SaveContactsToFile()` - Contact persistence operations

### Design Patterns
- **Async/Await**: Used in UI event handlers to prevent blocking during message sending
- **P/Invoke**: Windows API interop for UI automation
- **JSON Serialization**: Contact data persistence using `JavaScriptSerializer`
- **Event-Driven**: UDP message handling with real-time UI updates

## Important Notes

### Namespace Usage
- The project uses a single namespace: `WeChatAuto`
- All components exist within the `WeChatAuto` namespace

### Operational Requirements
- WeChat desktop client must be running and logged in
- Application automates mouse/keyboard input, requiring user permissions
- Timing-sensitive operations use `Thread.Sleep()` for UI synchronization

### Safety Considerations
- This is UI automation software that simulates user input
- Requires careful handling of window positioning and timing
- May be affected by WeChat UI changes or different screen resolutions

## File Structure
- `Form1.cs/Form1.Designer.cs` - Main application form and UI designer
- `Program.cs` - Application entry point
- `Properties/` - Standard .NET project properties and resources
- `App.config` - .NET configuration file
- `contacts.json` - Runtime file storing contact data (auto-generated)

## Additional Features
- **UDP Server Integration**: The application includes UDP server functionality allowing external systems to send messages and trigger WeChat automation remotely
- **Contact Data Management**: Persistent contact storage with JSON serialization for data integrity
- **Real-time Logging**: Message log with automatic entry limiting (max 500 entries) to maintain performance