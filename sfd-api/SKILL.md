---
name: sfd-api
description: >
  Use this whenever the user wants to write or implement the code of a Superfighters
  Deluxe (SFD) script mod. Trigger whenever the task touches the in-game scripting API:
  the `Game` singleton (`IGame`), `Game.Events`, `IPlayer`, `IObject`, `IGameEventsHandler`,
  callbacks/events, the `OnStartup`/`AfterStartup`/`OnShutdown` hooks, or any gameplay
  logic run inside a script. Trigger even if they don't name the API outright — if they are
  "adding a feature to the SFD mod", "hooking an event", "getting a player/object", or
  "using Game" this skill applies. This skill covers navigating the script API and knowing
  its types. It does NOT cover scaffolding/building a script project — that belongs to the
  sfd-api-template skill — and it does NOT provide worked examples — those belong to the
  sfd-api-examples skill.
---

# SFD Script API (sfd-api)

## Orientation

An SFD script is C# written as `public partial class GameScript : GameScriptInterfaceExtended`
(plus any extra types nested inside it), running against the game's proprietary script API
inside the `Game` singleton. The coding model is very similar to 2D game development: you
have a game world, entities (players, objects, projectiles), and an event system.

**Constrained environment.** The script engine has access only to the script API plus core
.NET (collections, math, strings, the CLR). It does **not** have XNA or FNA, even though
the game itself is built with them. Never use external framework types (`Microsoft.Xna.*`,
`Microsoft.FNA.*`, or any non-core library) — they will not compile or run. Stick strictly
to the types the script API exposes and low-level .NET.

A script is **not** a desktop or console application, so console/desktop facilities are
unavailable. `Console.WriteLine` and similar do not work. Whenever the script API offers a
capability that roughly matches a `System.*` counterpart, **prefer the script API's version
by default** — e.g. use `Game.WriteToConsole(...)` rather than `Console.WriteLine(...)`.

## The golden rule: verify against the scripts, never invent

This is an **obscure, proprietary API** — there is no public documentation and no training
data for you to draw on. You do **not** natively know its types, methods, parameters, or
signatures. Any method, type, parameter, property, or enum value you write into code that
you have not *explicitly verified* against the dump is **made up**, and made-up API usage
will not compile.

Therefore:

- **Never invent or guess** any API member or signature. Ever. Not for "simple" scripts,
  not for obvious-sounding method names like `Game.GetPlayers()` or `Game.Events.StartX`.
  Every name you use must be confirmed to exist, with its real signature.
- **Look things up BEFORE writing code, not after it fails to compile.** The workflow is
  query → verify → then write. Checking the dump only after a build error is backwards and
  wastes time.
- **Only write a member once you have verified it**, either (a) from the dump via the
  `query_api.fsx` scripts below, or (b) it is already present verbatim in the current
  project's context (existing code you can see). Neither counts as "invented".
- If you cannot verify a needed capability exists, say so and find the closest real,
  verified member — do not fabricate one.

## The `Game` singleton (`IGame`) — the hub

`Game` (an `IGame`) is the most important type in the API. It is your handle to the whole
world: players, objects, areas, state, effects, storage, and events. If you need something
about the game, it is very likely reachable from here — most commonly through **events**
and **`Get...`/`Create...` methods**. When you are unsure where a capability lives, start by
looking inside `IGame`. Everything is read through it or a value it returns.

Two broad groups stand out:
- **`Get...` / `Create...`** — retrieving or spawning things (players, objects, areas, states).
- **Events** — reacting to things happening in the world.

## `IObject` — the second most important type

Everything in the map is an `IObject`, which makes this the next type after `IGame` you'll
rely on — and it **includes players** (players are a kind of object). Most things you get
back from `Get...` calls or receive from events are objects. Notable exceptions like
`IProjectile` exist, but **`IObject(...)`-named types are always objects**.

Because their lifetime is managed by the game, treat any object reference as potentially
gone at any moment. **Before running arbitrary logic on them — especially in event
handlers — be null-safe.** Guard code with lifecycle checks first: an object can be removed
(`IsRemoved`), and a player additionally can be dead (`IsDead`). Check these before touching
an object's members, rather than assuming the reference is still valid.

## The three auto-called lifecycle methods

Every script can define these hooks; the engine calls them automatically. They are **not
part of the script API** — the engine invokes them by name — so they can be declared any
way you like, ideally as **static** functions, e.g. `public static void OnStartup()`.

| Method | Called | Good for |
| --- | --- | --- |
| `OnStartup` | While the map is loading | Startup logic **independent** of game state — e.g. registering the events you'll use for the whole script. |
| `AfterStartup` | During the first update of the map | Startup logic **dependent** on game state — e.g. touching players, objects, or anything that only exists once the world is live. |
| `OnShutdown` | When the script is deactivated, or right before a map restart | Cleanup and persisting data. |

Prefer `AfterStartup` over `OnStartup` whenever your work needs the world to be ready.

## Events

Events are the primary way to react to the game. Register an event with
`Game.Events.Start...` (e.g. `Game.Events.StartPlayerKeyInputCallback(...)`), and keep the
returned callback token so you can stop it later with `.Stop()`. The exact callback
signatures and available events live in `IGameEventsHandler` and the various
`...Callback` types — look those up with the navigation scripts rather than guessing.

