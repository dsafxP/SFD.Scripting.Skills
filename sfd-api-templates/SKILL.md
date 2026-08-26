---
name: sfd-api-template
description: >
  Use this whenever the user wants to create, start, scaffold, build, or set up a
  Superfighters Deluxe script (a C# mod loaded in-game as a single .txt file).
  Trigger on any of: "SFD", "Superfighters Deluxe", "SFD script", "extension script",
  "GameScript", "script mod", or "make/write/create a script for SFD" — even if they
  don't mention a template. This skill handles scaffolding and building a new SFD script
  project from the SFDScript template. It does NOT cover the SFD scripting API itself —
  for how to write code against the API, defer to the sfd-api skill when `Game`,
  `Game.Events`, `IPlayer`, commands, or other API usage is involved.
---

# SFD Script Template (sfd-api-template)

## Orientation

A Superfighters Deluxe (SFD) script is a C# mod written against the game's proprietary
`SFD.GameScriptInterface.dll` API. You develop it as a normal .NET 8 project, then "weld"
all the C# source files into a single non-compilable `*.txt` file that the game loads as a
script. This skill is about creating and building that project from the SFDScript template
— not about the SFD API itself. If the user's goal is really to write gameplay/API code,
scaffold the project with this skill and then defer to the `sfd-api` skill for the code.

## Prerequisites — do NOT install these yourself

These are environmental, not code, and must be handled by the user. Do not attempt to do
them with your own commands or permissions.

- **SFDScript template installed.** It is a local .NET template, NOT a NuGet package, and
  cannot be reliably installed by an assistant. The user must run
  `dotnet new install SFDScript` themselves. Verify it exists with
  `dotnet new list sfd-script`; if it's not there, ask the user to install it and wait.
- **.NET 8 SDK** (`dotnet --version`) and **F#** with `dotnet fsi` available (the bundled
  tooling runs as F# scripts).

Do **not** go hunting for the Superfighters Deluxe installation or the proprietary DLL
yourself — LibrarySetup locates it automatically (it checks the well-known Steam paths),
so none of that checking is needed up front.

## Create the project

```sh
dotnet new sfd-script -n <ProjectName>
```

This produces a project with roughly this layout:

```
<ProjectName>/
├── <ProjectName>.csproj   # DLL reference + CheckSdk & GenerateScript build targets
├── Program.cs             # base class + empty GameScript partial
├── lib/                   # where the game DLL gets symlinked/copied (populated next)
└── tools/                 # bundled SFD.ScriptTools scripts + their README
```

### What the pieces are

- **`Program.cs`** defines `GameScriptInterfaceExtended : GameScriptInterface` exposing a
  `protected static readonly IGame Game` singleton, and `public partial class GameScript :
  GameScriptInterfaceExtended`.
- **`<ProjectName>.csproj`** references `SFD.GameScriptInterface.dll` by `HintPath` (default
  `lib/`, overridable), runs a `CheckSdk` validation before resolving references, and has a
  `GenerateScript` target that welds the source files into the loadable `.txt`.

### The one rule that matters for the welder

Every `.cs` file that contributes to the script must:

- declare `public partial class GameScript : GameScriptInterfaceExtended`, and
- nest its members **inside** that class (this is what lets them all see `Game`).

`using` directives are **not allowed** in the welded output, but that's not a burden you
need to think about for the obvious cases: implicit .NET usings (`System`, collections,
etc.) and `SFDGameScriptInterface` are already available implicitly, so write them plainly —
**don't** fully-qualify `System.String` or `SFDGameScriptInterface.IGame`. The rule only
bites for anything *outside* those, which is the same set that would normally need a new
`using`. For that, prefer fully qualifying over adding another `using` directive. If the
generator refuses to run because a file has `using` directives, replace them by
fully-qualifying only those non-implicit references. You can group feature files into
folders (`Utils/`, `Events/`, etc.) — every `@(Compile)` file is welded in.

## Essential manual step — LibrarySetup

If the project will live in a git repository, create a proper `.gitignore` **first** (e.g.
`dotnet new gitignore`), then run LibrarySetup afterwards so the DLL it links gets ignored
by that gitignore. Skipping this risks the proprietary DLL being committed.

Then make the proprietary DLL available locally:

```sh
dotnet fsi tools/SFD.ScriptTools.LibrarySetup.fsx
```

This locates `SFD.GameScriptInterface.dll` in your SFD installation (checking the usual
Steam locations automatically), symlinks (or copies) it into `lib/`, adds it to
`.gitignore` so it's never committed, and pins its version into
`<RequiredGameSdkVersion>` in the `.csproj`. If the pinned version ever mismatches the
installed DLL the build will fail; pass `--force` to update it, or `--dry-run` to check
without touching anything.

Normally LibrarySetup finds the DLL on its own — don't do any locating before running it.
Only if LibrarySetup **fails to find the DLL** should you try locating
`SFD.GameScriptInterface.dll` manually and point it at the path explicitly:

```sh
dotnet fsi tools/SFD.ScriptTools.LibrarySetup.fsx --file <path-to-dll>
```

## Build and generate the script

A plain build is usually all you need:

```sh
dotnet build
```

Compiling alone validates your C# against the script API. **Do not generate the script by
default** — generating (`dotnet build -t:GenerateScript`) welds all `.cs` files into the
loadable `<ProjectName>.txt` and takes longer than a plain build, so it is only worth doing
when the user actually asks for the final script. Default to `dotnet build`; reserve the
`-t:GenerateScript` weld for when the user wants the actual `.txt` output

The weld writes the script to the project output directory as `<ProjectName>.txt` — the
file you load in Superfighters Deluxe. The tooling may emit harmless warnings during these
steps — ignore them rather than trying to fix them. Do **not** bother inspecting or
verifying the generated `.txt` output — the weld is deterministic and not your concern; the
build succeeding is the only signal you need.

## Everything else — read the bundled README

`tools/README.md` is shipped inside the template and documents the remaining bundled
tooling. Whenever a task touches anything beyond LibrarySetup and the build/generate
workflow (e.g. the event-name migration helper), read that README and follow it rather than
guessing at the tools.

## Troubleshooting

| Symptom | Cause / fix |
| --- | --- |
| Generator errors mentioning `using` | A contributing `.cs` file has a non-allowed `using`; fully-qualify only the non-implicit references it introduces. |
| Build fails on SDK version | The installed DLL version differs from `<RequiredGameSdkVersion>`; run LibrarySetup with `--force`. |
| DLL missing in `lib/` / build can't find it | LibrarySetup wasn't run, or the DLL isn't committed (it's `gitignore`d by design — run LibrarySetup on each fresh checkout). |
| `sfd-script` template not found | User has not run `dotnet new install SFDScript`; ask them to. |
