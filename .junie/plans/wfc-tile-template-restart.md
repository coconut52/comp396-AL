---
sessionId: session-261016-205817-1ke7
---

# Requirements

### Overview & Goals
The WFC system was removed, so only `Assets/Prefab/WFC/Tiles/Tile.prefab` remains. This plan rebuilds that prefab as a clean, reusable **Tile template**, using the newly imported Yughues pavement materials. It is the starting point for future WFC work.

### Scope
**In scope**
- Redesign `Tile.prefab` as a single flat plane that is 4x4 world units.
- Make the pavement materials render correctly under URP.
- Create a few tile variants as prefab variants of the template, so the template can be seen in use.
- Add a small preview scene or scene section to inspect the tiles side by side.

**Out of scope**
- WFC scripts, a generator, a tile-data ScriptableObject, an adjacency or edge-socket system.
- Props or buildings (BretwaldaGames).
- Changes to `Docs/*.md` (these describe the removed system). They can be deleted or ignored.

### Functional Requirements
- `Tile` has a root GameObject named `Tile` at the origin with unit scale, and one child `Ground` plane.
- `Ground` is a 4x4 unit horizontal plane. The footprint is the same as the old 16-quad tile (4x4 world units, with the Tile root at the centre of the footprint), so the old grid assumption `cellWidth = cellHeight = 4` still holds.
- The prefab has one material slot, so a variant swaps the look by overriding one material.
- Tile variants use `M_YFPM_*` pavement materials, for example `Pavement01`, `StonesFlat` and `Rough01`. Existing `Assets/Materials/Water.mat` and `Sand.mat` are left as they are.
- The prefab has no scripts, so it works with any later WFC code.

### Non-Functional Requirements
- Rendering works in the project's URP setup (`Assets/Settings/PC_RPAsset`). No pink materials.
- Fewer renderers and colliders than before: 1 instead of 16 of each.
- Asset GUIDs for `Tile.prefab` are preserved, because the existing `.meta` file is kept.

# Technical Design

### Current Implementation
- `Assets/Prefab/WFC/Tiles/Tile.prefab` is 1858 lines of YAML. The root `Tile` (`Transform` `435671973238645324`) has 16 children named `Quad`, `Quad (1)` ... `Quad (14)`.
- Each child is a built-in Unity Quad (mesh `fileID: 10210`) rotated 90° on X, with a `MeshRenderer` and a `MeshCollider`.
- Children sit at x/z offsets of ±0.5 and ±1.5, so the footprint is 4x4 units.
- All children share one material (`guid 31321ba15b8f8eb4c954353edc038b1d`).
- `Assets/Scripts/WFC` is empty (only the folder `.meta`). `Assets/Prefab/WFC/Tiles` contains only `Tile.prefab`.
- The new materials are in `Assets/YughuesFreePavementsMaterials/Materials/M_YFPM_*.mat`, with textures in `.../Textures`.
- **Their shader is `fileID: 45, guid 0000000000000000f000000000000000`, which is the Built-in Standard shader.** The project uses URP, so these materials would render pink. The folder also contains `YughuesFreePavementMaterials_URP.unitypackage`, which has URP versions.

### Key Decisions
- **Single plane instead of 16 quads.** This is what you chose. Per-cell material variation is not needed for a template, and a single plane is cheaper and easier to author.
- **Keep the 4x4 world-unit footprint.** This matches the previous design (`cellWidth = cellHeight = 4`) and the existing scene scale.
- **Use the Unity built-in Plane (10x10 mesh) scaled to 0.4, or a Quad rotated 90° on X scaled to (4,4,1).** The Quad is the better fit because it matches the mesh already used (`10210`), and a scale of 4 gives the correct tile size. Its UVs run 0 to 1 over the quad, so use material tiling to keep the texel density consistent across variants.
- **Variants are prefab variants of `Tile.prefab`.** They override only the `Ground` material. Changes to the template then flow to every variant.
- **Convert the pavement materials to URP by importing `YughuesFreePavementMaterials_URP.unitypackage`**, which overwrites the Standard-shader `.mat` files. If you would rather not run the package import, the fallback is the Render Pipeline Converter (Built-in to URP, Material Upgrade).