**`SFDGameScriptInterface.Events` is not the same as `IGame.Events`.** They are two different
things and must not be confused:
- `SFDGameScriptInterface.Events` is the namespace that **defines the callback types**
  themselves (`Events.UpdateCallback`, `Events.PlayerDamageCallback`, etc.) — the delegate
  types holding the event handlers.
- `IGame.Events.*` is a surface that exposes **methods to start those callbacks**
  (`Game.Events.StartUpdateCallback(...)`, `Game.Events.StartPlayerDamageCallback(...)`, ...).

So `Game.Events` starts an `SFDGameScriptInterface.Events.*` callback; the naming overlap is
coincidental and they index into different parts of the API.

Events are automatically torn down when the script is disabled (e.g. on a map restart), so
explicit cleanup/stopping is **not always required**. If an event is meant to live for the
entire script, you generally don't need to `.Stop()` it or deactivate it in
`OnShutdown` — only do so when you need to end it early while the script is still running.

## Navigating the API

The scripts in this skill's `scripts/` folder (F#, run via `dotnet fsi`) let you search the
entire API surface quickly. They are for you, not the user — use them freely.

### 1. Ensure the dump exists

A one-time JSON snapshot of the whole assembly is cached at `/tmp/sfd_api_dump.json`. If it
is missing (or looks stale), regenerate it yourself. Find the game DLL — the project's
`lib/SFD.GameScriptInterface.dll` (set up by the sfd-api-template skill) normally holds a
link to it — and run:

```sh
dotnet fsi scripts/dump_api.fsx -f <path-to-SFD.GameScriptInterface.dll>
```

The default output path matches the cache location, so no `-o` is usually needed.

### 2. Query symbols

```sh
dotnet fsi scripts/query_api.fsx <mode> <value>
```

It reads the dump and prints JSON to stdout. Pick a mode by need:

| You want... | Mode |
| --- | --- |
| Everything about one type (all members + signatures) | `--type <Name>` |
| Find types/members by substring | `--find <text>` |
| Find a member by name across all types | `--member <name>` |
| List types in a namespace | `--ns <Namespace>` |
| Everything that references a type | `--where <TypeName>` |
| The full flat list of types | `--list` |
| One shorthand list of every type and member name | `--index` |

If the dump is not at the default path, pass `-i <dump.json>`.

### 3. JSON schema — read the raw output faithfully

The scripts return structured JSON. Do **not** summarize or paraphrase it from memory, and
do **not** strip/reformat it loosely: read and transcribe the fields you need exactly, then
shorten by keeping only the specific types/members relevant to your task. Every field below
is a `string` unless noted.

**`query_api.fsx` output envelope:**

```json
{
  "query":   { "mode": "--type", "value": "IGame" },
  "count":   1,
  "results": [ ... ]          // array; empty (or a "message" instead) when nothing matches
}
```

When nothing matches, the document is `{ "count": 0, "message": "no results for <mode> <value>" }`.

**Type object** (each element of `results` for `--type`):

```json
{
  "namespace":     "SFDGameScriptInterface",
  "name":          "IGame",
  "kind":          "class",            // class | interface | enum | struct | delegate
  "baseType":      "System.Object",
  "interfaces":    ["..."],
  "genericParams": ["T"],
  "isAbstract":    true,
  "isStatic":      false,
  "enums":         [ { "name": "A", "value": 0 } ],       // only for enum types
  "methods":       [ Method ],
  "properties":    [ Property ],
  "fields":        [ Field ],
  "events":        [ Event ]
}
```

**Member shapes:**

```json
Method:    { "name":"GetPlayer","returnType":"IPlayer","parameters":[ Param ],
             "isStatic":false,"isVirtual":true,"isAbstract":true,"isOverride":false,
             "isExtension":false,"access":"public" }
Param:     { "name":"id","ptype":"int","isRef":false,"isOut":false,
             "isOptional":false,"defaultValue":"" }
Property:  { "name":"TotalScore","ptype":"int","canGet":true,"canSet":false,
             "isStatic":false,"isIndexer":false }
Field:     { "name":"X","ftype":"float","isStatic":false,"isConst":false,"access":"public" }
Event:     { "name":"...","handlerType":"delegate-or-handler-type" }
```

Type names in `returnType`/`ptype`/`ftype`/`handlerType`/`baseType`/`interfaces` are
**stack-qualified** strings (e.g. `SFDGameScriptInterface.IPlayer`, `System.Collections.Generic.IList<IPlayer>`). `--find`, `--member`, `--ns`, `--where`, and `--list` return
simplified objects (type + kind + matched members, or plain name strings) — read those
`"name"`/`"type"` fields verbatim. When you copy a member into your code, reproduce the
types exactly as emitted; don't "fix" or rename them.

### 4. Verify before you write (mandatory sequence)

Use the navigation scripts to confirm every API touchpoint *before* typing it into code.
For any feature you implement, look up — at minimum — the `Game`/`IGame` members, the
`Game.Events` callback, and any `IPlayer`/`IObject`/`...Info` types involved, and copy the
verified signatures into what you write. Start with `--index`/`--list` to orient yourself,
then `--type` into `IGame`, `IGameEventsHandler`, `IPlayer`, `IObject`, `IProjectile`, and
the relevant `...Callback`/`...Info` types. A failed compile is *not* a substitute for this
step — verify first.

## Examples

Worked, end-to-end examples are deliberately **out of scope here**. For runnable/illustrated
usage, defer to the `sfd-api-examples` skill. Use this skill to identify the correct types
and members, and look there when you want to see them assembled into real code.
