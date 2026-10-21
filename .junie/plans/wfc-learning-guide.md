---
sessionId: session-261017-205439-1csa
---

# Overview

## Goal
Build a Wave Function Collapse tile generator **step by step**, learning each concept with visual feedback before moving to the next. You'll write the code yourself with guided explanations — not copy-paste.

## What You Have
- **12 tile prefabs** in `Assets/Prefab/WFC/Tiles/`: Grass, Road, Sand, Water (full tiles), plus transitions (Grass_Road, Grass_RoadCorner, Grass_RoadT, Grass_Water, Sand_Grass, Sand_GrassCorner, Sand_Water, Sand_WaterCorner)
- **2 materials**: `Sand.mat`, `Water.mat` (Grass and Road likely use default or need creation)
- **17 ScriptableObject assets** in `Assets/ScriptableObjects/WFC_Tiles/` (old schema, will be recreated)
- **Existing `mapGenerator` scene** ready to use
- **Comprehensive rework docs** in `Docs/` (01–04) from a previous session — these are your reference

## Key Design Decisions (from your feedback)
1. **Baby steps** — each stage produces something you can see in the Unity Editor before moving on
2. **Code-driven setup** — tile adjacency rules defined in code (a static compatibility matrix), not hand-dragged in ScriptableObjects
3. **ScriptableObjects as immutable config only** — each stores a prefab reference + 4 edge enums + rotation info. The algorithm **never modifies** the SO, only reads from it. Runtime state lives in plain classes/structs.
4. **Rotation in code** — author each tile once at rotation 0; the generator computes rotated variants at runtime
5. **Your prefabs as-is** — we'll work with the 12 prefabs you already have

## Why the Old Approach Failed
The previous `TileData` used `List<TileData>` for neighbor rules — meaning each SO referenced other SOs. When the algorithm modified these lists during constraint propagation, it **mutated the asset files on disk**, causing bloat and corruption. The fix: use a static compatibility matrix (enum-based) so SOs are read-only config.

# Delivery Steps

### ✓ Step 1: Stage 1 — Tile identity: EdgeTerrainType enum + TilePrototype SO
Define what a tile *is* — its edges and identity — without any algorithm yet. You'll create two files and test by inspecting SOs in the Editor.
- Create `EdgeTerrainType.cs` — a `[Flags] enum` with values: `Water`, `Sand`, `Grass`, `Road`. This is the vocabulary for describing tile edges.
- Create `TilePrototype.cs` — a `ScriptableObject` with: `baseTilePrefab` (GameObject ref), `baseSpawnWeight` (int), 4x `EdgeTerrainType` fields (North/East/South/West edges at rotation 0), `doesTileAllowRotation` (bool), `distinctRotationStepCount` (1=center, 2=straight, 4=corner).
- Create 3–4 TilePrototype assets in the Editor (e.g., Grass full, Water full, Sand_WaterCorner) to practice assigning edges. Verify the Inspector shows clean enum dropdowns — no lists, no self-references.
- **Visual test**: Open the SOs in the Editor and confirm each tile's edges match its visual appearance (e.g., Sand_WaterCorner has Water on two adjacent edges, Sand on the other two).

### ✓ Step 2: Stage 2 — Compatibility matrix: EdgeCompatibility static class
Define which terrain types can touch each other — the single source of truth for adjacency rules.
- Create `EdgeCompatibility.cs` — a `static class` with a `Dictionary<EdgeTerrainType, EdgeTerrainType[]>` mapping each terrain to its compatible neighbors. Example: `Grass` is compatible with `{Water, Sand, Grass, Road}`, but `Road` only with `{Grass, Road}`.
- Implement `AreEdgeTerrainsCompatible(a, b)` — a pure function that checks both directions (symmetric union).
- **Visual test**: Write a small MonoBehaviour `CompatibilityTester` that logs which pairs are compatible/incompatible. Attach to an empty GameObject in the scene and read the Console output to verify your rules make sense (e.g., Road↔Water should be incompatible).

### ✓ Step 3: Stage 3 — Rotation math: TileVariant struct
Learn how rotating a tile remaps its edges — the core insight that lets you author fewer prefabs.
- Create `TileVariant.cs` — a `readonly struct` with: reference to the `TilePrototype`, `clockwiseRotationStepCount` (0–3), and 4 computed edge fields (`rotatedNorthEdgeTerrain`, etc.).
- Implement the rotation formula: step 0 = no change, step 1 (90° CW) = N←W, E←N, S←E, W←S, etc.
- Implement `IEquatable<TileVariant>` so variants can be compared and deduplicated.
- **Visual test**: Write a MonoBehaviour `RotationTester` that prints all 4 rotations of a corner tile's edges. Verify that rotating a Water/Sand corner 90° moves the Water edges clockwise.
- Build a helper method `BuildAllDistinctVariants(List<TilePrototype>)` that generates all variants from your prototypes, deduplicating by edge signature.

