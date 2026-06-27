# VSSDK SDK-Style Conversion Plan

## Overview

**Target**: Convert the Emmet Visual Studio extension project to SDK-style while preserving VSIX packaging and F5 deploy behavior.
**Scope**: Single VSIX project plus solution deploy marker updates and validation.

## Tasks

### 01-raise-vssdk-buildtools-version: Raise VSSDK build tools to required minimum

Update central package management so `Microsoft.VSSDK.BuildTools` is at least `18.5.38461` before conversion validation. The current centrally pinned version is below the required floor for SDK-style VSIX conversion and can cause build-task failures.

**Done when**: `src/Directory.Packages.props` sets `Microsoft.VSSDK.BuildTools` to a version >= `18.5.38461` and restore succeeds.

---

### 02-convert-project-format: Convert Emmet project file to SDK-style

Convert `src/Emmet/Emmet.csproj` from legacy format to SDK-style structure while keeping the project target framework unchanged. This is the structural conversion step before VSSDK-specific overlay adjustments.

**Done when**: Project file root uses SDK-style format, legacy imports are removed, and conversion succeeds without changing the target framework.

---

### 03-apply-vssdk-overlay: Apply VSIX-specific SDK-style adjustments

Apply VSSDK-specific project configuration required for VSIX projects, including project capabilities, generated-file metadata mode (`Compile Update`), package assets, and removal of legacy debug launch properties and assembly info conflicts.

**Done when**: VSIX manifest/VSCT/content items are preserved, generated files use `Compile Update`, legacy debug launch properties are removed, and `Properties/AssemblyInfo.cs` is removed.

---

### 04-update-solution-deploy-markers: Add deploy marker for SDK-style VSIX debugging

Update `Emmet.slnx` to include a deploy marker for the Emmet project so F5 launches and deploys to the experimental instance using modern VSIX deployment behavior.

**Done when**: `Emmet.slnx` includes `<Deploy />` under the Emmet project entry.

---

### 05-validate-build-and-output: Validate build and VSIX output

Run clean/build validation to ensure the converted project builds warning-free and still emits a `.vsix` package. Confirm no legacy conversion leftovers remain.

**Done when**: Solution build succeeds, VSIX output is produced, and validation checklist items from the scenario are satisfied.