### Proposed Changes
- Rewrite `Tile.prefab` in the Unity Editor, or by editing the YAML. Remove the 16 `Quad (n)` children. Add one child `Ground` with `MeshFilter` (Quad), `MeshRenderer` and `MeshCollider`. Set its local rotation to (90,0,0), scale to (4,4,1) and position to (0,0,0).
- Set `Ground` to `Static` for batching, with `Cast Shadows = Off`. Assign `M_YFPM_Pavement01` as the default material.
- Add `Assets/Prefab/WFC/Tiles/Variants/` with 3 prefab variants of `Tile`: `Tile_Pavement`, `Tile_StonesFlat` and `Tile_Rough`. Each overrides only the `Ground` material.
- Add a throwaway preview in `Assets/Scenes/mapGenerator.unity` (or a new scene `TilePreview.unity`) that lays the variants out 4 units apart, to check tiling and seams.

### File Structure
- Modified: `Assets/Prefab/WFC/Tiles/Tile.prefab`. Its `.meta` and GUID are kept.
- Added: `Assets/Prefab/WFC/Tiles/Variants/Tile_Pavement.prefab`, `Tile_StonesFlat.prefab` and `Tile_Rough.prefab`.
- Modified: `Assets/YughuesFreePavementsMaterials/Materials/M_YFPM_*.mat` (URP shader after the package import).
- Added: a preview scene or scene section.

### Risks
- **Pink materials.** Mitigated by the URP package import or the converter. Verify by reading the `m_Shader` GUID of the materials afterwards (it should be the URP/Lit shader, not `fileID: 45`).
- **Texture seams or visible tiling repeats at the tile edges.** Mitigated by checking in the preview scene and tuning the material tiling to a whole number.
- **Unity must be closed or must reload.** Hand-editing prefab YAML while the Editor is open can cause conflicts, so do the edits in the Editor or reimport afterwards.
- **The unused `Docs/*.md` describe the old 16-quad, 10x10 system and could mislead later work.** They are not touched here.

# Testing

### Validation Approach
Check the changed assets in the file system, and check them visually in the Editor if it is available. No code is added, so there are no unit tests.

### Key Scenarios
- `Tile.prefab` contains exactly one root `Tile` and one child `Ground`. It has no `MonoBehaviour` entries and no references to deleted scripts (grep for `m_Script`).
- `Ground` has world size 4x4, a single `MeshRenderer` and a single material.
- Each variant file references `Tile.prefab` as its source prefab and overrides only the material.
- The materials used by the tiles reference a URP shader (grep `m_Shader` in the `M_YFPM_*.mat` files).
- In the preview scene, the three variants sit side by side with no pink materials and no gaps between them.

### Edge Cases
- No missing references or errors in the Unity Console after reimporting.
- The old material GUID `31321ba1...` is no longer referenced by `Tile.prefab`.
- Other prefabs and scenes do not reference `Tile.prefab`'s deleted child fileIDs (grep the scenes for `Tile.prefab` GUID usage).

# Delivery Steps

###   Step 1: Make the pavement materials URP-compatible
The `M_YFPM_*` materials use a URP shader and render without pink in the project.

- Confirm the materials currently reference the Built-in Standard shader (`m_Shader fileID: 45`).
- Import `YughuesFreePavementMaterials_URP.unitypackage`. If the import is not possible, run the Render Pipeline Converter material upgrade instead.
- Verify that `M_YFPM_Pavement01`, `StonesFlat` and `Rough01` now reference the URP/Lit shader, and that the textures are still assigned.

###   Step 2: Rebuild Tile.prefab as a single-plane template
`Tile.prefab` is one root `Tile` with a single 4x4 `Ground` plane and no scripts.

- Remove the 16 `Quad (n)` children and their renderers and colliders.
- Add a `Ground` child with Quad mesh, `MeshRenderer` and `MeshCollider`, rotation (90,0,0) and scale (4,4,1).
- Assign `M_YFPM_Pavement01` as the default material. Mark `Ground` as `Static`. Turn shadow casting off.
- Keep `Tile.prefab.meta` so the GUID is preserved.

###   Step 3: Add tile variants and a preview scene
Three variants of the template are visible side by side, 4 units apart, in a preview scene.

- Create `Assets/Prefab/WFC/Tiles/Variants/` with `Tile_Pavement`, `Tile_StonesFlat` and `Tile_Rough` as prefab variants of `Tile`. Each overrides only the `Ground` material.
- Place them in a preview scene (`TilePreview.unity` or in `mapGenerator.unity`) and check material tiling and seams.
- Grep the prefabs, materials and scenes for leftover references to removed assets or script GUIDs.