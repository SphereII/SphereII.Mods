#!/usr/bin/env python3
"""Source assertions for DropBox one-pass distribution.

This check intentionally reads only production source files. It does not inspect
GSD workflow state, planning artifacts, audit directories, or git metadata.
"""

from pathlib import Path
import re
import sys


REPO_ROOT = Path.cwd()
DROPBOX_SOURCE = REPO_ROOT / "Mods/0-SCore/Features/RemoteCrafting/Harmony/DropBoxToContainers.cs"
UTILS_SOURCE = REPO_ROOT / "Mods/0-SCore/Features/RemoteCrafting/Scripts/RemoteCraftingUtils.cs"


def read_source(path: Path) -> str:
    assert path.is_file(), f"S02 contract violated: expected source file is missing: {path.relative_to(REPO_ROOT)}"
    return path.read_text(encoding="utf-8-sig")


def assert_contains(source: str, pattern: str, message: str) -> None:
    assert re.search(pattern, source, re.MULTILINE | re.DOTALL), f"S02 contract violated: {message}"


def main() -> int:
    dropbox = read_source(DROPBOX_SOURCE)
    utils = read_source(UTILS_SOURCE)

    assert "Log.Out(" not in dropbox, "DropBox hot path must not contain Log.Out logging"

    old_distance_call = re.compile(
        r"AddToNearbyContainer\s*\(\s*primaryPlayer\s*,\s*items\s*\[\s*i\s*\]\s*,\s*distance\s*\)"
    )
    assert not old_distance_call.search(dropbox), (
        "S02 contract violated: DropBox slot loop must not call the distance-based AddToNearbyContainer overload"
    )

    get_tile_entities_call = "GetTileEntities(primaryPlayer, distance, false)"
    occurrences = dropbox.count(get_tile_entities_call)
    assert occurrences == 1, (
        "S02 contract violated: DropBox must precompute eligible containers exactly once; "
        f"found {occurrences} occurrences of {get_tile_entities_call!r}"
    )

    assert_contains(
        dropbox,
        r"var\s+tileEntities\s*=\s*RemoteCraftingUtils\.GetTileEntities\s*\(\s*primaryPlayer\s*,\s*distance\s*,\s*false\s*\)\s*;",
        "DropBox must assign the one precomputed eligible-container list before iterating slots",
    )
    assert_contains(
        dropbox,
        r"AddToNearbyContainer\s*\(\s*primaryPlayer\s*,\s*items\s*\[\s*i\s*\]\s*,\s*tileEntities\s*\)",
        "DropBox must call the precomputed-list AddToNearbyContainer overload",
    )

    assert_contains(
        utils,
        r"public\s+static\s+bool\s+AddToNearbyContainer\s*\(\s*EntityAlive\s+player\s*,\s*ItemStack\s+itemStack\s*,\s*float\s+distance\s*\)",
        "RemoteCraftingUtils must preserve the public distance-based compatibility overload",
    )
    assert_contains(
        utils,
        r"public\s+static\s+bool\s+AddToNearbyContainer\s*\(\s*EntityAlive\s+player\s*,\s*ItemStack\s+itemStack\s*,\s*IEnumerable\s*<\s*TileEntity\s*>\s+tileEntities\s*\)",
        "RemoteCraftingUtils must expose the precomputed IEnumerable<TileEntity> overload",
    )
    assert_contains(
        utils,
        r"AddToNearbyContainer\s*\(\s*player\s*,\s*itemStack\s*,\s*tileEntities\s*\)",
        "distance-based compatibility overload must delegate to the precomputed-list overload",
    )

    print("PASS: DropBox one-pass distribution, compatibility overloads, and log cleanup verified.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except AssertionError as exc:
        print(exc, file=sys.stderr)
        raise SystemExit(1)
