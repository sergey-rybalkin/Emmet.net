# Assessment: VSSDK SDK-Style Conversion

## Target Project
| Property | Value |
|----------|-------|
| Project | Emmet |
| Path | src/Emmet/Emmet.csproj |
| Current TFM | .NET Framework 4.7.2 (`v4.7.2`) |
| Solution format | .slnx |
| packages.config | No |

## VSIX Components Found
- [x] VSIX manifest (`src/Emmet/source.extension.vsixmanifest`)
- [x] VSCT command table (`src/Emmet/EmmetPackage.vsct`)
- [ ] Tool windows
- [x] MEF exports (`System.ComponentModel.Composition` reference present)
- [ ] Custom editors
- [ ] Language services

## Current Package References
- `Microsoft.ClearScript.V8` (central package version: `7.5.0`)
- `Microsoft.ClearScript.V8.Native.win-x64` (central package version: `7.5.0`)
- `Microsoft.VisualStudio.SDK` (central package version: `17.14.40265`)
- `Microsoft.VSSDK.BuildTools` (central package version: `17.14.2142`)

## Baseline
- Project builds: Yes
- Solution builds: Yes

## Key Findings
- Project is legacy non-SDK style and includes legacy imports (`Microsoft.Common.props`, `Microsoft.CSharp.targets`, `Microsoft.VsSDK.targets`) plus debug launch properties (`StartAction`, `StartProgram`, `StartArguments`) that must be removed.
- Auto-generated files (`source.extension.cs`, `EmmetPackage.Constants.cs`) currently use `Compile Include`; they must be converted to `Compile Update` in SDK-style to avoid duplicate compile items.
- `Properties/AssemblyInfo.cs` exists and should be removed (or assembly info generation disabled) during SDK-style conversion.
- Solution is `.slnx`; deploy marker must be added as `<Deploy />` inside the Emmet project node for F5 experimental instance deployment.
- Central package management currently pins `Microsoft.VSSDK.BuildTools` to `17.14.2142`, which is below required minimum `18.5.38461`; conversion requires raising this package version before validation build.
