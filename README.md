# Magic Portal (WPF)

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![UI](https://img.shields.io/badge/UI-WPF-purple)
![Azure](https://img.shields.io/badge/Azure-Blob%20Storage-0089D6)

A lightweight WPF desktop tool for managing files in an Azure Blob Storage container.

![alt text](https://github.com/rileymurphy93/magic-portal/blob/main/magic-portal.png)

## Prerequisites

Before running the app, make sure you have:

- **Visual Studio 2022**
- **.NET 8 SDK** (or your org’s supported .NET version)
- **Azure Storage account + container**
- A valid **Container SAS token** with permissions to:
  - list/read blobs  
  - create/write blobs  
  - overwrite blobs if needed  

## Configuration

The WPF app reads its config from `appsettings.json` at runtime. Edit the fields of the json file with your information.

### `appsettings.json` (template)

```json
{
  "AzureBlob": {
    "AccountUrl": "https://<YOUR_ACCOUNT>.blob.core.windows.net/",
    "SasToken": "?sv=...YOUR_SAS_TOKEN...",
    "ContainerName": "YOUR_CONTAINER"
  }
}
```

## Formatting Requirements

To avoid configuration bugs, make sure these rules are followed:

| Setting | Requirement | Example |
|---|---|---|
| `AccountUrl` | Must end with a `/` | `https://foo.blob.core.windows.net/` |
| `SasToken` | Must start with a `?` | `?sv=...` |
| `ContainerName` | Must match the Azure container name exactly | `software` |

## Build & Run (Visual Studio)

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

## Download Location

Downloaded files are saved automatically to:

- **Windows:** `C:\Magic Portal`
- *(A Linux path exists in code, but WPF is Windows-only)*

The folder is created automatically if it doesn’t already exist.

## Troubleshooting

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


