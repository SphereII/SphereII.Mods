// Metadata-only audit of Harmony [HarmonyPatch] targets against a game build.
// Nothing is loaded or executed, so it works on any runtime. Rerun after every game update.
//
// Usage: HarmonyAudit.exe <Mod.dll> <search dir> [<search dir> ...]
//   e.g. Tools\bin\HarmonyAudit.exe 0-SCore\SCore.dll ..\7DaysToDie_Data\Managed 0_TFP_Harmony
//
// Output: BROKEN (target or injected parameter no longer matches), WARN (ambiguous overload),
// MANUAL (TargetMethod(s)/reverse patches - check by hand). Classes that patch through
// harmony.Patch(...) calls instead of attributes are not seen at all.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

if (args.Length < 2)
{
    Console.WriteLine("Usage: HarmonyAudit.exe <Mod.dll> <search dir> [<search dir> ...]");
    return 1;
}
var modPath = args[0];
var resolver = new DefaultAssemblyResolver();
resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(modPath)));
foreach (var d in args.Skip(1)) resolver.AddSearchDirectory(d);
var mod = AssemblyDefinition.ReadAssembly(modPath, new ReaderParameters { AssemblyResolver = resolver });

int ok = 0, bad = 0, manual = 0;

foreach (var type in mod.MainModule.GetTypes())
{
    var classInfo = new PatchInfo();
    bool isPatch = false;
    foreach (var a in type.CustomAttributes.Where(IsHarmonyPatchAttr)) { classInfo.Merge(a); isPatch = true; }
    var patchMethods = type.Methods.Where(m => m.IsStatic && (IsPatchName(m.Name) || m.CustomAttributes.Any(a => a.AttributeType.Name is "HarmonyPrefix" or "HarmonyPostfix" or "HarmonyTranspiler" or "HarmonyFinalizer"))).ToList();
    if (!isPatch) continue;
    if (type.Methods.Any(m => m.Name is "TargetMethod" or "TargetMethods"))
    {
        manual++; Console.WriteLine($"MANUAL  {type.FullName}: uses TargetMethod(s), check by hand"); continue;
    }

    foreach (var pm in patchMethods.DefaultIfEmpty())
    {
        var info = classInfo.Clone();
        if (pm != null) foreach (var a in pm.CustomAttributes.Where(IsHarmonyPatchAttr)) info.Merge(a);
        if (info.Type == null) { if (pm == null) { manual++; Console.WriteLine($"MANUAL  {type.FullName}: no declaring type in attributes"); } continue; }

        TypeDefinition target;
        try { target = info.Type.Resolve(); } catch { target = null; }
        if (target == null) { bad++; Console.WriteLine($"BROKEN  {type.FullName}: type {info.Type.FullName} not found"); continue; }

        var (method, err) = FindTarget(target, info);
        if (method == null) { bad++; Console.WriteLine($"BROKEN  {type.FullName}: {err}"); continue; }
        if (err != null) Console.WriteLine($"WARN    {type.FullName}: {err}");
        if (pm == null) { ok++; continue; }

        var problems = CheckParams(pm, method, target);
        if (problems.Count == 0) ok++;
        else { bad++; foreach (var p in problems) Console.WriteLine($"BROKEN  {type.FullName}.{pm.Name}: {p}"); }
    }
}
Console.WriteLine($"== {Path.GetFileName(modPath)}: {ok} ok, {bad} broken, {manual} manual");
return bad > 0 ? 2 : 0;

static bool IsHarmonyPatchAttr(CustomAttribute a) => a.AttributeType.FullName == "HarmonyLib.HarmonyPatch";
static bool IsPatchName(string n) => n is "Prefix" or "Postfix" or "Transpiler" or "Finalizer";

static (MethodDefinition, string) FindTarget(TypeDefinition t, PatchInfo info)
{
    string desc = $"{t.FullName}.{info.Name ?? info.MethodType}" + (info.ArgTypes != null ? $"({string.Join(", ", info.ArgTypes.Select(a => a.Name))})" : "");
    for (var cur = t; cur != null; cur = SafeBase(cur))
    {
        IEnumerable<MethodDefinition> cands;
        switch (info.MethodType)
        {
            case "Getter": cands = cur.Properties.Where(p => p.Name == info.Name && p.GetMethod != null).Select(p => p.GetMethod); break;
            case "Setter": cands = cur.Properties.Where(p => p.Name == info.Name && p.SetMethod != null).Select(p => p.SetMethod); break;
            case "Constructor": cands = cur.Methods.Where(m => m.IsConstructor && !m.IsStatic); break;
            case "StaticConstructor": cands = cur.Methods.Where(m => m.IsConstructor && m.IsStatic); break;
            default:
                if (info.Name == null) return (null, $"no method name for {t.FullName}");
                cands = cur.Methods.Where(m => m.Name == info.Name);
                break;
        }
        var list = cands.ToList();
        if (info.ArgTypes != null)
        {
            var match = list.FirstOrDefault(m => m.Parameters.Count == info.ArgTypes.Length &&
                m.Parameters.Select((p, i) => Strip(p.ParameterType).FullName == Strip(info.ArgTypes[i]).FullName).All(x => x));
            if (match != null) return (match, null);
            if (list.Count > 0 || info.MethodType is "Constructor" or "StaticConstructor")
            {
                var sigs = string.Join(" | ", list.Select(m => "(" + string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name)) + ")"));
                return (null, $"overload not found: {desc}; available: {sigs}");
            }
            if (info.MethodType == "Constructor") return (null, $"ctor not found: {desc}");
        }
        else if (list.Count > 0)
            return (list[0], list.Count > 1 && info.MethodType == null ? $"ambiguous ({list.Count} overloads) for {desc}" : null);
    }
    return (null, $"method not found: {desc}");
}

