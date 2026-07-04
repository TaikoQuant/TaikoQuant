# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Development Commands

| Goal | Command | Notes |
|------|---------|-------|
| **Clean and build all projects** | `dotnet clean && dotnet build -c Release` | Binaries are placed under `src\<Project>\bin\Release\net10.0\`. |
| **Run the game** | `dotnet run -c Release --project src/TaikoQuant.Game/TaikoQuant.Game.csproj` | Starts the main entry point (`Program.cs`). |
| **Run a single project** | `dotnet run -c Release --project src/<ProjectName>/<ProjectName>.csproj` | Replace `<ProjectName>` with `TaikoQuant.Core`, `TaikoQuant.Rendering.Raylib`, `TaikoQuant.Audio.ManagedBass`, or `TaikoQuant.Game`. |
| **Re‑build a single project** | `dotnet build -c Release --project src/<ProjectName>/<ProjectName>.csproj` | Useful when only one library changed. |
| **Restore NuGet packages** | `dotnet restore` | Re‑downloads packages such as `ManagedBass`. |
| **Run tests (when added)** | `dotnet test -c Release` | No test projects exist now; this will discover any that are added later. |
| **Watch source changes** | `dotnet watch --project src/TaikoQuant.Game/TaikoQuant.Game.csproj run` | Auto‑restarts the game on file changes. |
| **Open workspace in VS Code** | `code .` | The repository is already a .NET workspace; you can add tasks to invoke the above commands. |
| **Play a sound effect (debug)** | ```csharp\nvar audio = new ManagedBassAudioService();\nvar s = audio.LoadSound(\"Theme/default/sounds/dong.wav\");\ns.Play();\n``` | Uses `ManagedBassAudioService`. |
| **Play a music track (debug)** | ```csharp\nvar audio = new ManagedBassAudioService();\nvar m = audio.LoadMusic(\"path/to/song.ogg\"); // OGG → ManagedBass\nm.Play();\n``` | Falls back to `SimpleMusic` if Bass fails. |

## High‑Level Architecture Overview

The solution consists of four independent .NET class‑library projects that communicate through clearly defined interfaces.

### 1. `TaikoQuant.Core`

- **Domain layer** containing gameplay concepts, data structures, and contracts.  
- Key interfaces: `IRenderer`, `IInputService`, `IAudioService`, `IScene`, `IFont`, `ITexture`.  
- Core components:  
  - `TJAParser` – reads `.tja` chart files using **Shift‑JIS (ANSI)** encoding (`Encoding.GetEncoding(932)`).  
  - `SceneManager` – holds the current `IScene` and drives the `Update` → `Draw` loop.  
  - `GameSettings`, `SongInfo`, `Note`, `TimelineObject`, etc. – represent chart and game state.  
- **Scene implementations** (`TitleScreen`, `SongSelectScene`, `DiffSelectScene`, `GamePlayScene`) reside under `TaikoQuant.Core/Scenes`. They use the services defined in this project.

### 2. `TaikoQuant.Rendering.Raylib`

- Implements `IRenderer` as `RaylibRenderer`.  
- Wraps Raylib‑cs native types (`Texture2D`, `Font`, `Color`, `Vector2`) in adapter classes `RaylibTexture` and `RaylibFont`.  
- Provides **font overload**: `LoadFont(string path, int size, int[] codepoints)` to load Japanese glyphs.  
- All drawing calls are fully‑qualified (`Raylib_cs.Raylib.Draw*`) to avoid name clashes with wrapper methods.  
- Primitive drawing helpers (rectangles, lines) are exposed through the `IRenderer` contract.

### 3. `TaikoQuant.Audio.ManagedBass`

- Implements `IAudioService`.  
- Uses **ManagedBass** for low‑latency effect samples (`BassSound`) and streaming OGG/MP3 music (`BassMusic`).  
- Falls back to `System.Media.SoundPlayer` (`SimpleSound` / `SimpleMusic`) when the native Bass library cannot load a file or when playing WAV files.  
- **Sample caching**: up to 32 simultaneous instances per sound (mirrors the reference TJAPlayer implementation).  
- Audio initialization (`Bass.Init`) occurs lazily in the constructor; failures are silently ignored (fallback path is used).  
- `LoadMusic` selects implementation based on file extension and validates `BassMusic` via `IsValid`; if invalid it logs a warning and uses the simple WAV player.

### 4. `TaikoQuant.Game`

- Entry point (`Program.cs`) that wires everything together:  
  - Loads settings (`SettingsHelper.Load`).  
  - Initializes the Raylib window and VSYNC.  
  - Instantiates `ManagedBassAudioService`, `RaylibInputService`, `RaylibRenderer`, and `SceneManager`.  
  - Runs the main loop (`while (!WindowShouldClose())`) which updates input, updates the current scene, and draws the scene.  
- No custom `Dispose` logic beyond calling each service’s `Dispose`.

## Scene Flow

| Scene | Purpose | Navigation |
|-------|---------|------------|
| `TitleScreen` | Shows game title; waits for **Enter**. | `Enter → SongSelectScene` |
| `SongSelectScene` | Scans `songs/` folder, loads TJA titles (cached), displays selectable list. Handles **Up/Down**, **Enter** (select song → `DiffSelectScene`), **Esc** (back to title). | `Enter → DiffSelectScene` |
| `DiffSelectScene` | Shows difficulty options for the chosen song (currently hard‑coded to Easy). | `Enter → GamePlayScene` |
| `GamePlayScene` | Core gameplay: parses selected chart, scrolls notes, judges input, updates score/combo, plays cue sounds (`dong.wav`, `ka.wav`). | `Esc → SongSelectScene`; end of song → `SongSelectScene` |

All scenes share a single `IRenderer` instance; UI fonts are loaded lazily during the first `Draw` call. Japanese UI text is displayed using `NotoSansCJKjp-Regular.otf` (or any font containing the required glyphs) together with the generated codepoint list.

## Asset Conventions

- **Fonts** – `Theme/default/Fonts/`. UI text uses `IRenderer.LoadFont(path, size, codepoints)`. The repository now includes `NotoSansCJKjp-Regular.otf`, which contains the needed Japanese glyphs.  
- **Sounds** – `Theme/default/sounds/`. Short effects (`dong.wav`, `ka.wav`) are loaded as **ManagedBass samples** for low latency.  
- **Music** – Any audio file referenced by a TJA chart (`.wav`, `.ogg`, `.mp3`). OGG is streamed via ManagedBass; WAV falls back to `SoundPlayer`.  
- **Charts** – `songs/` directory. Each sub‑folder contains a `.tja` chart file and its associated audio assets. The parser expects **Shift‑JIS** encoding; ensure charts are saved with that encoding.

## Extending the Codebase

- **Add a new scene**: create a class implementing `IScene`, add a corresponding entry in the `SceneType` enum, and wire navigation in `SceneManager` or the current scene.  
- **Support additional audio formats**: extend `LoadMusic` to instantiate `BassMusic` for other extensions (`.mp3`, `.flac`, etc.) or create new wrapper classes.  
- **Share font‑codepoint generation**: the `GenerateJapaneseCodepoints` helper appears in multiple UI scenes; move it to a shared utility class in `TaikoQuant.Core` to avoid duplication.  
- **Introduce unit tests**: test `TJAParser` by providing a known Shift‑JIS `.tja` file and asserting parsed metadata, note count, and branching logic. Mock `IRenderer` to verify drawing calls if needed.  

## Key Files to Know

- `src/TaikoQuant.Core/TJAParser.cs` – parsing logic and Shift‑JIS handling.  
- `src/TaikoQuant.Rendering.Raylib/RaylibRenderer.cs` – full renderer implementation and font overload.  
- `src/TaikoQuant.Audio.ManagedBass/ManagedBassAudioService.cs` – audio service, ManagedBass integration, fallback logic.  
- `src/TaikoQuant.Game/Program.cs` – main loop, service wiring, startup sequence.  
- `src/TaikoQuant.Core/Scenes/*` – UI flow and scene implementations.  

These files define the public contracts (`IRenderer`, `IAudioService`, `IInputService`, `IScene`) and the concrete implementations that compose the game. Understanding them gives a quick entry point for any future development effort.