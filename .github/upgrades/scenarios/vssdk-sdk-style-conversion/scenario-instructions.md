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

## Source Control
- **Source Branch**: master
- **Working Branch**: vssdk-sdk-style-conversion
- **Pending Changes Handling**: stash

## Custom Instructions
<!-- Task-specific overrides: "For {taskId}: {instruction}" -->
