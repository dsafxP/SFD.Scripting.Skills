---
name: sfd-api-examples
description: >
  Use this whenever the user is building a Superfighters Deluxe (SFD) script mod and wants a
  worked, end-to-end example of how to implement a feature. Trigger on requests like "how do
  I spawn mines", "make a drone", "listen for player damage/input/melee/creation", "raycast",
  "joints", "storage", "fire/burning", "player modifiers", "victory condition", "change
  outfit", "give/remove items", "ammo", "create buttons", "visual debugging", or any general
  "give me an SFD script example". This skill is a library of vetted, ready-made script
  examples consulted by topic. It does NOT cover navigating the API types/signatures — that
  belongs to the sfd-api skill — and it does NOT cover scaffolding/building a script project —
  that belongs to the sfd-api-template skill.
---

# SFD Script Examples (sfd-api-examples)

## Orientation

This skill is a compendium of **worked, end-to-end script examples**, one per file under
`references/`. The examples are **authoritative** — they have been vetted and reflect real,
working usage of the script API. Use them to see how a feature is assembled, and adapt the
code to your project.

The examples follow the same conventions as the `sfd-api` skill: code lives nested inside
`public partial class GameScript : GameScriptInterfaceExtended`, lifecycle hooks are static,
and the script API is preferred over `System` (e.g. `Game.WriteToConsole` not
`Console.WriteLine`). No XNA/FNA — only the script API plus core .NET.

## How to use

1. **Identify the goal.** Determine what feature the user wants, in their terms.
2. **Find the matching reference(s)** in the Topic Index below, then **read the whole file**.
3. **Adapt, don't rewrite.** Copy the closest example and reshape it to the user's needs.
   Combine files when a feature spans several topics.
4. **Produce the example in the target project's structure** (namespace, project name), not
   detached snippets. Use the scaffold from the `sfd-api-template` skill if a project doesn't
   exist yet.

## Core references — read first

Start with these when learning the shape of a script or building anything non-trivial:

- `script_lifecycle_events.md` — the `OnStartup`/`AfterStartup`/`OnShutdown` hooks and how a script is structured.
- `events_overview.md` — the full catalogue of script events and their handler signatures.
- `visual_debugging.md` — rendering debug output (`DrawLine`, `DrawCircle`, `DrawArea`, `DrawText`) used throughout the examples.

## Topic index

| If the user wants to... | Read |
| --- | --- |
| Lifecycle hooks (`OnStartup`/`AfterStartup`/`OnShutdown`) | `script_lifecycle_events.md` |
| Register/handle any event (overview of all events) | `events_overview.md` |
| Listen for player damage / death | `listening_on_player_dmg.md` |
| Listen for player keyboard input | `listening_on_player_input.md` |
| React to player melee actions | `listening_on_player_melee_act.md` |
| React to player creation / termination | `listening_on_player_creation_and_termination.md` |
| React to object creation / damage / termination | `listening_on_obj_creation_dmg_termination.md` |
| React to explosions | `listening_on_explosions.md` |
| React to user join / leave | `listening_on_user_join_leave.md` |
| Give / remove player items or ammo | `give_remove_player_items.md`, `ammo_management.md` |
| Apply/read player modifiers | `player_modifiers.md` |
| Change a player's outfit | `change_player_outfit.md` |
| Manipulate projectiles | `projectile_manipulation.md` |
| Spawn mines / drone-like objects | `camping_mines.md` |
| Fire / burning behavior | `fire.md` |
| Create joints between objects | `joints.md` |
| Create buttons through script | `creating_buttons.md` |
| RayCast / HitTest in the world | `raycast_hittest.md` |
| Persist/load data across runs | `storage_testbed.md` |
| Implement a victory condition | `victory_condition.md` |
| Draw visual debugging output | `visual_debugging.md` |

## Notes

- Where a topic has multiple files, consult all listed ones — they cover complementary cases.
- The examples are complete and compilable starting points; if an example touches a member
  you are unsure about, cross-check its signature with the `sfd-api` skill — but keep the
  example's usage as written, since these are authoritative.