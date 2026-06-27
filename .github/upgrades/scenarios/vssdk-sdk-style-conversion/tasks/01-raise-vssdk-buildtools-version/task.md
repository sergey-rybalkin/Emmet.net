# 01-raise-vssdk-buildtools-version: Raise VSSDK build tools to required minimum

Update central package management so `Microsoft.VSSDK.BuildTools` is at least `18.5.38461` before conversion validation. The current centrally pinned version is below the required floor for SDK-style VSIX conversion and can cause build-task failures.

**Done when**: `src/Directory.Packages.props` sets `Microsoft.VSSDK.BuildTools` to a version >= `18.5.38461` and restore succeeds.

## Research Findings

### Projects Affected
- `src/Emmet/Emmet.csproj` — consumes `Microsoft.VSSDK.BuildTools` and is the conversion target.
- `src/Directory.Packages.props` — central package management file where package versions are pinned for the solution.

### Files to Modify
- `src/Directory.Packages.props` — raise `Microsoft.VSSDK.BuildTools` from `17.14.2142` to at least `18.5.38461`.

### Packages to Update
| Package | Current | Target | Notes |
|---------|---------|--------|-------|
| Microsoft.VSSDK.BuildTools | 17.14.2142 | 18.5.38461 | Required minimum for SDK-style VSIX conversion. |

### Dependencies & Risks
- Repository uses Central Package Management (`ManagePackageVersionsCentrally=true`), so version changes must be made in `Directory.Packages.props` and not in the project file.
- Validation should use restore/build after version update to confirm package resolution.

### Decisions Made
- Keep the update scoped to the single required package floor increase and validate via build.
