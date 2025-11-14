# Area_51_v2

Area 51 v2 is a top-down stealth action prototype built with Unity. You guide a trio of quirky aliens through a guarded facility, using unique abilities to dodge enemies, collect items, and reach the extraction point before getting caught.

## Getting Started

### Requirements

- Unity 2022.3 LTS (or later 2022 LTS release)
- Windows or macOS

### Opening the Project

1. Clone or download this repository.
2. Open Unity Hub and add the folder `Area_51_v2` as an existing project.
3. Launch the project from Unity Hub.

### Running the Game

1. In the Unity Editor, open the scene `Assets/Scenes/MainMenu.unity`.
2. Press Play. The boot sequence automatically routes through the Main Menu.
3. Press any key to start a run.

### Controls

| Input                | Action                                             |
|----------------------|----------------------------------------------------|
| WASD / Arrow Keys    | Move the current leader                             |
| `E`                  | Interact (pickup, door) / Fire ability              |
| `Space`              | Switch to the next party member                     |
| `R`                  | Restart a run (only on Win/Lose screens)           |
| `M`                  | Return to Main Menu (only on Lose screen)          |

Each alien has a unique `E`-ability once in range of a valid target:

- **Collector** attaches items or keys to the squad.
- **Freezer** locks every nearby soldier or camera for ~6 seconds, preventing movement, AI updates, and sweep lights.
- **Telekinetic** drags distant items toward the squad from a safe distance.

## Project Structure

```
Assets/
 ├─ Aliens/            # Playable character scripts and assets
 ├─ Enemies/           # Enemy AI, prefabs, sprites
 ├─ Core Scripts/      # Shared systems (restart manager, scene bootstrap, camera)
 ├─ Scenes/            # Unity scenes (MainMenu, TheGame, Win, Lose)
 ├─ Screens/           # UI imagery for start/win/lose
 ├─ UI/                # UI logic, prefabs, sprites
 └─ ...
```

Notable scripts:

- `PlayableCharacter`: Base class for all controllable aliens (movement, HP, damage handling).
- `PartyManager`: Handles party leader switching, follower trailing, and camera assignment.
- `EnemyBase` and `EnemyAI`: Core enemy stats and behaviour (patrol, chase logic).
- `EnemyContactDamage`: Applies touch damage to any `IDamageable` when colliding or overlapping.
- `PlayerHealthUI`: Subscribes to the active leader’s HP updates and refreshes HUD text/bar.
- `Freezer`: Finds all `EnemyBase`/`SecurityCameraV2` in range and calls their disable hooks while applying a freeze outline.
- `RestartManager`: Global input watcher for run restarts and returning to menu.
- `InitialSceneLoader`: Ensures the game always boots into `MainMenu`.
- `AnyKeyStart`: On the main menu, starts `TheGame` when any key is pressed.

## Gameplay Overview

- You control one alien at a time. The current leader is the only member with unlocked controls.
- Interact with items and doors using `E`.
- Enemies deal 25 HP damage on contact; HP updates immediately in the HUD.
- If HP hits zero, you’re sent to the Lose scene. Press `R` there to retry or `M` to return to the menu.
- On victory, the Win scene is shown; press `R` to replay.

## Building

1. Open `File → Build Settings…`.
2. Make sure the scenes are ordered:
   1. `MainMenu`
   2. `TheGame`
   3. `Win`
   4. `Lose`
3. Select your target platform (PC, Mac & Linux Standalone recommended).
4. Click **Build** (or **Build and Run**) and choose an output folder.

## Contributing

1. Fork the repository or create a feature branch.
2. Implement your changes.
3. Run play mode smoke tests to confirm functionality.
4. Submit a pull request summarizing the update and testing performed.

## Troubleshooting

- The game skips the menu: ensure `InitialSceneLoader` and `MainMenu` are included in build settings and no other script calls `SceneManager.LoadScene` at startup.
- Restart hotkey doesn’t work: verify you’re on the Win/Lose scene and the `RestartManager` GameObject persists in play mode.
- HP doesn’t update: confirm the player prefab uses `PlayableCharacter` derivatives and `PlayerHealthUI` is present in `TheGame` scene.

For additional issues, inspect the Unity Console for errors and check the relevant script referenced above.

---

Enjoy infiltrating Area 51 (again)! 👽

