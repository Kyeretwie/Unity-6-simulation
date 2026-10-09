# Imported asset sources

## Office / commercial buildings

The project contains the **Kenney City Kit Commercial** under
`ThirdParty/Kenney/City Kit Commercial`.  Use the `.fbx` files in
`Models/FBX format` in Unity; they are the most direct Unity-ready version.
Useful office-like choices include `building-a` through `building-n` and the
`skyscraper-*` models.  This kit is released under CC0 1.0.

## Ground materials

- `Materials/Grass_Ground_4K_CC0.jpg` — 4096 x 4096 tileable grass colour map,
  from Poly Haven's Grass Ground material, CC0.
- `Materials/pavement_asphalt.jpg` — 4809 x 3108 pavement/asphalt colour map,
  previously imported from ambientCG, CC0.

Set a texture's **Wrap Mode** to `Repeat`, then assign it to a URP/Lit
material's **Base Map**. Set that material's Tiling to start at `4, 4` for a
large ground plane and adjust from there.
