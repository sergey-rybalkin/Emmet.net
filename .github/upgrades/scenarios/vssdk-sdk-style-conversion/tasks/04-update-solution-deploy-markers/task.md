# 04-update-solution-deploy-markers: Add deploy marker for SDK-style VSIX debugging

Update `Emmet.slnx` to include a deploy marker for the Emmet project so F5 launches and deploys to the experimental instance using modern VSIX deployment behavior.

**Done when**: `Emmet.slnx` includes `<Deploy />` under the Emmet project entry.

## Research Findings

### Projects Affected
- `src/Emmet/Emmet.csproj` — VSIX project that requires solution deploy metadata for F5 deployment after SDK-style conversion.

### Files to Modify
- `Emmet.slnx` — add `<Deploy />` inside the existing `src/Emmet/Emmet.csproj` project entry.

### Dependencies & Risks
- The solution is `.slnx`, so the deploy marker is an XML child element under the project node rather than a classic `.sln` `Deploy.0` configuration entry.
- The existing Emmet project node already has platform mappings; the deploy marker can be added alongside those mappings without changing project IDs or platforms.

### Decisions Made
- Execute this as an atomic task because it modifies one solution file for one concern and has a single direct validation criterion.
