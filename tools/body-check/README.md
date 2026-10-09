Run the isolated body-spawning checks with:

```powershell
dotnet run --project tools/body-check/BodyCheck.csproj
```

The harness compiles the production zombie and corpse spawners against small game/Unity substitutes. It checks organ selection, template and collar rules, preview totals, native corpse metadata, world placement, fresh bodies, zombie registration, failed drops and invalid game state. It does not replace an in-game check of rendering, pickup, autopsy, resurrection or save/load.
