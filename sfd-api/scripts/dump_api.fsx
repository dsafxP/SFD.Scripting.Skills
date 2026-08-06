#!/usr/bin/env -S dotnet fsi --

// Dumps the entire public API surface of the proprietary
// SFD.GameScriptInterface.dll assembly into a JSON snapshot, so a navigation
// script (query_api.fsx) can answer symbol lookups without re-reflecting.
//
// This assembly is self-contained (no external references) and defines only
// abstractions, so reflection alone is enough — no decompilation needed.
//
// Usage:
//   dotnet fsi scripts/dump_api.fsx -f <SFD.GameScriptInterface.dll> [-o <output.json>]
//
//   -f, --file   Path to the assembly to dump (required)
//   -o, --output  Where to write the JSON snapshot (default: /tmp/sfd_api_dump.json)
//   -h, --help   Show this help and exit

open System
open System.IO
open System.Reflection
open System.Text.Json

// Argument parsing

let printUsage () =
    printfn "Usage: dump_api.fsx -f <SFD.GameScriptInterface.dll> [-o <output>] [-h]"
    printfn ""
    printfn "  -f, --file    Path to the assembly DLL to dump (required)"
    printfn "  -o, --output  Output JSON path (default: /tmp/sfd_api_dump.json)"
    printfn "  -h, --help    Show this help and exit"

type ParsedArgs =
    { File: string option
      Output: string option
      Help: bool }

let parseArgs (args: string list) =
    let rec go acc = function
        | [] -> acc
        | ("-f" | "--file") :: value :: rest -> go { acc with File = Some value } rest
        | ("-o" | "--output") :: value :: rest -> go { acc with Output = Some value } rest
        | ("-h" | "--help") :: rest -> go { acc with Help = true } rest
        | token :: rest ->
            eprintfn "Warning: ignoring unrecognized argument '%s'" token
            go acc rest
    go { File = None; Output = None; Help = false } args

let parsed =
    fsi.CommandLineArgs
    |> List.ofArray
    |> List.skip 1
    |> parseArgs

if parsed.Help then
    printUsage ()
    exit 0

let dllPath =
    match parsed.File with
    | Some p -> p
    | None ->
        eprintfn "Error: no assembly provided. Use -f <path-to-SFD.GameScriptInterface.dll>"
        exit 1

// Type-name rendering (stack-qualified so the assistant can resolve references)

let rec renderType (t: Type) =
    if isNull t then ""
    elif t.IsGenericParameter then t.Name
    elif t.IsByRef then renderType (t.GetElementType())
    elif t.IsPointer then renderType (t.GetElementType()) + "*"
    elif t.IsArray then
        let rank = String(',', t.GetArrayRank() - 1)
        renderType (t.GetElementType()) + "[" + rank + "]"
    elif t.IsGenericType then
        let def = t.GetGenericTypeDefinition()
        let args = t.GetGenericArguments() |> Array.map renderType |> String.concat ", "
        let ns = if String.IsNullOrEmpty(def.Namespace) then "" else def.Namespace + "."
        let bare =
            let nm = def.Name
            let i = nm.IndexOf('`')
            if i >= 0 then nm.Substring(0, i) else nm
        ns + bare + "<" + args + ">"
    else
        if String.IsNullOrEmpty(t.Namespace) then t.Name else t.Namespace + "." + t.Name

let accessOf (mi: MemberInfo) =
    match mi with
    | :? MethodBase as m ->
        if m.IsPublic then "public"
        elif m.IsFamily then "protected"
        elif m.IsFamilyOrAssembly then "protected internal"
        elif m.IsAssembly then "internal"
        elif m.IsFamilyAndAssembly then "private protected"
        else "private"
    | :? FieldInfo as f ->
        if f.IsPublic then "public"
        elif f.IsFamily then "protected"
        elif f.IsFamilyOrAssembly then "protected internal"
        elif f.IsAssembly then "internal"
        elif f.IsFamilyAndAssembly then "private protected"
        else "private"
    | _ -> "public"

// DTOs

type ParamJson = {
    name: string
    ptype: string
    isRef: bool
    isOut: bool
    isOptional: bool
    defaultValue: string }

type MethodJson = {
    name: string
    returnType: string
    parameters: ParamJson list
    isStatic: bool
    isVirtual: bool
    isAbstract: bool
    isOverride: bool
    isExtension: bool
    access: string }

type PropertyJson = {
    name: string
    ptype: string
    canGet: bool
    canSet: bool
    isStatic: bool
    isIndexer: bool }

type FieldJson = {
    name: string
    ftype: string
    isStatic: bool
    isConst: bool
    access: string }

type EventJson = {
    name: string
    handlerType: string }

type EnumJson = {
    name: string
    value: int64 }

type TypeJson = {
    ``namespace``: string
    name: string
    kind: string
    baseType: string
    interfaces: string list
    genericParams: string list
    isAbstract: bool
    isStatic: bool
    enums: EnumJson list
    methods: MethodJson list
    properties: PropertyJson list
    fields: FieldJson list
    events: EventJson list }

type AssemblyJson = {
    name: string
    version: string
    dllPath: string }

