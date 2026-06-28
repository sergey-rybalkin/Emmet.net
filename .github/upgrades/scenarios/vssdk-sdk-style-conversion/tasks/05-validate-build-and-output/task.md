# 05-validate-build-and-output: Validate build and VSIX output

Run clean/build validation to ensure the converted project builds warning-free and still emits a `.vsix` package. Confirm no legacy conversion leftovers remain.

**Done when**: Solution build succeeds, VSIX output is produced, and validation checklist items from the scenario are satisfied.

## Research Findings

### Projects Affected
- `src/Emmet/Emmet.csproj` — final validation target for the SDK-style VSIX conversion.
- `src/Emmet.Tests/Emmet.Tests.csproj` — affected test project used to verify linked extension code still compiles and tests pass.

### Files to Modify
- `src/Emmet/Emmet.csproj` — remove validation leftovers found during final checks: SDK assembly-info opt-out and NuGet warning suppressions.
- `src/Emmet/Properties/AssemblyInfo.cs` — remove legacy assembly info file per SDK-style conversion guidance.
- `src/Emmet/Properties/InternalsVisibleTo.cs` — preserve the custom `InternalsVisibleTo("Emmet.Tests")` assembly attribute outside the removed legacy assembly info file.
- `src/Emmet/Diagnostics/Log.cs`, `src/Emmet/ViewContext.cs`, `src/Emmet/ExternalCommandsDispatcher.cs`, and command target classes — add explicit UI-thread assertions or thread-safe dispatch to satisfy Visual Studio threading analyzers without suppressing warnings.
- `src/Emmet.Tests/Emmet.Tests.csproj` and test-owned source files — keep nullable analysis for tests while preventing linked legacy extension sources from producing nullable warnings in the .NET 10 test project.

### Validation Checklist
- Project builds successfully.
- `.vsix` file is produced in the output directory.
- No duplicate `Compile` item warnings are present.
- `packages.config` is absent.
- `Properties/AssemblyInfo.cs` is removed.
- Solution file contains the deploy marker for the project.
- No legacy `<Import>` elements remain.
- No `StartAction` / `StartProgram` / `StartArguments` properties remain.
- Target framework remains `net472` / .NET Framework 4.7.2.

### Dependencies & Risks
- `Microsoft.VSSDK.BuildTools` is centrally pinned to `18.5.38461`, satisfying the SDK-style VSIX minimum.
- The project targets .NET Framework 4.7.2, uses VSIX packaging, and contains WPF/XAML, so Visual Studio/MSBuild validation is required rather than relying only on `dotnet build`.
- Removing `NoWarn="NU1604"` should be safe because package versions are supplied by central package management in `src/Directory.Packages.props`.

### Decisions Made
- Execute as one atomic validation/fix-up task because all changes are final cleanup items for the same converted VSIX project and are validated together by the final build/test gate.
