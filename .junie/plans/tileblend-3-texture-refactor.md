---
sessionId: session-261019-193920-1ph4
---

# Current Shader Graph Analysis

### Shader Graph: SG_TileBlend — Current Structure

Your shader graph (`Assets/Shaders/SG_TileBlend.shadergraph`) is a **Universal Render Pipeline (URP) Lit** shader that blends between **2 textures** using a grayscale Mask.

#### Properties (10 total)
| Property | Type | Purpose |
|----------|------|---------|
| `TerrainA` | Texture2D | First albedo/diffuse texture |
| `TerrainB` | Texture2D | Second albedo/diffuse texture |
| `NormalA` | Texture2D (Normal) | First normal map |
| `NormalB` | Texture2D (Normal) | Second normal map |
| `Mask` | Texture2D (Default) | Grayscale blend mask (black = A, white = B) |
| `TilingA` | Vector2 | UV tiling for Texture A |
| `TilingB` | Vector2 | UV tiling for Texture B |
| `MaskCutoff` | Float (0–1) | Threshold for mask blending (default 0.5) |
| `MaskSoftness` | Float (0.001–0.5) | Smoothstep softness for mask edges (default 0.05) |
| `Smoothness` | Float (0–1) | Surface smoothness (default 0.1) |

#### Current Node Flow
1. **Mask Shape group** — UV node feeds into a Sample Texture 2D that samples the `Mask` texture
2. The mask value goes through a **Smoothstep** (using MaskCutoff and MaskSoftness) to produce a smooth 0–1 blend weight
3. **Two Lerp nodes**:
   - One Lerp blends `TerrainA` ↔ `TerrainB` (Base Color) using the mask weight
   - Another Lerp blends `NormalA` ↔ `NormalB` (Normal map) using the same mask weight
4. The blended results feed into the **Surface Description** master (BaseColor, NormalTS, Smoothness)

#### What You Want
Extend from **2 textures (A/B)** to **3 textures (A/B/C)** using an RGB channel mask instead of a single grayscale mask.

# Refactoring Instructions

### Step-by-Step Refactoring Guide

Open `SG_TileBlend.shadergraph` in the Shader Graph editor and follow these steps.

---

### Step 1: Add New Properties for Texture C

Create three new properties in the Properties panel:

1. **Right-click → Create → Property → Texture 2D**
   - Name: `TerrainC`
   - Default Type: Default (albedo texture)

2. **Right-click → Create → Property → Texture 2D**
   - Name: `NormalC`
   - Default Type: Normal Map

3. **Right-click → Create → Property → Vector 2**
   - Name: `TilingC`
   - Default Value: (1, 1)

---

### Step 2: Replace the Grayscale Mask with RGB Channel Mask

Your current `Mask` property is a grayscale texture. You have two options:

**Option A — Keep the same property name, change usage**
- Rename the existing `Mask` property to `BlendMask`
- In your material, assign an **RGB-painted mask texture** where:
  - **Red channel** = weight for Texture A
  - **Green channel** = weight for Texture B
  - **Blue channel** = weight for Texture C

**Option B — Keep grayscale mask + add a second mask (simpler transition)**
- Keep the existing `Mask` for A ↔ B blending
- Add a new `MaskC` property (Texture 2D) for controlling Texture C weight
- This gives you more control but requires 2 mask textures

**Recommended: Option A** (single RGB mask texture)

---

### Step 3: Split the RGB Mask Into Individual Channels

1. Find your existing **Sample Texture 2D** node that samples the mask texture (inside the "Mask Shape" group)
2. Add a new **Split** node (search for "Split" in the node menu)
3. Connect the mask Sample Texture 2D output to the Split node's RGBA input
4. The Split node gives you separate **R**, **G**, and **B** outputs — these are your three blend weights

---

### Step 4: Replace the Single Lerp With a 3-Way Blend

Remove the existing Lerp nodes and rebuild the blending logic:

#### For Base Color (Albedo):

1. Add **three Sample Texture 2D** nodes for `TerrainA`, `TerrainB`, and `TerrainC`
   - Wire each through its own **Tiling And Offset** node using `TilingA`, `TilingB`, `TilingC` respectively

2. Add a **Multiply** node for each texture × its mask channel:
   - `TerrainA` sample × **R** channel from Split
   - `TerrainB` sample × **G** channel from Split
   - `TerrainC` sample × **B** channel from Split

3. Add two **Add** nodes to sum the results:
   - First Add: `(TerrainA × R) + (TerrainB × G)`
   - Second Add: `result + (TerrainC × B)`

4. Connect the final Add output to the **Surface Description → Base Color** master

#### For Normal Maps:

1. Add **three Sample Texture 2D** nodes for `NormalA`, `NormalB`, and `NormalC`
   - Set each to sample as **Normal Map**

2. Use the same Multiply + Add pattern with R, G, B channels:
   - `NormalA × R + NormalB × G + NormalC × B`

3. Connect to **Surface Description → Normal** master

---

### Step 5: Optional — Normalize the Mask Weights

If your mask doesn't always sum to 1.0 (R + G + B ≠ 1), add normalization:

1. Add an **Add** node to sum R + G + B
2. Add a **Divide** node
3. Divide each channel weight by the sum before multiplying
4. This ensures correct blending even where channels overlap unevenly

---

### Step 6: Clean Up Old Properties

Remove or repurpose the properties no longer needed:
- **Remove** `MaskCutoff` and `MaskSoftness` (no longer needed with RGB channel blending; smoothness is now baked into your painted mask)
- Or keep them if you want to apply Smoothstep to each individual channel

---

### Step 7: Update Your Material

1. Open the Material using `SG_TileBlend`
2. Assign the new `TerrainC` and `NormalC` textures
3. Replace the `Mask` texture with an **RGB blend mask**:
   - Pure red areas = 100% Texture A
   - Pure green areas = 100% Texture B
   - Pure blue areas = 100% Texture C
   - Mixed colors = smooth blend between textures

---

### Alternative: Two-Stage Lerp (Simpler, No RGB Mask Needed)

If you prefer to keep your grayscale mask workflow:

1. Keep the existing Lerp for `TerrainA ↔ TerrainB` using the grayscale mask
2. Add a new grayscale mask property `MaskC` (or use the Alpha channel of your existing mask)
3. Add a second Lerp that blends the result of the first Lerp with `TerrainC` using `MaskC`
4. Repeat for normals

This is simpler but gives less independent control over the three textures.

# Delivery Steps

### ✓ Step 1: Deliver complete refactoring instructions
Provide the step-by-step guide for refactoring SG_TileBlend from 2-texture grayscale mask blending to 3-texture RGB channel mask blending.

### ✓ Step 2: Provide mask texture creation guide
Include guidance on how to create RGB blend mask textures in painting tools (Photoshop, Substance Painter, Aseprite) so the user can produce the correct mask format.