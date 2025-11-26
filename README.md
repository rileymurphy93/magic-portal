# Magic Portal (WPF)

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![UI](https://img.shields.io/badge/UI-WPF-purple)
![Azure](https://img.shields.io/badge/Azure-Blob%20Storage-0089D6)

A lightweight WPF desktop tool for managing files in an Azure Blob Storage container — browse what’s available, upload new versions, and download what you need in a couple clicks.

---

## ✨ Features

- 📃 **List blobs** in a target Azure container on startup  
- 🔄 **Refresh** blob list on demand  
- ⬆️ **Upload files** to the container (overwrites same-name blobs)  
- ⬇️ **Download selected blobs** to a local folder  
- 📂 **Auto-open download folder** after download  
- 🟢 **Status updates** for clear user feedback  

---

## 🧱 Solution Structure

```text
magic-portal.sln
│
├─ magic-portal/                       # WPF desktop app
│   ├─ App.xaml
│   ├─ MainWindow.xaml                 # Minimal shell (UI built in code)
│   ├─ MainWindow.xaml.cs              # Code-only UI + event handlers
│   └─ appsettings.json                # Azure Blob configuration (template)
│
└─ magic-portal_class_library/         # Core blob logic
    └─ blob-storage-service.cs         # BlobStorageService

---

---

## ✅ Prerequisites

Before running the app, make sure you have:

- 🧰 **Visual Studio 2022**
- 🟦 **.NET 8 SDK** (or your org’s supported .NET version)
- ☁️ **Azure Storage account + container**
- 🔑 A valid **Container SAS token** with permissions to:
  - 📃 list/read blobs  
  - ✍️ create/write blobs  
  - ♻️ overwrite blobs if needed  
```

## ⚙️ Configuration

The WPF app reads its config from `appsettings.json` at runtime.

### `appsettings.json` (template)

```json
{
  "AzureBlob": {
    "AccountUrl": "https://<your-account>.blob.core.windows.net/",
    "SasToken": "?sv=...YOUR_SAS_TOKEN...",
    "ContainerName": "software"
  }
}
```

### Formatting Requirements

To avoid configuration bugs, make sure these rules are followed:

| Setting | Requirement | Example |
|---|---|---|
| `AccountUrl` | Must end with a `/` | `https://foo.blob.core.windows.net/` |
| `SasToken` | Must start with a `?` | `?sv=...` |
| `ContainerName` | Must match the Azure container name exactly | `software` |

> **Tip**  
> If `AccountUrl` is missing the trailing `/`, the SAS URI will be malformed and blob calls will fail.

---

## 🔐 Local Secrets (Recommended)

> **Warning**  
> Never commit a real SAS token to GitHub.

`appsettings.json` is safe to commit only as a **placeholder template**.  
Keep your real SAS token in a local-only file:

1. Create `appsettings.local.json` with **real values**
2. Keep `appsettings.json` as the **sanitized template**
3. The app loads both files; local overrides when present

```csharp
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();
```
✅ appsettings.local.json is ignored by git via .gitignore.

---

## 🚀 Build & Run (Visual Studio)

1. Open `magic-portal.sln` in **Visual Studio 2022**
2. Right-click **magic-portal** → **Set as Startup Project**
3. Press **F5** to run in Debug

### Output Verification

After building, confirm your output folder contains:

- `magic-portal.exe`
- `appsettings.json`
- *(optionally)* `appsettings.local.json`

Output path:
```text
magic-portal/bin/Debug/net8.0-windows/
```
Note
If appsettings.json is missing from the output, set its file properties to:
Build Action = Content and Copy to Output Directory = Copy if newer.

---

## 📥 Download Location

Downloaded files are saved automatically to:

- 🪟 **Windows:** `C:\Magic Portal`
- 🐧 *(A Linux path exists in code, but WPF is Windows-only)*

The folder is created automatically if it doesn’t already exist.

---

## 📦 Publish a Shareable Build

To create a build you can zip/share with testers:

1. Right-click **magic-portal** → **Publish…**
2. Select **Folder**
3. Choose an output directory (example: `publish/`)
4. Click **Publish**

Send the entire publish folder to testers.

> **Tip**  
> The publish output should include your `.exe` and `appsettings.json` side-by-side.

---

## 🔒 Security Notes

- 🔑 SAS tokens grant direct access to storage — treat them like passwords  
- 🚫 Do **not** commit real SAS tokens to GitHub  
- ⏳ Prefer short-lived SAS tokens and rotate if exposed  
- 🏢 Long-term best practice is Entra ID / RBAC, but SAS is OK for early internal testing  

---

## 🛠 Troubleshooting

### App can’t find `appsettings.json`
Make sure the file properties in the WPF project are:

- **Build Action:** `Content`
- **Copy to Output Directory:** `Copy if newer`

### “AuthorizationFailure” / 403 errors
- SAS token expired or missing permissions  
- Confirm SAS includes: `r`, `l`, `w`, `c`, `a`

### Blob list is empty but container isn’t
- Wrong container name  
- SAS scoped to a different container


