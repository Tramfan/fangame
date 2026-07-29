# Fangame development guidance

## Project

- This is a Unity 6000.5.4f1 bullet-hell fan-game prototype.
- The active prototype scene is `Assets/Scenes/Prototype.unity`.
- Prototype scripts belong in `Assets/Scripts/Prototype`.
- The current enemy bullet prefab is `Assets/Prefabs/prototype/EnemyBullet.prefab`.
- Preserve the existing Menu and Game scenes unless the user explicitly asks to change them.

## Collaboration

- The user is learning Unity and C# while building the game.
- Communicate with the user in Russian. Keep code identifiers in English.
- Before editing, briefly explain which files you intend to change and why.
- After editing, explain the important code and list any required Unity Inspector steps.
- Do not assume that successful static inspection means the feature works in Unity. Ask the user to test it in Play Mode.
- Prefer small, understandable implementations over premature universal frameworks.
- Do not rewrite working systems merely to make them more sophisticated.

## Unity files

- Do not modify `Library`, `Temp`, `Logs`, `obj`, or generated IDE files.
- Do not directly edit `.unity`, `.prefab`, or `.meta` YAML unless the user explicitly requests it.
- Never regenerate or duplicate Unity `.meta` GUIDs.
- Prefer giving the user Inspector instructions for scene and prefab changes.
- Do not add, remove, or update Unity packages without explicit approval.

## Current prototype rules

- The player's true hitbox collider is always active.
- Focus mode only changes movement speed and hitbox renderer visibility.
- `GrazeArea` is separate from `Hitbox`.
- One enemy bullet can award graze only once.
- Bullet patterns and bullet movement should remain separate responsibilities.
- Existing bullets currently use straight-line movement.
- Already fired bullets may remain after an emitter or attack phase is disabled.

## C# style

- Keep one primary class per file.
- Use descriptive English names.
- Prefer private fields with `[SerializeField]` when values should be adjustable in the Inspector.
- Avoid unnecessary global state and scene searches.
- Handle missing Inspector references with clear errors.
- Keep formatting and indentation consistent with the existing prototype scripts.

## Git

- Preserve unrelated and uncommitted user changes.
- Do not commit, push, amend, reset, revert, or delete files unless explicitly asked.
- Group changes into meaningful gameplay milestones rather than tiny commits.
- Before reporting completion, inspect `git diff` and `git status`.
- Never use destructive Git commands.

## Verification

- Check edited C# files for obvious compile errors.
- Run `git diff --check` when appropriate.
- Report what was verified statically and what still requires Unity Play Mode testing.