type RootJson = {
    assembly: AssemblyJson
    types: TypeJson list }

// Extraction

let defaultParamValue (p: ParameterInfo) =
    try
        if p.HasDefaultValue
           && not (isNull p.DefaultValue)
           && not (p.DefaultValue :? DBNull) then
            sprintf "%A" p.DefaultValue
        else ""
    with _ -> ""

let paramJson (p: ParameterInfo) =
    let pt = p.ParameterType
    let baseT = if pt.IsByRef then pt.GetElementType() else pt
    { name = if isNull p.Name then "" else p.Name
      ptype = renderType baseT
      isRef = pt.IsByRef && not p.IsOut && not p.IsIn
      isOut = p.IsOut
      isOptional = p.IsOptional
      defaultValue = defaultParamValue p }

let methodJson (m: MethodInfo) =
    let isOverride =
        try m.IsVirtual && (not (isNull (m.GetBaseDefinition()))) && m.GetBaseDefinition().DeclaringType <> m.DeclaringType
        with _ -> false
    { name = m.Name
      returnType = renderType m.ReturnType
      parameters = m.GetParameters() |> Array.map paramJson |> Array.toList
      isStatic = m.IsStatic
      isVirtual = m.IsVirtual
      isAbstract = m.IsAbstract
      isOverride = isOverride
      isExtension = m.IsDefined(typeof<System.Runtime.CompilerServices.ExtensionAttribute>, false)
      access = accessOf m }

let withoutIndexerName (name: string) =
    name.Replace("\u0020", "").Replace(".", "_").Replace("Item", "this[]")

let propertyJson (p: PropertyInfo) =
    { name = if p.GetIndexParameters().Length > 0 then "this[]" else p.Name
      ptype = renderType p.PropertyType
      canGet = not (isNull (p.GetGetMethod()))
      canSet = not (isNull (p.GetSetMethod()))
      isStatic = let gm = p.GetGetMethod() in (not (isNull gm) && gm.IsStatic)
                || let sm = p.GetSetMethod() in (not (isNull sm) && sm.IsStatic)
      isIndexer = p.GetIndexParameters().Length > 0 }

let fieldJson (f: FieldInfo) =
    { name = f.Name
      ftype = renderType f.FieldType
      isStatic = f.IsStatic
      isConst = f.IsLiteral
      access = accessOf f }

let eventJson (e: EventInfo) =
    { name = e.Name
      handlerType = renderType e.EventHandlerType }

let enumValues (t: Type) =
    let values = Enum.GetValues(t)
    [ for i in 0 .. values.Length - 1 ->
        { name = Enum.GetName(t, values.GetValue(i))
          value = Convert.ToInt64(values.GetValue(i)) } ]

let kindOf (t: Type) =
    if t.IsEnum then "enum"
    elif not (isNull t.BaseType) && t.BaseType = typeof<System.MulticastDelegate> then "delegate"
    elif t.IsInterface then "interface"
    elif t.IsValueType then "struct"
    else "class"

let typeJson (t: Type) =
    { ``namespace`` = if isNull t.Namespace then "" else t.Namespace
      name = t.Name
      kind = kindOf t
      baseType = renderType t.BaseType
      interfaces = t.GetInterfaces() |> Array.map renderType |> Array.distinct |> Array.toList
      genericParams = t.GetGenericArguments() |> Array.filter (fun a -> a.IsGenericParameter) |> Array.map renderType |> Array.toList
      isAbstract = t.IsAbstract
      isStatic = t.IsAbstract && t.IsSealed
      enums = if t.IsEnum then enumValues t else []
      methods =
        t.GetMethods(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly)
        |> Array.filter (fun m ->
            let n = m.Name
            not (m.IsSpecialName && (n.StartsWith("get_", StringComparison.Ordinal) || n.StartsWith("set_", StringComparison.Ordinal))))
        |> Array.map methodJson |> Array.toList
      properties = t.GetProperties(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly) |> Array.map propertyJson |> Array.toList
      fields = t.GetFields(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly) |> Array.map fieldJson |> Array.toList
      events = t.GetEvents(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly) |> Array.map eventJson |> Array.toList }

// Entry

let outputPath = defaultArg parsed.Output "/tmp/sfd_api_dump.json"

try
    let asm = Assembly.LoadFrom(dllPath)
    let asmName = asm.GetName()
    let types = asm.GetExportedTypes() |> Array.map typeJson |> Array.toList

    let root =
        { assembly =
            { name = asmName.Name
              version = string asmName.Version
              dllPath = Path.GetFullPath dllPath }
          types = types }

    let opts = JsonSerializerOptions(WriteIndented = true)
    let json = JsonSerializer.Serialize(root, opts)
    File.WriteAllText(outputPath, json)
    printfn "Dumped %d type(s) from '%s' (%s) to '%s'" types.Length asmName.Name (string asmName.Version) outputPath
with
| :? ReflectionTypeLoadException as ex ->
    eprintfn "Error: failed to load candidate types from %s" dllPath
    for l in ex.LoaderExceptions do
        eprintfn "  %s" (if isNull l then "" else l.Message)
    exit 1
| ex ->
    eprintfn "Error: %s" ex.Message
    exit 1