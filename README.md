## Schedule 1 Modding Tool

- Work-in-progress WPF editor inspired by MCreator for building Schedule One quest, NPC, and other S1API content.
- Generates feature classes (quests, npcs, saveables) that plug into a single `Core : MelonMod` entry referencing S1API.
- MVVM structure: `Models/`, `ViewModels/`, `Views/`; generator logic in `Services/`, helpers in `Utils/`, styles/icons in `Resources/`.
- Current status: active development; no public release yet. Progress tracking lives on the project Trello board [S1 Mod Creator](https://trello.com/b/D8wEoJsh/s1-mod-creator).

## Version Management

The version is managed in `Schedule1ModdingTool.csproj` as the single source of truth. To update the version in both the project file and `AutoUpdater.xml`:

**PowerShell:**
```powershell
.\scripts\UpdateVersion.ps1 -Version "1.0.0-beta.3"
```

**Batch (Windows):**
```batch
scripts\UpdateVersion.bat 1.0.0-beta.3
```

If no version is provided, the script will prompt you for it. The CI/CD workflow automatically updates both files during releases.



## Support Us
- [Estonia](https://ko-fi.com/estonla)
- [Bars](https://ko-fi.com/ifbars)
