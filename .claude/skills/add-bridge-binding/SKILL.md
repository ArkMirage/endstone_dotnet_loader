---
name: add-bridge-binding
description: Add a new native bridge function binding across the C++/C# boundary in the endstone_dotnet_loader project. Use when adding a new API method that must be callable from C# (e.g. exposing a new endstone::Player/Server/Block/... method to the managed side). Covers the 5 coordinated edits (C++ struct field, C++ implementation, C++ table binding, C# table field, optional C# wrapper) and the naming/type-mapping rules that keep the three declarations in sync.
---

# Add a Bridge Binding

This project exposes native Endstone APIs to C# through a **function-pointer table** (`BridgeTable`). Every callable API is declared in **three coordinated places** that must stay in sync:

1. `src/cpp/include/bridge.h` — the `struct BridgeTable` field declaration (the ABI contract).
2. `src/cpp/bridge.cpp` — the implementation function **and** its binding in `getBridgeTable()`.
3. `src/csharp/Bridge.cs` — the `Bridge.Table` field declaration (read by offset, so order matters).

Optionally a 4th place: the C# wrapper class (e.g. `Player.cs`) that exposes the call to plugin authors.

> **Critical**: `Bridge.cs` reads the table **by field offset**, so the field **order** in `Bridge.cs` must exactly match `bridge.h`. Never reorder existing fields; only append new ones in the same relative position in both files.

## The 5-Step Workflow

To add a new API (example: expose `endstone::Player::getFoo()` returning a `std::string`):

### Step 1 — Declare the field in `bridge.h`

