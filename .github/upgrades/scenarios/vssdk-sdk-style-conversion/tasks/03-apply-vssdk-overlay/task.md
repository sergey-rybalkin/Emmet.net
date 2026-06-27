# 03-apply-vssdk-overlay: Apply VSIX-specific SDK-style adjustments

Apply VSSDK-specific project configuration required for VSIX projects, including project capabilities, generated-file metadata mode (`Compile Update`), package assets, and removal of legacy debug launch properties and assembly info conflicts.

**Done when**: VSIX manifest/VSCT/content items are preserved, generated files use `Compile Update`, legacy debug launch properties are removed, and `Properties/AssemblyInfo.cs` is removed.
