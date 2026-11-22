# Purchased Asset Store packs (local)

Freehold’s GitHub repo is **public**, so purchased packs stay on this machine only (gitignored).

## Required for Press Play visuals

Import (or keep) these under the Freehold project:

1. **Floreswa — Low Poly Character Pack** → `Assets/Floreswa/`
2. **BigRookGames — Stylized Hand Painted Nature Pack** → `Assets/BigRookGames/Nature Pack/`

Source on this PC:

- `%APPDATA%\Unity\Asset Store-5.x\Floreswa\...\Low Poly Character Pack.unitypackage`
- `%APPDATA%\Unity\Asset Store-5.x\BigRookGames\...\Stylized Hand Painted Nature Pack - Free.unitypackage`

Package Manager → My Assets → Import / Download, or double-click the `.unitypackage` while Freehold is open.

## Wiring

- Player: `PackPlayerVisual` → `Assets/Floreswa/Prefabs/male01_1.prefab`
- Trees / bushes / grass / ground: `PackPropFactory` → `Assets/BigRookGames/Nature Pack/Prefabs/*` + grass materials
