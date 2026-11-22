# Freehold

**Claim your ground. Make your own way.**

Stylized low-poly Unity life sandbox. This repo ships the **first playable survival scavenging slice**: explore a small starter area, manage hunger/thirst/stamina, pick up tools and supplies, place a campfire, cook food, and boil water.

## Unity version

- **Expected editor:** Unity **6** — **`6000.0.32f1`** (matches the Freehold Hub / Hometown Baseball install)
- **Render pipeline:** Built-in RP (no URP/HDRP required for this slice)
- Open with Unity Hub → select editor **6000.0.32f1** → open this project folder

> Cloud agents cannot run the Unity Editor. Scripts, scene, and project settings are authored so you can open locally and press Play. On first open, Unity 6 may rewrite some `ProjectSettings` fields and refresh the package lock — that is expected.

## Open & play (Hub steps for 6000.0.32f1)

1. Install [Unity Hub](https://unity.com/download).
2. In Hub → **Installs** → install **Unity 6000.0.32f1** (Unity 6 LTS line) if it is not already installed (same build used for Hometown Baseball).
3. Clone this repository (or pull the latest PR branch).
4. Hub → **Projects** → **Add** → choose the repo root (folder that contains `Assets/`, `Packages/`, `ProjectSettings/`).
5. Set the project’s editor to **6000.0.32f1**, then **Open**.
6. Allow the first import to finish (`Library/` is generated locally and gitignored).
7. Open `Assets/Scenes/StarterArea.unity` (or menu **Freehold → Open Starter Area Scene**).
8. Press **Play**. `GameBootstrap` builds the world, player, camera, loot, and HUD at runtime.

Optional: **Freehold → Validate Survival Slice** checks that core scripts and the scene are present.

If Hub offers to upgrade from an older stamp, choose **6000.0.32f1** and open — this branch is already stamped for that version.

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look (click game view to lock cursor; Esc unlocks) |
| Left Shift | Sprint (drains stamina) |
| Space | Jump |
| E | Pick up loot / light campfire / cook / boil |
| C | Place campfire (costs **3 sticks**) |
| R | Eat cooked food |
| T | Drink clean water |
| F | Eat raw food (weak restore) |
| Y | Drink dirty water (weak restore) |

## What this slice includes

- Third-person capsule player with walk, sprint, jump, and follow camera
- Needs: **hunger**, **thirst**, **stamina** with HUD bars
- Low-poly stylized starter clearing (procedural colored primitives: ground, trees, rocks, pond)
- Ground loot: axe, pickaxe, knife, plastic bottle, dirty water, raw food, sticks, stones
- Inventory list + interaction prompt
- Campfire craft/place → light → cook raw food → boil dirty water
- Survive loop: scavenge → manage needs → cook/boil → keep exploring

## Project layout

```
Assets/
  Editor/           Freehold menu helpers
  Scenes/           StarterArea.unity
  Scripts/
    Player/         Controller, camera, interaction
    Needs/          Hunger / thirst / stamina
    Items/          Types, pickups, inventory
    Crafting/       Campfire place + cook/boil
    UI/             Needs HUD + prompts
    World/          Bootstrap + starter area builder
Packages/           ugui + built-in modules (Built-in RP)
ProjectSettings/    Unity 6 (6000.0.32f1) project config
```

## Intentionally deferred

Housing, cars, guns, economy, multiplayer, clans, land plots — not in this slice.

## Notes for contributors

- Freehold standardizes on **Unity 6** (`6000.0.32f1`).
- Art is intentionally primitive-colored meshes so there is **no Asset Store dependency**.
- Staying on **Built-in RP** — Standard shader materials, no URP conversion.
- Input uses the legacy Input Manager (`activeInputHandler: 0`); New Input System is not required for this slice.
- Git LFS is not required for this scaffold (no large binary assets yet).
- After first local open, commit any Unity-regenerated `.meta` / settings diffs if your team wants shared GUIDs.
