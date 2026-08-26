# OopsCaps 🔠🔄

OopsCaps is a lightweight, portable Windows utility that allows you to quickly change the case of selected text in any application using global hotkeys. 

## ✨ Features
- **4 Text Transformation Modes:**
  - **Invert case:** `hELLO` ➔ `Hello`
  - **UPPERCASE:** `hello` ➔ `HELLO`
  - **lowercase:** `HELLO` ➔ `hello`
  - **Title Case:** `hello world` ➔ `Hello World`
* **Smart Caps Lock Toggle:** Automatically toggles your system's Caps Lock state after using the *Invert* function, immediately fixing the root cause of your typo.
- **Customizable Hotkeys:** Set your own modifiers (Ctrl, Shift, Alt) and specific letter keys for each action.
- **Multi-language Support:** The interface is available in English, Latvian, and Russian.
- **Audio Feedback:** An optional, pleasant sound effect ("Speech On" / subtle click) plays when text is successfully transformed.
- **Start with Windows:** Option to automatically run the app in the background on system startup.
- **Portable & Native:** Written purely in C# (Windows Forms). No installation required.

## 🚀 How to Use
1. Run `OopsCaps.exe`. It will quietly sit in your system tray.
2. Select text in any editor, browser, or document.
3. Press one of the chosen hotkeys:
   - `Ctrl + Shift + I` ➔ **Invert Case**
   - `Ctrl + Shift + U` ➔ **UPPERCASE**
   - `Ctrl + Shift + L` ➔ **lowercase**
   - `Ctrl + Shift + T` ➔ **Title Case**
4. The text will instantly be replaced!

*Note: You can right-click or left-click the purple "Oo" tray icon to access **Settings** or Exit.*

## ⚠️ SmartScreen Warning & Downloading
If you download the pre-compiled `.exe` from the **Releases** page, Windows might show a **"Windows protected your PC" (SmartScreen)** warning. This happens because the application is an indie project and isn't digitally signed with an expensive certificate.

**How to run it:**
- Click **More info** ➔ **Run anyway**.
- *Alternatively*, download the `.zip` version from Releases (which sometimes bypasses the warning after extraction).
- *Best option:* Compile it yourself for 100% transparency! (See below).

## 🛠️ How to Build from Source
You don't need Visual Studio or any heavy IDEs to build OopsCaps! It uses the standard C# compiler (`csc.exe`) that is already built into Windows.

1. Download the source code. Ensure you have `Program.cs`, `VersionInfo.cs` (if used), and `OopsCaps.ico` in the same folder.
**(Note: The `.ico` file must be present for the compilation command to work).**
2. Open **Command Prompt (CMD)** in that folder.
3. Run this exact command:

```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.dll /out:OopsCaps.exe /win32icon:OopsCaps.ico Program.cs VersionInfo.cs
```

Result: A fresh, locally trusted OopsCaps.exe will be generated in your folder. Windows will not flag it because it was compiled by you on your own machine.

## ☕ Support
If you find this tool useful and it saves you time, feel free to support the project:

https://www.buymeacoffee.com/didthislv

## 📄 License
This project is licensed under the MIT License.

Thank you for using my tool! If you have any questions or suggestions for improvements, feel free to reach out or open an Issue.

## Changelog
* **v1.4** - Changed default "Toggle Caps Lock (after Invert)" to True.
* **v1.3** - Fixed clipboard conflicts causing application crashes in Autodesk AutoCAD and Revit.
* **v1.2** - Initial settings and translation support.
* **v1.1** - Functional and visual improvements.
* **v1.0** - Let's start ;)