### ✓ Step 4: Stage 4 — Grid and entropy: GridCell class + grid initialization
Set up the 2D grid data structure and understand entropy (the measure of uncertainty).
- Create `GridCell.cs` — a class with: `gridCoordinateX`, `gridCoordinateY`, `remainingPossibleTileVariants` (List<TileVariant>), `isGridCellCollapsed` (bool), and a `CurrentEntropy` property (count of remaining variants).
- Create a small MonoBehaviour `GridVisualizer` that:
  - Holds a `List<TilePrototype>` field (drag your prototypes in the Inspector)
  - Builds all distinct variants (from Stage 3)
  - Creates a 10x10 `GridCell[,]` grid, seeding each cell with all variants
  - Draws a colored grid overlay using `OnDrawGizmos` — each cell colored by its entropy (blue = high, red = low)
- **Visual test**: In the Editor, see a 10x10 grid of blue cells (all equal entropy). This is your starting state before any collapse.

### ✓ Step 5: Stage 5 — Collapse: pick the most constrained cell
Implement the core WFC step: find the cell with lowest entropy and collapse it to one variant.
- Extend `GridVisualizer` (or create `WFCCollapseStep`) with:
  - `GetCellWithLowestEntropy()` — scans the grid, skips collapsed cells, returns the cell with fewest remaining variants (ties broken randomly)
  - `CollapseGridCell(cell)` — picks a random variant from the cell's remaining variants (weighted by `baseSpawnWeight`), clears the list, sets `isCollapsed = true`
  - Update `OnDrawGizmos` to color collapsed cells differently (e.g., green) and show which variant was chosen
- **Visual test**: Add a button (or call collapse once per frame with a key press) to collapse one cell at a time. Watch the grid turn from blue to green, one cell at a time, prioritizing low-entropy cells first. No propagation yet — just pure collapse.

### ✓ Step 6: Stage 6 — Propagation: constraint spreading to neighbors
The heart of WFC: when a cell collapses, remove incompatible variants from its neighbors, cascading.
- Implement `PropagateCollapseConstraints(startCell)` using a `Stack<GridCell>`:
  - For each neighbor (up/down/left/right), check which of its remaining variants are compatible with the collapsed cell's variant using `AreTileVariantsCompatibleInDirection()` (uses rotated edges + the compatibility matrix from Stage 2)
  - Remove incompatible variants; if a neighbor drops to entropy 1, auto-collapse it
  - Push modified neighbors onto the stack to continue cascading
- Implement `AreTileVariantsCompatibleInDirection(currentVariant, neighborVariant, direction)` — checks the facing edges of both variants against the compatibility matrix
- **Visual test**: Collapse one cell and watch the propagation cascade through neighbors. Cells should turn green (collapsed) or shift color (entropy reduced). Watch how a corner tile's collapse forces its neighbors to compatible types.
- Handle contradictions: if a cell drops to entropy 0, log a warning and note the coordinates.

### ✓ Step 7: Stage 7 — Full generation loop + spawning prefabs
Put it all together: run the full WFC algorithm and instantiate your prefabs in the scene.
- Create `WaveFunctionCollapseGenerator.cs` — a MonoBehaviour that:
  - Holds `width`, `height`, `cellSize`, and `List<TilePrototype>` fields
  - In `Start()`: initializes the grid, runs the collapse+propagate loop until all cells are collapsed or a contradiction occurs
  - On success: calls `SpawnAllCollapsedTileObjects()` — iterates the grid, instantiates each collapsed tile's prefab at `(x * cellSize, 0, y * cellSize)` with rotation `Quaternion.Euler(0, step * 90, 0)`
  - Parents all spawned objects under a `WFC_GeneratedMap` root for easy cleanup
  - On contradiction: logs the failure and optionally retries with a new random seed
- **Visual test**: Play the `mapGenerator` scene. Watch tiles appear in a 10x10 grid. Verify: no gaps between tiles, Water touches Water/Sand/Grass, Roads only touch Grass/Road, corners rotate correctly.
- Add a `Regenerate()` method callable from the Inspector (via `[ContextMenu]`) to re-run without stopping play mode.

### ✓ Step 8: Stage 8 — Polish: weighted selection, retry logic, and debug overlay
Add the finishing touches: better tile distribution, robustness, and visual debugging.
- Implement weighted random selection in `CollapseGridCell()` so Grass appears more often than Road (using `baseSpawnWeight` from prototypes).
- Add a retry loop: if generation hits a contradiction, clear the grid and restart (up to `maxGenerationRetryCount`, e.g., 5 attempts).
- Add a debug overlay: `OnDrawGizmos` shows the grid coordinates, entropy heatmap, and highlights the current cell being processed.
- Add `[ContextMenu]` buttons on the generator: `Regenerate`, `ClearMap`, `Show/Hide Debug Overlay`.
- **Final test**: Run generation 5+ times with different seeds. Verify consistent results: full 10x10 grid, no contradictions after retries, natural-looking terrain transitions, roads don't touch water.