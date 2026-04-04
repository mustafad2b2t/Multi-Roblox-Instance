<div align="center">
  <img src="src/logo.png" alt="Logo" width="120" style="border-radius:20px"/>
  <h1>Roblox Elite Multi-Instance Launcher</h1>
  <p>Open-source Windows WPF application for securely launching and managing multiple Roblox clients with encryption and optimized process handling.</p>

[![WPF](https://img.shields.io/badge/WPF-C%23_.NET_4.8-13A89E.svg?style=for-the-badge&logo=csharp)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-MVVM-5C2D91.svg?style=for-the-badge&logo=visualstudio)](#)
[![License](https://img.shields.io/badge/License-MIT-007EC6.svg?style=for-the-badge&logo=open-source-initiative)](#)
[![Download Latest Release](https://img.shields.io/github/v/release/mustafad2b2t/Multi-Roblox-Instance?sort=semver&cacheSeconds=0&style=for-the-badge)](https://github.com/mustafad2b2t/Multi-Roblox-Instance/releases/latest)

</div>

---

## ✨ Features

### 🛡️ Multi-Instance Support
Allows running multiple Roblox clients simultaneously by handling process-level limitations safely.

### 🔒 Secure Cookie Storage
Uses Windows DPAPI (`ProtectedData`) to encrypt `.ROBLOSECURITY` tokens locally.

### 🚀 Auth Ticket System
Generates secure authentication tickets via official Roblox endpoints instead of exposing cookies.

### ⚡ Stable Launch System
Prevents crashes and conflicts during multi-launch scenarios with controlled file access.

### 🤖 AFK Automation
Optional background system that simulates mouse movement per window to prevent idle kick.

### 🎨 Clean UI
Minimal, responsive WPF interface inspired by modern IDE design.

---

## 🛠️ Setup & Build

1. Clone the repository:
   ```bash
   git clone https://github.com/mustafad2b2t/ProjeRoblox.git
   ```

2. Open with **Visual Studio 2022**
   - Open `RobloxMultiLauncher.sln`

3. Requirements:
   - .NET Framework 4.8
   - WPF Tools

4. Build:
   - Set configuration to `Release`
   - Press `Ctrl + B`

5. Run:
   - `bin/Release/RobloxMultiLauncher.exe`

---

## 📖 How to Use

### 1. Adding Your Accounts
*   Click the **+ Add Account** button on the main dashboard.
*   **Display Username**: Enter a name to identify this account (e.g., "MainAcc" or "Alt1").
*   **Roblox Cookie**: Paste your `.ROBLOSECURITY` cookie.
    *   *Tip: Use the "How to get?" button in the dialog for a quick tutorial on extracting your cookie safely.*
*   **Place ID (Game ID)**: The numeric ID of the game you want to join (found in the Roblox URL: `roblox.com/games/1234567/...`).
*   **Private Server (Optional)**:
    *   Paste a **Full Link** (like `https://www.roblox.com/share?code=...`).
    *   OR paste just the **Server Code** (e.g., `22064a7b66a...`).
    *   *Note: If you paste just a code, ensure the Place ID is also filled.*

### 2. Launching Multiple Instances
*   Once your accounts are added, you can click **▶ Play** next to any account to launch it individually.
*   Click **🚀 Launch All** at the top to start all accounts one after another.
*   Adjust the **Launch Delay** in the ⚙️ Settings if your PC needs more time between launches to prevent crashes.

### 3. AFK Prevention
*   To keep your accounts from being kicked for idling:
    *   Click the **Toggle** button under the "AFK TOGGLE" column for specific accounts.
    *   Or use **AFK All ON / OFF** at the top to control all windows at once.
*   The status indicator will show **🟢 AFK ON** when active.

### 4. Managing Saved Games
*   Use the **🎮 Games** button to save frequently played games.
*   Saving a game here makes it easier to select when adding new accounts or updating existing ones.

---

## 🛠️ Setup & Build
1. Clone the repository:
   ```bash
   git clone https://github.com/mustafad2b2t/ProjeRoblox.git
   ```
2. Open `RobloxMultiLauncher.sln` with **Visual Studio 2022**.
3. Ensure you have **.NET 6.0+** or **.NET Framework 4.8** installed.
4. Build in `Release` mode and run the executable.

---

## 🔒 Security Note
*   **Local Encryption**: Your cookies are encrypted using Windows DPAPI, meaning ONLY YOU can access them on your computer.
*   **Open Source**: You can audit the code yourself to see how your data is handled. We never send your cookies to any external server.

---

## ⚠️ Disclaimer

This project is for educational purposes only.  
The developers are not responsible for any account actions, restrictions, or misuse.  
**Roblox is a trademark of Roblox Corporation.**
