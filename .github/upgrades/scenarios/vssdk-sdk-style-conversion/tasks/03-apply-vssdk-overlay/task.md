# 03-apply-vssdk-overlay: Apply VSIX-specific SDK-style adjustments

Apply VSSDK-specific project configuration required for VSIX projects, including project capabilities, generated-file metadata mode (`Compile Update`), package assets, and removal of legacy debug launch properties and assembly info conflicts.

**Done when**: VSIX manifest/VSCT/content items are preserved, generated files use `Compile Update`, legacy debug launch properties are removed, and `Properties/AssemblyInfo.cs` is removed.

## Verification Findings
- `src/Emmet/Emmet.csproj` already contains VSSDK overlay properties and capability (`VSSDKBuildToolsAutoSetup`, `VsixDeployOnDebug`, `UseCodebase`, `ProjectCapability CreateVsixContainer`).
- VSIX manifest item, VSCT compile item, and VSIX content items are preserved.
- Generated files use `Compile Update` metadata entries (including `source.extension.cs` and `EmmetPackage.Constants.cs`).
- Legacy debug launch properties (`StartAction`, `StartProgram`, `StartArguments`) are already removed.
- `Properties/AssemblyInfo.cs` is already absent.

## Decisions Made
- Mark task complete as already satisfied by previous conversion edits; no additional code changes required.
