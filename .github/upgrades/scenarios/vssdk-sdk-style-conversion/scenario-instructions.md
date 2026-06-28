# VSSDK SDK-Style Conversion

## Strategy
Convert the Emmet VSIX project in-place to SDK-style, preserving target framework and VSIX behavior.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Pace**: Standard
- **Target Project**: src/Emmet/Emmet.csproj
- **Branch Sync**: Auto (Merge)

## Decisions
- Stash existing local changes before starting conversion work.
- Keep conversion scope to the Emmet VSIX project and required solution deploy-marker updates.
- Use the current git branch for remaining conversion changes (user request, 2026-06-28).

## Source Control
- **Source Branch**: master
- **Working Branch**: vssdk-sdk-style-conversion
- **Pending Changes Handling**: stash

## Build Tool Decisions
- **src/Emmet/Emmet.csproj**: Visual Studio/MSBuild (SDK-style VSIX project targeting .NET Framework 4.7.2 with VSIX packaging and WPF/XAML content)
- **Emmet.slnx**: Visual Studio/MSBuild for final solution validation

## Custom Instructions
<!-- Task-specific overrides: "For {taskId}: {instruction}" -->