static TypeDefinition SafeBase(TypeDefinition t) { try { return t.BaseType?.Resolve(); } catch { return null; } }
static TypeReference Strip(TypeReference t) => t is ByReferenceType b ? b.ElementType : t;

static List<string> CheckParams(MethodDefinition patch, MethodDefinition target, TypeDefinition targetType)
{
    var res = new List<string>();
    if (patch.Name == "Transpiler") return res;
    foreach (var p in patch.Parameters)
    {
        var n = p.Name; var pt = Strip(p.ParameterType);
        if (n == "__instance") { if (!Assignable(pt, target.DeclaringType)) res.Add($"__instance type {pt.FullName} not assignable from {target.DeclaringType.FullName}"); continue; }
        if (n == "__result") { if (!Assignable(pt, target.ReturnType) && !Assignable(target.ReturnType, pt)) res.Add($"__result {pt.FullName} vs return {target.ReturnType.FullName}"); continue; }
        if (n.StartsWith("___"))
        {
            var fname = n.Substring(3);
            FieldDefinition f = null;
            for (var cur = target.DeclaringType.Resolve(); cur != null && f == null; cur = SafeBase(cur)) f = cur.Fields.FirstOrDefault(x => x.Name == fname);
            if (f == null) res.Add($"field {fname} not found on {target.DeclaringType.FullName}");
            else if (!Assignable(pt, f.FieldType) && !Assignable(f.FieldType, pt)) res.Add($"field {fname} is {f.FieldType.FullName}, patch expects {pt.FullName}");
            continue;
        }
        if (n.StartsWith("__") && int.TryParse(n.Substring(2), out var idx))
        {
            if (idx >= target.Parameters.Count) res.Add($"{n} out of range ({target.Parameters.Count} params)");
            else if (!Assignable(pt, Strip(target.Parameters[idx].ParameterType))) res.Add($"{n} type {pt.FullName} vs {target.Parameters[idx].ParameterType.FullName}");
            continue;
        }
        if (n is "__state" or "__args" or "__originalMethod" or "__runOriginal" or "__exception") continue;
        var op = target.Parameters.FirstOrDefault(x => x.Name == n);
        if (op == null) { res.Add($"parameter '{n}' not on target ({string.Join(", ", target.Parameters.Select(x => x.ParameterType.Name + " " + x.Name))})"); continue; }
        var ot = Strip(op.ParameterType);
        if (!Assignable(pt, ot)) res.Add($"parameter '{n}' is {ot.FullName}, patch expects {pt.FullName}");
    }
    return res;
}

static bool Assignable(TypeReference to, TypeReference from)
{
    if (to.FullName == from.FullName || to.FullName == "System.Object") return true;
    if (to.IsGenericParameter || from.IsGenericParameter) return true;
    TypeDefinition fd, td;
    try { fd = from.Resolve(); td = to.Resolve(); } catch { return true; }
    if (fd == null || td == null) return true;
    if (fd.IsEnum && td.IsEnum) return true;
    if (fd.IsValueType && td.IsValueType && fd.FullName == td.FullName) return true;
    for (var cur = fd; cur != null; cur = SafeBase(cur))
    {
        if (cur.FullName == td.FullName) return true;
        if (cur.Interfaces.Any(i => i.InterfaceType.Resolve()?.FullName == td.FullName)) return true;
    }
    // closed generics: compare definitions
    var tdef = to is GenericInstanceType gt ? gt.ElementType.FullName : to.FullName;
    var fdef = from is GenericInstanceType gf ? gf.ElementType.FullName : from.FullName;
    return tdef == fdef;
}

class PatchInfo
{
    public TypeReference Type; public string Name; public TypeReference[] ArgTypes; public string MethodType;
    public PatchInfo Clone() => (PatchInfo)MemberwiseClone();
    public void Merge(CustomAttribute a)
    {
        foreach (var arg in a.ConstructorArguments)
        {
            var v = arg.Value;
            if (v is TypeReference tr) Type = tr;
            else if (v is string s) Name = s;
            else if (v is CustomAttributeArgument[] arr && arr.Length > 0 && arr[0].Value is TypeReference) ArgTypes = arr.Select(x => (TypeReference)x.Value).ToArray();
            else if (v is CustomAttributeArgument[] arr2 && arr2.Length == 0 && arg.Type.FullName == "System.Type[]") ArgTypes = Array.Empty<TypeReference>();
            else if (arg.Type.FullName == "HarmonyLib.MethodType") MethodType = ((int)v) switch { 1 => "Getter", 2 => "Setter", 3 => "Constructor", 4 => "StaticConstructor", 5 => "Enumerator", _ => null };
        }
    }
}
