# 02-convert-project-format: Convert Emmet project file to SDK-style

Convert `src/Emmet/Emmet.csproj` from legacy format to SDK-style structure while keeping the project target framework unchanged. This is the structural conversion step before VSSDK-specific overlay adjustments.

**Done when**: Project file root uses SDK-style format, legacy imports are removed, and conversion succeeds without changing the target framework.
