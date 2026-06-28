# 02-convert-project-format: Convert Emmet project file to SDK-style

Convert `src/Emmet/Emmet.csproj` from legacy format to SDK-style structure while keeping the project target framework unchanged. This is the structural conversion step before VSSDK-specific overlay adjustments.

**Done when**: Project file root uses SDK-style format, legacy imports are removed, and conversion succeeds without changing the target framework.

## Research Findings

### Projects Affected
- `src/Emmet/Emmet.csproj` — legacy VSIX project to be converted.

### Files to Modify
- `src/Emmet/Emmet.csproj` — structural conversion to SDK-style.

### Conversion Constraints
- Preserve the existing target framework (`v4.7.2`) in this step.
- Remove legacy import pattern and old project-format root metadata.
- Perform conversion with tooling (`convert_project_to_sdk_style`) rather than manual rewrite.

### Dependencies & Risks
- Visual Studio should unload the project before conversion to avoid file-lock/stale cache issues.
- VSIX-specific overlays are applied in the next task; this task focuses on structural conversion only.

### Decisions Made
- Attempted SDK conversion tool first; tool reported success but produced no file changes.
- Performed direct project-file conversion to SDK-style after project unload/solution close so work could proceed.
