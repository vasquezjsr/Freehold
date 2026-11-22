# Freehold

**Claim your ground. Make your own way.**

Stylized low-poly Unity life sandbox. Playable survival scavenging slice on **`main`**: explore, manage hunger/thirst/stamina, scavenge pack-based loot, place a campfire, cook, and boil water.

## Unity version

- **Expected editor:** Unity **6** — **`6000.0.32f1`**
- **Render pipeline:** Built-in RP (no URP/HDRP)

## Update an existing Hub project (preferred)

If Freehold is already added in Unity Hub:

1. Close the Unity Editor (optional but avoids import fights).
2. In the project folder: `git checkout main && git pull origin main`
3. Re-open the same Hub project (do **not** re-Add / re-clone).
4. Let Unity reimport new `Assets/ThirdParty` / `Resources` assets.
5. Open `Assets/Scenes/StarterArea.unity` → **Play**.

## First-time Hub open

1. Install Unity Hub + **6000.0.32f1**.
2. `git clone https://github.com/vasquezjsr/Freehold.git`
3. Hub → **Projects** → **Add** → select the clone root (`Assets/`, `Packages/`, `ProjectSettings/`).
4. Open with **6000.0.32f1**, wait for import, Play `StarterArea`.

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look (click game view to lock; Esc unlocks) |
| Left Shift | Sprint |
| Space | Jump |
| E | Pickup / light campfire / cook / boil |
| C | Place campfire (3 sticks) |
| R / T | Eat cooked / drink clean water |
| F / Y | Eat raw / drink dirty water |

## Art packs (CC0, vendored)

See `Assets/ThirdParty/THIRD_PARTY_NOTICES.md`.

| Pack | License | Link | Used for |
| --- | --- | --- | --- |
| **Kenney Nature Kit** | CC0 | [OpenGameArt](https://opengameart.org/content/nature-kit) / [kenney.nl](https://kenney.nl) | Trees, rocks, grass, logs, campfire stones/logs, water feature |
| **Quaternius Ultimate RPG Items** | CC0 | [OpenGameArt](https://opengameart.org/content/low-poly-rpg-pack) | Axe, dagger (knife), hammer (pickaxe), potions (bottles), food |
| **Vuena Free Low-Poly Characters** | CC0 | [GitHub](https://github.com/Vuena/free-low-poly-characters-cc0) | Blocky hero FBX + idle/walk/run/jump clips |

Gameplay still uses Freehold systems (needs, inventory, campfire cook/boil). Procedural meshes remain only as **fallback** if a pack file fails to load.

## What the slice includes

- Third-person pack hero (Vuena) with move/sprint/jump + pack animation bridge
- Needs HUD: hunger, thirst, stamina
- Kenney nature starter clearing + Quaternius loot props
- Campfire place/light/cook/boil loop

## Project layout

```
Assets/
  ThirdParty/       Vendored CC0 packs + notices
  Resources/        Characters/Hero.fbx (runtime load)
  Scripts/Art/Pack/ Obj loader + pack spawners
  Scripts/…         Player, Needs, Items, Crafting, UI, World
  Scenes/StarterArea.unity
```

## Intentionally deferred

Housing, cars, guns, economy, multiplayer, clans, land plots.

## Notes

- Prefer staying on **`main`** for day-to-day Hub work (`git pull`).
- Built-in RP only; lighting/fog unchanged by the asset-pack PR.
- After first import of new ThirdParty assets, commit Unity-generated `.meta` files if your team wants shared GUIDs.
