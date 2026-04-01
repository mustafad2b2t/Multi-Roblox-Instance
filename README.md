@ -1,48 +1,70 @@
<div align="center">
  <img src="logo.png" alt="Logo" width="120" style="border-radius:20px"/>
  <h1>Roblox Elite Multi-Instance Launcher</h1>
  <p>An advanced, open-source Windows WPF application for securely launching and managing infinite simultaneous Roblox clients with proprietary anti-cheat bypasses and Windows DPAPI encryption.</p>
  <p>An enterprise-grade, open-source Windows WPF application engineered for securely launching and managing infinite simultaneous Roblox clients. Features proprietary anti-cheat bypasses and native Windows DPAPI state encryption.</p>

  [![WPF Application](https://img.shields.io/badge/WPF-C%23_.NET_Framework_4.8-blue.svg)](https://dotnet.microsoft.com/)
  [![License](https://img.shields.io/badge/License-MIT-green.svg)](#)
  [![Status](https://img.shields.io/badge/Status-Fully_Operational-success.svg)](#)
  [![WPF Application](https://img.shields.io/badge/WPF-C%23_.NET_4.8-13A89E.svg?style=for-the-badge&logo=csharp)](https://dotnet.microsoft.com/)
  [![Architecture](https://img.shields.io/badge/Architecture-MVVM-5C2D91.svg?style=for-the-badge&logo=visualstudio)](#)
  [![License](https://img.shields.io/badge/License-MIT-007EC6.svg?style=for-the-badge&logo=open-source-initiative)](#)
</div>

<br/>

## ✨ Key Features
## 🌟 Why Elite Launcher?

*   **🛡️ Multi-Instance Mutex Bypass**: Bypasses the native Roblox `ROBLOX_singletonEvent` limitation allowing you to launch completely borderless, unrestricted simultaneous game instances.
*   **🔒 Error 773 Protection (FileShare)**: Specifically engineered file-lock implementation (`RobloxCookies.dat`) completely preventing the dreaded Error 773 teleport crashes when launching multiple clients.
*   **🚀 Official HTTP Auth-Tickets**: We do NOT use risky raw cookie command-line injections. The launcher pings `auth.roblox.com` under the hood generating highly-secure, one-time authentication tickets natively mimicking the official browser launch.
*   **🔑 DPAPI Encryption**: Your `.ROBLOSECURITY` cookies are heavily encrypted into `accounts.json` using Microsoft's native **Windows DPAPI** (`ProtectedData`). Only the Windows user account that created the file can decrypt it.
*   **🌌 VS Code Dark Aesthetic UI**: Built entirely from scratch using clean XAML, featuring a premium IDE-like dark theme without bloated external UI packages.
*   **🤖 Integrated AFK System**: Toggle autonomous randomized mouse movements for each loaded instance.
Unlike typical script-kiddie tools or bloated paid launchers, **Elite Launcher** handles everything natively in pure C#. No sketchy browser injection, no risky memory manipulation. We utilize official Roblox `auth.roblox.com` APIs to generate valid one-time launch tickets, ensuring your main accounts remain safe and undetected.

<br/>
## ✨ Core Architecture

### 🛡️ Infinite Instance Subversion (Mutex Bypass)
The application suppresses the native `ROBLOX_singletonEvent` mutex. Instead of forcefully killing the mutex—which Roblox anti-cheat monitors often flag—the launcher legally retains a continuous global lock during launch cycles, allowing subsequent, unrestricted game instances to run entirely borderless without limitation.

### 🔒 Zero-Crash Protocol (Error 773 Mitigation)
Simultaneous instances natively crash each other when attempting to simultaneously read/write `RobloxCookies.dat`. Our custom `FileShare.None` locking mechanism preemptively isolates the data files during rapid multi-launches, eliminating "Error 773" teleport dropouts entirely.

### 🚀 Secure HTTP Auth-Ticket Handshake
We **do not** use raw `.ROBLOSECURITY` command-line flags (which can be scraped by simple process monitors like Task Manager). Instead, the launcher negotiates a CSRF token and communicates directly with the official `auth.roblox.com/v1/authentication-ticket` endpoint to generate hyper-secure, one-time authentication tokens mimicking the official web-browser launch pattern.

### 🔑 Cryptographic DPAPI Hardening
Your `.ROBLOSECURITY` cookies are the literal keys to your account. Instead of storing them in vulnerable plain-text configurations, the launcher utilizes Microsoft's `ProtectedData` API (Windows DPAPI). Your accounts are cryptographically bound to your hardware. **Even if your `accounts.json` is stolen by malware, the attacker cannot decrypt or use your cookies.**

### 🌌 IDE-Grade UI & UX
A meticulously crafted, dependency-free XAML interface inspired by the minimalist and highly readable aesthetics of Visual Studio Code. Featuring pure matte darks (`#1E1E1E`), fluid datagrids, zero-bloat custom styles, and completely asynchronous UI logic that never freezes.

## 🛠️ Installation & Build
### 🤖 Multi-Threaded AFK Automation
Deploys individual background threads mapped to every launched game instance. The system runs trigonometric cursor math to simulate organic, randomized AFK mouse movements constrained only to the specific client's window rectangle—keeping your accounts logged in indefinitely without hijacking your actual physical mouse cursor.

1. Clone the repository:
---

## 🛠️ Setup & Compilation

You can compile the project yourself to guarantee zero malicious payloads.

1. **Clone the repository:**
   ```bash

   git clone https://github.com/mustafad2b2t/ProjeRoblox.git
   ```
2. Open `RobloxMultiLauncher.csproj` in Visual Studio 2022.
3. Ensure **.NET Framework 4.8** SDK is installed.
4. Press `F5` or click **Start** to build and run the application.
2. Open `RobloxMultiLauncher.sln` or `RobloxMultiLauncher.csproj` using **Visual Studio 2022**.
3. Ensure the **.NET Framework 4.8 SDK** and **WPF Tools** are installed via the VS Installer.
4. Set the Build Configuration to `Release` and press `F5` or `Ctrl+B` to compile.
5. Run the compiled `RobloxMultiLauncher.exe` found in your new `bin/Release` folder!

<br/>
---

## 📚 How to Use
## 📚 Quick Start Guide

1. Click **+ Add Account** in the top left corner.
2. Enter a Display Name.
3. Fetch your Roblox `.ROBLOSECURITY` cookie from your browser (`F12 -> Application -> Cookies`) and paste it securely.
4. Provide the **Place ID** of the game you want the bot/account to join.
5. Click **Launch All** to automatically stagger-launch all saved accounts into identical games. 
1. Click the **`+ Add Account`** button in the main interface.
2. Click the **`ℹ️ How to get?`** link inside the app to view the 10-second tutorial on extracting your `.ROBLOSECURITY` token via your browser's Developer Tools.
3. Paste the token and the **Place ID** (The numerical ID extracted from the game's URL).
4. Save the account.
5. Hit **`▶ Launch All`** and watch the magic happen as windows automatically stagger-launch into identical games. 

<br/>
*(Optional)* Click the `⚙️ Settings` panel to configure the stagger delay (ms) between subsequent launches to prevent CPU spooling, or adjust the AFK randomization radiuses.

---

## ⚠️ Disclaimer
## ⚠️ Disclaimer & Legal

This project is an open-source educational utility intended to showcase process management, API mocking, and data encryption in C#. It is not intended for malicious use. Roblox and all related properties are trademarks of the Roblox Corporation.
This project is an open-source educational utility intended to showcase robust Windows process management, secure API token exchange negotiation, and local cryptographic implementation in C# / .NET. 
It is provided strictly "AS IS" without any warranties. The developers assume no responsibility for account actions, moderations, or bans. 
**Roblox and all related properties are trademarks of the Roblox Corporation.**
