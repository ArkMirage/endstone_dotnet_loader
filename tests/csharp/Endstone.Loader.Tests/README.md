# Endstone.Loader.Tests

Unit tests for the managed loader (`src/csharp`).

These tests cover the **pure managed** surface only - currently `Location` and the
`Vector3Extensions` helpers. Nothing here loads the native bridge, so the tests run
without a Bedrock server or the C++ plugin.

## Running

```bash
dotnet test --project tests/csharp/Endstone.Loader.Tests/Endstone.Loader.Tests.csproj
```

The repository root has a `global.json` that selects `Microsoft.Testing.Platform` as the
test runner, which is why the SDK 10 `dotnet test --project` syntax applies (no `--`
separator needed). For the same reason the project can also be executed directly:

```bash
dotnet run --project tests/csharp/Endstone.Loader.Tests/Endstone.Loader.Tests.csproj -c Release
```

## Conventions

- `System.Numerics.Vector3` equality is **exact** (unlike the native `endstone::Vector`,
  which compares with a `1e-6` tolerance), so use the `AssertVectorNear` helper whenever a
  value passes through floating-point trigonometry.
- Tests are expected to mirror the native semantics in
  `build/_deps/endstone-src/include/endstone/level/location.h` and
  `.../util/vector.h`; when adding a wrapper member, add the test that pins the native
  behaviour it mirrors (dimension preconditions, epsilon equality, flooring, ...).

## Not covered here

Anything that crosses the native bridge (`Bridge.Table` dispatch, `Dimension.GetBlockAt`,
`Actor.Location`, event accessors, ...) needs the native loader and is out of scope for
this project.
