#!/usr/bin/env python3
"""Source-level contract verifier for remote-crafting eligible-container cache.

This intentionally reads only source files under Mods/0-SCore/Features/RemoteCrafting.
It is used when the 7 Days to Die runtime / .NET Framework references are not
available, so failures should name the missing compatibility or cache contract.
"""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
UTILS_PATH = ROOT / "Scripts" / "RemoteCraftingUtils.cs"
DROPBOX_PATH = ROOT / "Harmony" / "DropBoxToContainers.cs"


def read_source(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8-sig")
    except OSError as exc:
        raise AssertionError(f"Unable to read required source file {path}: {exc}") from exc


def assert_regex(source: str, pattern: str, message: str, flags: int = re.MULTILINE | re.DOTALL) -> re.Match[str]:
    match = re.search(pattern, source, flags)
    if not match:
        raise AssertionError(message)
    return match


def method_body(source: str, signature_pattern: str, method_name: str) -> str:
    signature = assert_regex(source, signature_pattern, f"Missing method contract for {method_name}", re.MULTILINE)
    brace_start = source.find("{", signature.end())
    if brace_start == -1:
        raise AssertionError(f"Unable to locate body for {method_name}")

    depth = 0
    for index in range(brace_start, len(source)):
        char = source[index]
        if char == "{":
            depth += 1
        elif char == "}":
            depth -= 1
            if depth == 0:
                return source[brace_start : index + 1]

    raise AssertionError(f"Unable to parse complete body for {method_name}")


def assert_public_compatibility_contract(utils_source: str) -> None:
    contracts = [
        (
            r"public\s+static\s+List\s*<\s*TileEntity\s*>\s+GetTileEntities\s*\(\s*EntityAlive\s+player\s*\)",
            "Missing public GetTileEntities(EntityAlive) compatibility wrapper",
        ),
        (
            r"public\s+static\s+List\s*<\s*TileEntity\s*>\s+GetTileEntities\s*\(\s*EntityAlive\s+player\s*,\s*float\s+distance\s*,\s*bool\s+forRepairs\s*\)",
            "Missing public GetTileEntities(EntityAlive,float,bool) cache-backed overload",
        ),
        (
            r"public\s+static\s+List\s*<\s*ItemStack\s*>\s+SearchNearbyContainers\s*\(\s*EntityAlive\s+player\s*\)",
            "Missing public SearchNearbyContainers(EntityAlive) overload",
        ),
        (
            r"public\s+static\s+List\s*<\s*ItemStack\s*>\s+SearchNearbyContainers\s*\(\s*EntityAlive\s+player\s*,\s*ItemValue\s+itemValue\s*\)",
            "Missing public SearchNearbyContainers(EntityAlive,ItemValue) overload",
        ),
        (
            r"public\s+static\s+List\s*<\s*ItemStack\s*>\s+SearchNearbyContainers\s*\(\s*EntityAlive\s+player\s*,\s*ItemValue\s+itemValue\s*,\s*float\s+distance\s*\)",
            "Missing public SearchNearbyContainers(EntityAlive,ItemValue,float) overload",
        ),
        (
            r"public\s+static\s+bool\s+AddToNearbyContainer\s*\(\s*EntityAlive\s+player\s*,\s*ItemStack\s+itemStack\s*,\s*float\s+distance\s*\)",
            "Missing public AddToNearbyContainer(EntityAlive,ItemStack,float) compatibility wrapper",
        ),
    ]
    for pattern, message in contracts:
        assert_regex(utils_source, pattern, message)

    wrapper_body = method_body(
        utils_source,
        r"public\s+static\s+bool\s+AddToNearbyContainer\s*\(\s*EntityAlive\s+player\s*,\s*ItemStack\s+itemStack\s*,\s*float\s+distance\s*\)",
        "AddToNearbyContainer(EntityAlive,ItemStack,float)",
    )
    if "GetTileEntities(player, distance, false)" not in wrapper_body:
        raise AssertionError("AddToNearbyContainer(EntityAlive,ItemStack,float) no longer uses GetTileEntities(player,distance,false)")
    if "AddToNearbyContainer(player, itemStack, tileEntities)" not in wrapper_body:
        raise AssertionError("AddToNearbyContainer(EntityAlive,ItemStack,float) no longer delegates to tile-entity overload")


def assert_cache_contract(utils_source: str) -> None:
    ttl_match = assert_regex(
        utils_source,
        r"private\s+const\s+int\s+NearbyContainerCacheTtlMs\s*=\s*(\d+)\s*;",
        "Missing NearbyContainerCacheTtlMs constant",
        re.MULTILINE,
    )
    ttl_ms = int(ttl_match.group(1))
    if ttl_ms < 250:
        raise AssertionError(f"NearbyContainerCacheTtlMs is too low for useful short-TTL reuse: {ttl_ms}ms < 250ms")
    if ttl_ms > 500:
        raise AssertionError(f"NearbyContainerCacheTtlMs exceeds conservative stale-state bound: {ttl_ms}ms > 500ms")

    required_anchors = [
        (r"NearbyContainerCacheEntry", "Missing single-entry cache record type"),
        (r"_nearbyContainerCache", "Missing single-entry cache storage field"),
        (r"TryGetNearbyContainerCache\s*\(", "Missing cache lookup helper"),
        (r"StoreNearbyContainerCache\s*\(", "Missing cache store helper"),
        (r"DateTime\.UtcNow\s*<=\s*cache\.ExpiresAtUtc", "Cache lookup does not enforce expiry before reuse"),
        (r"DateTime\.UtcNow\.AddMilliseconds\s*\(\s*NearbyContainerCacheTtlMs\s*\)", "Cache store does not use NearbyContainerCacheTtlMs"),
        (r"new\s+List\s*<\s*TileEntity\s*>\s*\(\s*cache\.TileEntities\s*\)", "Cache hit should return a defensive list copy"),
        (r"new\s+List\s*<\s*TileEntity\s*>\s*\(\s*tileEntities\s*\)", "Cache store should keep a defensive list copy"),
        (r"InvalidateNearbyContainerCache\s*\(\s*\)", "Missing public invalidation helper"),
    ]
    for pattern, message in required_anchors:
        assert_regex(utils_source, pattern, message)

    key_struct = method_body(
        utils_source,
        r"private\s+readonly\s+struct\s+NearbyContainerCacheKey\s*:\s*IEquatable\s*<\s*NearbyContainerCacheKey\s*>",
        "NearbyContainerCacheKey",
    )
    for anchor, message in [
        ("_playerId", "Cache key no longer tracks player identity"),
        ("_distance", "Cache key no longer tracks search distance"),
        ("_forRepairs", "Cache key no longer tracks repair/crafting mode"),
        ("_positionBucket", "Cache key no longer tracks player position bucket"),
        ("_context", "Cache key no longer tracks workstation/config context"),
    ]:
        if anchor not in key_struct:
            raise AssertionError(message)

    build_key_body = method_body(
        utils_source,
        r"private\s+static\s+bool\s+TryBuildNearbyContainerCacheKey\s*\(",
        "TryBuildNearbyContainerCacheKey",
    )
    for anchor, message in [
        ("player.entityId", "Cache key builder must use player identity"),
        ("distance", "Cache key builder must include search distance"),
        ("forRepairs", "Cache key builder must include repair/crafting mode"),
        ("player.GetPosition()", "Cache key builder must read player position"),
        ("NearbyContainerCachePositionBucketSize", "Cache key builder must bucket player movement"),
        ("Mathf.FloorToInt", "Cache key builder must use stable position bucket rounding"),
        ("GetCurrentWorkstation", "Cache key builder must include current workstation context where available"),
        ("landClaimContainersOnly", "Cache key context must include land-claim container mode"),
        ("landClaimPlayerOnly", "Cache key context must include land-claim player mode"),
        ("disabledsender", "Cache key context must include disablesender config"),
        ("nottoWorkstation", "Cache key context must include nottoWorkstation config"),
        ("bindtoWorkstation", "Cache key context must include bindtoWorkstation config"),
        ("Invertdisable", "Cache key context must include Invertdisable config"),
        ("enforcebindtoWorkstation", "Cache key context must include enforcebindtoWorkstation config"),
    ]:
        if anchor not in build_key_body:
            raise AssertionError(message)

    get_entities_body = method_body(
        utils_source,
        r"public\s+static\s+List\s*<\s*TileEntity\s*>\s+GetTileEntities\s*\(\s*EntityAlive\s+player\s*,\s*float\s+distance\s*,\s*bool\s+forRepairs\s*\)",
        "GetTileEntities(EntityAlive,float,bool)",
    )
    for anchor, message in [
        ("TryBuildNearbyContainerCacheKey", "GetTileEntities(EntityAlive,float,bool) does not build cache key"),
        ("TryGetNearbyContainerCache", "GetTileEntities(EntityAlive,float,bool) does not attempt cache reuse"),
        ("return cachedTileEntities", "GetTileEntities(EntityAlive,float,bool) does not return cache hits"),
        ("StoreNearbyContainerCache", "GetTileEntities(EntityAlive,float,bool) does not store scan results"),
    ]:
        if anchor not in get_entities_body:
            raise AssertionError(message)


def assert_hot_path_context_contract(utils_source: str) -> None:
    context_type = method_body(
        utils_source,
        r"private\s+sealed\s+class\s+NearbyContainerScanContext",
        "NearbyContainerScanContext",
    )
    for anchor, message in [
        ("LandClaimContainersOnly", "Scan context no longer tracks land-claim container filtering"),
        ("LandClaimPlayerOnly", "Scan context no longer tracks land-claim player filtering"),
        ("DisabledSenderRaw", "Scan context no longer tracks disablesender cache context"),
        ("NotToWorkstationRaw", "Scan context no longer tracks nottoWorkstation cache context"),
        ("BindToWorkstationRaw", "Scan context no longer tracks bindtoWorkstation cache context"),
        ("CurrentWorkstation", "Scan context no longer tracks current workstation"),
        ("HasLocalWorkstationContext", "Scan context no longer tracks local workstation availability"),
        ("CanUseNearbyContainerCache", "Scan context no longer tracks cache eligibility"),
        ("InvertDisable", "Scan context no longer tracks Invertdisable"),
        ("EnforceBindToWorkstation", "Scan context no longer tracks enforcebindtoWorkstation"),
        ("DisabledSenderValues", "Scan context no longer stores parsed disablesender values"),
        ("NotToWorkstationBindings", "Scan context no longer stores parsed nottoWorkstation bindings"),
        ("BindToWorkstationBindings", "Scan context no longer stores parsed bindtoWorkstation bindings"),
    ]:
        if anchor not in context_type:
            raise AssertionError(message)

    build_context_body = method_body(
        utils_source,
        r"private\s+static\s+NearbyContainerScanContext\s+BuildNearbyContainerScanContext\s*\(",
        "BuildNearbyContainerScanContext",
    )
    for anchor, message in [
        ('Configuration.GetPropertyValue(AdvFeatureClass, "disablesender")', "BuildNearbyContainerScanContext must read disablesender once per scan"),
        ('Configuration.GetPropertyValue(AdvFeatureClass, "nottoWorkstation")', "BuildNearbyContainerScanContext must read nottoWorkstation once per scan"),
        ('Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation")', "BuildNearbyContainerScanContext must read bindtoWorkstation once per scan"),
        ('bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Invertdisable"))', "BuildNearbyContainerScanContext must parse Invertdisable once per scan"),
        ('bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "enforcebindtoWorkstation"))', "BuildNearbyContainerScanContext must parse enforcebindtoWorkstation once per scan"),
        ('currentWorkstation = GetCurrentWorkstation(playerLocal);', "BuildNearbyContainerScanContext must resolve current workstation once for local players"),
        ('DisabledSenderValues = SplitCsv(disabledSenderRaw)', "BuildNearbyContainerScanContext must pre-split disablesender values"),
        ('NotToWorkstationBindings = ParseWorkstationBindings(notToWorkstationRaw, true)', "BuildNearbyContainerScanContext must pre-parse nottoWorkstation bindings"),
        ('BindToWorkstationBindings = ParseWorkstationBindings(bindToWorkstationRaw, false)', "BuildNearbyContainerScanContext must pre-parse bindtoWorkstation bindings"),
        ('CanUseNearbyContainerCache = canUseNearbyContainerCache', "BuildNearbyContainerScanContext must carry cache eligibility into the scan context"),
    ]:
        if anchor not in build_context_body:
            raise AssertionError(message)

    get_entities_body = method_body(
        utils_source,
        r"public\s+static\s+List\s*<\s*TileEntity\s*>\s+GetTileEntities\s*\(\s*EntityAlive\s+player\s*,\s*float\s+distance\s*,\s*bool\s+forRepairs\s*\)",
        "GetTileEntities(EntityAlive,float,bool)",
    )
    scan_context_anchor = "BuildNearbyContainerScanContext(player, landClaimContainersOnly, landClaimPlayerOnly)"
    if scan_context_anchor not in get_entities_body:
        raise AssertionError("GetTileEntities(EntityAlive,float,bool) no longer builds the scan context once per scan")
    if "ShouldIncludeTileEntity(scanContext, tileEntity)" not in get_entities_body:
        raise AssertionError("GetTileEntities(EntityAlive,float,bool) no longer routes candidate filtering through the scan context helper")
    if get_entities_body.index(scan_context_anchor) > get_entities_body.index("TryBuildNearbyContainerCacheKey"):
        raise AssertionError("GetTileEntities(EntityAlive,float,bool) must build scan context before cache-key construction")

    should_include_body = method_body(
        utils_source,
        r"private\s+static\s+bool\s+ShouldIncludeTileEntity\s*\(",
        "ShouldIncludeTileEntity",
    )
    for anchor, message in [
        ("scanContext.DisabledSenderValues.Length > 0", "ShouldIncludeTileEntity must use pre-split disablesender values"),
        ("DisableSender(scanContext.DisabledSenderValues, scanContext.InvertDisable, tileEntity)", "ShouldIncludeTileEntity must use the context-aware DisableSender overload"),
        ("NotToWorkstation(scanContext, tileEntity)", "ShouldIncludeTileEntity must use the context-aware nottoWorkstation helper"),
        ("BindToWorkstation(scanContext, tileEntity)", "ShouldIncludeTileEntity must use the context-aware bindtoWorkstation helper"),
    ]:
        if anchor not in should_include_body:
            raise AssertionError(message)
    for forbidden, message in [
        ("Configuration.GetPropertyValue", "ShouldIncludeTileEntity must not re-read configuration inside the candidate loop"),
        ("GetCurrentWorkstation", "ShouldIncludeTileEntity must not re-resolve workstation inside the candidate loop"),
    ]:
        if forbidden in should_include_body:
            raise AssertionError(message)

    public_disable_sender_body = method_body(
        utils_source,
        r"public\s+static\s+bool\s+DisableSender\s*\(\s*IEnumerable\s*<\s*string\s*>\s+value\s*,\s*ITileEntity\s+tileEntity\s*\)",
        "DisableSender(IEnumerable<string>,ITileEntity)",
    )
    for anchor, message in [
        ("value as string[] ?? value.ToArray()", "Public DisableSender wrapper must preserve IEnumerable compatibility"),
        ('bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Invertdisable"))', "Public DisableSender wrapper must preserve Invertdisable behavior"),
        ("DisableSender(disableSenderValues, invertdisable, tileEntity)", "Public DisableSender wrapper must delegate to the internal overload"),
    ]:
        if anchor not in public_disable_sender_body:
            raise AssertionError(message)

    assert_regex(
        utils_source,
        r"private\s+static\s+bool\s+DisableSender\s*\(\s*IReadOnlyCollection\s*<\s*string\s*>\s+value\s*,\s*bool\s+invertDisable\s*,\s*ITileEntity\s+tileEntity\s*\)",
        "Missing internal DisableSender overload for pre-parsed values",
    )

    for pattern, message in [
        (
            r"private\s+static\s+bool\s+BindToWorkstation\s*\(\s*NearbyContainerScanContext\s+scanContext\s*,\s*TileEntity\s+tileEntity\s*\).*?scanContext\.HasLocalWorkstationContext.*?scanContext\.BindToWorkstationBindings.*?scanContext\.CurrentWorkstation",
            "BindToWorkstation must use scan-context workstation bindings and current workstation",
        ),
        (
            r"private\s+static\s+bool\s+NotToWorkstation\s*\(\s*NearbyContainerScanContext\s+scanContext\s*,\s*TileEntity\s+tileEntity\s*\).*?scanContext\.HasLocalWorkstationContext.*?scanContext\.NotToWorkstationBindings.*?scanContext\.CurrentWorkstation",
            "NotToWorkstation must use scan-context workstation bindings and current workstation",
        ),
    ]:
        assert_regex(utils_source, pattern, message)



def assert_decrement_loop_contract(utils_source: str) -> None:
    consume_body = method_body(
        utils_source,
        r"public\s+static\s+void\s+ConsumeItem\s*\(",
        "ConsumeItem",
    )
    for anchor, message in [
        ("var removedCount = Math.Min(item.count, num);", "ConsumeItem must cap container removal by the remaining requirement"),
        ("item.count -= removedCount;", "ConsumeItem must subtract the bounded removal amount from the slot"),
        ("num -= removedCount;", "ConsumeItem must subtract the bounded removal amount from the remaining requirement"),
        ("_removedItems.Add(new ItemStack(item.itemValue.Clone(), removedCount));", "ConsumeItem must record the actual removed amount"),
    ]:
        if anchor not in consume_body:
            raise AssertionError(message)

    for forbidden, message in [
        ("while (num >= 0)", "ConsumeItem must not use the off-by-one decrement loop"),
        ("item.count--;", "ConsumeItem must not decrement one item at a time in the partial-removal branch"),
        ("num--;", "ConsumeItem must not decrement the remaining requirement one item at a time in the partial-removal branch"),
    ]:
        if forbidden in consume_body:
            raise AssertionError(message)

    if "else\n                        {\n                            // Otherwise, let's just count down until we meet the requirement." in consume_body:
        raise AssertionError("ConsumeItem still contains the legacy partial-removal decrement-loop branch")



def assert_invalidation_contract(utils_source: str) -> None:
    check_tile_entity_body = method_body(
        utils_source,
        r"private\s+static\s+bool\s+CheckTileEntity\s*\(",
        "CheckTileEntity",
    )
    for anchor, message in [
        ("changed = true", "CheckTileEntity must track successful mutation before invalidating"),
        ("finally", "CheckTileEntity must use finally to restore container access state"),
        ("lootTileEntity.SetModified();", "CheckTileEntity successful mutation must mark container modified"),
        ("InvalidateNearbyContainerCache();", "CheckTileEntity successful mutation must invalidate eligible-container cache"),
        ("lootTileEntity.SetUserAccessing(false);", "CheckTileEntity must release container access state"),
    ]:
        if anchor not in check_tile_entity_body:
            raise AssertionError(message)

    if not re.search(r"if\s*\(\s*changed\s*\)\s*\{[^{}]*lootTileEntity\.SetModified\s*\(\s*\)\s*;[^{}]*InvalidateNearbyContainerCache\s*\(\s*\)\s*;", check_tile_entity_body, re.DOTALL):
        raise AssertionError("CheckTileEntity invalidation must be inside the successful-mutation changed branch")

    consume_body = method_body(
        utils_source,
        r"public\s+static\s+void\s+ConsumeItem\s*\(",
        "ConsumeItem",
    )
    for anchor, message in [
        ("containerChanged = true", "ConsumeItem must track successful container slot updates"),
        ("lootTileEntity.UpdateSlot", "ConsumeItem must update mutated container slots"),
        ("lootTileEntity.SetModified();", "ConsumeItem successful mutation must mark container modified"),
        ("InvalidateNearbyContainerCache();", "ConsumeItem successful mutation must invalidate eligible-container cache"),
    ]:
        if anchor not in consume_body:
            raise AssertionError(message)

    if not re.search(r"if\s*\(\s*containerChanged\s*\)\s*\{[^{}]*lootTileEntity\.SetModified\s*\(\s*\)\s*;[^{}]*InvalidateNearbyContainerCache\s*\(\s*\)\s*;", consume_body, re.DOTALL):
        raise AssertionError("ConsumeItem invalidation must be inside the successful containerChanged mutation branch")


def assert_dropbox_logging_guard(dropbox_source: str) -> None:
    log_out_lines = [
        line_no
        for line_no, line in enumerate(dropbox_source.splitlines(), start=1)
        if "Log.Out" in line and not line.lstrip().startswith("//")
    ]
    if log_out_lines:
        raise AssertionError(
            "DropBoxToContainers.cs contains normal-path Log.Out calls at lines "
            + ", ".join(str(line_no) for line_no in log_out_lines)
        )


def main() -> None:
    utils_source = read_source(UTILS_PATH)
    dropbox_source = read_source(DROPBOX_PATH)

    assert_public_compatibility_contract(utils_source)
    assert_cache_contract(utils_source)
    assert_hot_path_context_contract(utils_source)
    assert_decrement_loop_contract(utils_source)
    assert_invalidation_contract(utils_source)
    assert_dropbox_logging_guard(dropbox_source)

    print("remote crafting cache source contract: PASS")


if __name__ == "__main__":
    main()