Find the module section (see [Module Map](#module-map)) and add the field **in the correct section**, keeping the same relative position as its C# counterpart.

```cpp
// ---- player ----
const char *(*player_get_foo)(void *);
```

### Step 2 — Write the implementation in `bridge.cpp`

Add the function in the matching `// ---- player ----` section of `bridge.cpp`, using the existing helpers (`asPlayer`, `strOut`, etc.).

```cpp
const char *playerGetFoo(void *p) { return strOut(asPlayer(p)->getFoo()); }
```

### Step 3 — Bind it in `getBridgeTable()`

Add the designated-initializer line in `getBridgeTable()` (in `bridge.cpp`), in the same section.

```cpp
.player_get_foo = &playerGetFoo,
```

### Step 4 — Declare the field in `Bridge.cs`

Add the matching field in `Bridge.Table`, in the same section and **same relative position**.

```csharp
public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetFoo;
```

### Step 5 — (Optional) Add the C# wrapper

Expose it on the wrapper class (e.g. `Player.cs`).

```csharp
public string Foo => Bridge.Str(T->PlayerGetFoo(_ptr));
```

## Naming Rules

The three names are derived from the **C++ implementation function** (camelCase):

| Place | Example | Rule |
|---|---|---|
| C++ impl function | `playerGetFoo` | camelCase, `{module}{Action}{Name}` |
| C++ struct field | `player_get_foo` | snake_case of the impl function |
| C# table field | `PlayerGetFoo` | PascalCase of the impl function |

Conversion: `playerGetFoo` → snake_case `player_get_foo`; → PascalCase `PlayerGetFoo`.

## Type Mapping (C++ → C# `delegate*`)

| C++ type | C# `delegate*` type |
|---|---|
| `void` | `void` |
| `void *` | `void*` |
| `const void *` | `void*` |
| `const char *` | `byte*` |
| `const char **` | `byte**` |
| `bool` | `bool` |
| `int` | `int` |
| `float` | `float` |
| `int64_t` | `long` |
| `uint64_t` | `ulong` |
| `uint32_t` | `uint` |
| `int8_t` | `sbyte` |
| `uint8_t *` | `byte*` |
| `const float *` | `float*` |
| `void **` | `void**` |
| `int *` | `int*` |
| `uint64_t *` | `ulong*` |

The first parameter is always the receiver object pointer (`void *`), except for static/registry functions (e.g. `enchant_get_by_id`, `item_stack_create`, `permission_create`) which take their arguments directly.

## Module Map

Find which section a field belongs to by its prefix:

| Prefix | Section in `bridge.h` / `bridge.cpp` / `Bridge.cs` |
|---|---|
| `player_` | `// ---- player ----` |
| `server_` | `// ---- server ----` |
| `plugin_manager_` | `// ---- plugin manager ----` |
| `plugin_` | `// ---- plugin ----` |
| `event_`, `chat_`, `command_`, `server_cmd_`, `move_`, `actor_tp_`, `interact_`, `actor_damage_`, `actor_explode_`, `actor_knockback_`, `death_`, `bed_`, `dim_change_`, `drop_`, `emote_`, `gm_change_`, `consume_`, `held_`, `join_`, `quit_`, `kick_`, `login_`, `pickup_`, `skin_change_`, `cook_`, `block_explode_`, `grow_`, `from_to_`, `piston_`, `place_`, `chunk_`, `broadcast_`, `packet_`, `plugin_event_`, `script_`, `ping_`, `server_load_`, `thunder_change_`, `weather_change_` | `// ---- events: ... ----` |
| `actor_`, `mob_`, `dimension_` | `// ---- objects: actor / mob ----` |
| `item_`, `item_actor_`, `nbt_`, `block_`, `block_state_`, `damage_source_` | `// ---- objects: item / block / damage source ----` |
| `enchant_` | `// ---- objects: enchantment ----` |
| `sender_` | `// ---- objects: sender ----` |
| `form_` | `// ---- objects: form ----` |
| `boss_bar_` | `// ---- objects: boss bar ----` |
| `level_` | `// ---- objects: level ----` |
| `dimension_` | `// ---- objects: dimension ----` |
| `chunk_obj_`, `item_stack_` | `// ---- objects: chunk / item stack ----` |
| `map_`, `canvas_`, `map_renderer_` | `// ---- objects: map ----` |
| `inventory_` | `// ---- objects: inventory ----` |
| `scheduler_`, `task_` | `// ---- scheduler ----` |
| `service_` | `// ---- service manager ----` |
| `permission_` | `// ---- objects: permission ----` |
| `attachment_` | `// ---- objects: permission attachment ----` |
| `attachment_info_` | `// ---- objects: permission attachment info ----` |
| `scoreboard_`, `objective_`, `score_` | `// ---- objects: scoreboard ----` |
| `ban_list_`, `ban_entry_`, `player_ban_`, `ip_ban_` | `// ---- objects: ban list ----` |

## C# Wrapper Call Patterns

When adding the C# wrapper (Step 5), use the existing helper methods in `Bridge.cs` for string marshalling:

| Pattern | Example |
|---|---|
| Return string | `Bridge.Str(T->PlayerGetFoo(_ptr))` |
| Pass string arg | `Bridge.Call1(T->PlayerSetFoo, _ptr, value)` |
| Pass 2 string args | `Bridge.Call2(T->PlayerSetFooBar, _ptr, a, b)` |
| String + bool | `Bridge.CallBoolStr(...)` |
| String + int | `Bridge.CallIntStr(...)` |
| Event kind arg | `Bridge.CallKindPtr(...)` / `CallKindBool` / `CallKindInt` / `CallKindStr` |
| Return pointer | `T->PlayerGetFoo(_ptr)` (cast to wrapper) |

## Verification Checklist

After making all edits, **run the repo's check script** from a PowerShell terminal:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/check_bridge.ps1
```

It verifies all three invariants that keep the ABI safe:

1. **Field count parity** — `bridge.h` and `Bridge.cs` have the same number of fields.
2. **Bidirectional name parity** — every C++ field name (converted to PascalCase) exists in `Bridge.cs`, and vice versa. Both directions are compared in PascalCase canonical form, because the reverse (PascalCase → snake_case) is ambiguous for acronyms (e.g. `YAt` could come from `y_at` or `yat`).
3. **Order parity** — the full field sequence matches, because `Bridge.cs` reads the table by offset.

It prints a final `RESULT: PASS` / `RESULT: FAIL` line and always exits 0 — read the printed result, don't rely on the exit code. On failure it lists the exact mismatches (missing/extra names, and the first out-of-order index with both sides' names).

## Common Pitfalls

- **Forgetting one of the 3 declarations** — always do all of Steps 1, 3, 4 together.
- **Reordering fields** — `Bridge.cs` reads by offset; reordering breaks the ABI silently. Only append.
- **Wrong type mapping** — e.g. `const char*` must be `byte*`, not `string`, in the `delegate*` signature.
- **Wrong module section** — use the [Module Map](#module-map) to place the field in the correct section.
- **Two fields on one line** — in `bridge.h`, some declarations share a line (e.g. `plugin_manager_is_plugin_enabled` and `plugin_manager_enable_plugin`). When adding near these, keep them on separate lines.
