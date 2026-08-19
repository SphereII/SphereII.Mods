The Weapon Camera Sway feature suppresses the idle motion of the first person camera and the
held weapon, for players who find it uncomfortable or who simply want the weapon to sit where
they are aiming.

Four pieces of motion are covered, stopped at the source rather than scaled down: camera sway,
camera bob, weapon sway and weapon bob.

## Control and Configuration

This is a console command:

```
weaponsway true     sway OFF
weaponsway false    sway ON
```

**Mind the sense of the parameter.** It says whether to *suppress* the motion, not whether to
have it, so `weaponsway true` is the one that turns sway off. The wording is inherited from the
cvar this writes and is kept for compatibility rather than because it reads well.

Type it into the console of the machine it should affect. It acts on the local player only and
is never sent to anyone else, so two people in the same game can set it independently.

### The cvar

The command writes `$WeaponSway` on the local player:

| Value | Meaning |
| ----- | ------- |
| absent | SCore default - vanilla sway runs |
| `0` | sway on |
| `1` or higher | sway suppressed |

`SwayUtilities.CanSway` reads it and returns whether the original routine should run. Anything
else that can set a cvar - a buff, a quest, a `MinEvent` - can drive the feature the same way.

### Persistence

**"Sway on" does not survive a reload.** `EntityBuffs.Write` skips zero valued cvars on the save
path (the condition is `_netSync || CVars[key] != 0f`), and sway-on is stored as zero, so it
reverts to the default when the save is loaded. Sway-off, being `1`, does persist. Re-run the
command after loading if you want the motion back.

Nothing is net-synced either: `SetCustomVar` only sends when the entity is remote or the cvar
name begins with `%`, and neither applies to the local player setting this on themselves.

## What is patched

Harmony prefixes over four methods, each returning `SwayUtilities.CanSway()` so a `false` result
skips the original:

- `vp_FPCamera.UpdateSwaying`, `vp_FPCamera.UpdateBob`
- `vp_FPWeapon.UpdateSwaying`, `vp_FPWeapon.UpdateBob`

`CanSway` calls `World.GetPrimaryPlayer()` on every one of those, every frame. That is a plain
field read (`return m_LocalPlayerEntity;`), so it needs no caching.

## The standalone modlet

`SphereII Disable Sway` ships this same feature outside SCore, so that someone who wants only
this can have it without installing SCore. The duplication is intentional. Two things follow
from it:

- **Same cvar, same polarity.** Both copies patch the same four methods, and Harmony skips the
  original if *any* prefix returns false, so opposite readings of `$WeaponSway` would leave the
  command unable to turn motion back on whenever both are installed.
- **Only one console command survives.** `SdtdConsole.RegisterCommand` keeps the first
  registration and drops the rest with `Command with name "weaponsway" already loaded, not
  loading from class ConsoleCmdWeaponSway`. Which one wins is assembly enumeration order, so
  the wording has to match in both.

The one deliberate difference is the default. SCore starts from vanilla sway and lets the cvar
switch it off; the standalone starts with sway already suppressed, so for it an absent cvar
means no sway and `weaponsway false` is what brings the motion back.

Keep the two copies in step - a change to either belongs in both, in the same edit.
