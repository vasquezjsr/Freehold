# Freehold

**Claim your ground. Make your own way.**

Stylized low-poly Unity life sandbox. This repo ships the **first playable survival scavenging slice**: explore a small starter area, manage hunger/thirst/stamina, pick up tools and supplies, place a campfire, cook food, and boil water.

## Unity version

- **Expected editor:** Unity **2022.3 LTS** (project stamped as `2022.3.50f1`)
- Built-in Render Pipeline (no URP/HDRP package required for this slice)
- Open with Unity Hub → **Add** → select this repo folder → open with 2022.3 LTS

> Cloud agents cannot run the Unity Editor. Scripts, scene, and project settings are authored so you can open locally and press Play.

## Open & play

1. Install [Unity Hub](https://unity.com/download) and **Unity 2022.3 LTS**.
2. Clone this repository.
3. In Hub: **Add** → choose the repo root (contains `Assets/`, `Packages/`, `ProjectSettings/`).
4. Open the project (let Unity import; it will generate `.meta` files / `.csproj` on first open).
5. Open `Assets/Scenes/StarterArea.unity` (or use menu **Freehold → Open Starter Area Scene**).
6. Press **Play**. `GameBootstrap` builds the world, player, camera, loot, and HUD at runtime.

Optional: **Freehold → Validate Survival Slice** checks that core scripts and the scene are present.

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
Packages/           Built-in modules only
ProjectSettings/    Unity 2022.3 project config
```

## Intentionally deferred

Housing, cars, guns, economy, multiplayer, clans, land plots — not in this slice.

## Notes for contributors

- Art is intentionally primitive-colored meshes so there is **no Asset Store dependency**.
- Git LFS is not required for this scaffold (no large binary assets yet). Add LFS later if you commit FBX/textures/audio.
- After first local open, commit generated `.meta` files if your team wants stable GUIDs across machines.
