#!/usr/bin/env -S dotnet fsi --

// Navigates a JSON snapshot produced by dump_api.fsx and answers symbol
// lookups over the SFD.GameScriptInterface API. Emits JSON on stdout for the
// assistant to consume.
//
// Usage:
//   dotnet fsi scripts/query_api.fsx <mode> [<value>]
//   dotnet fsi scripts/query_api.fsx -i <dump-file> <mode> [<value>]
//
//   -i, --input  Dump JSON path (default: /tmp/sfd_api_dump.json)
//
// Modes:
//   --type <Name>        Full detail of one type (all members)
//   --find <substring>   Types whose type name OR any member name contains it
//   --member <name>      Search members by name across all types
//   --ns <namespace>     List type names in a namespace (prefix match)
//   --where <TypeName>   Everything that references a type (reverse lookup)
//   --list               List all type names
//   --index              Flat list of every type and member name
//
// Output envelope: { "query": {...}, "count": n, "results": [...] } or
// { "count": 0, "message": "no results for <mode> <value>" } when nothing matches.
//
//   -h, --help  Show this help and exit

open System
open System.IO
open System.Text.Json
open System.Text.Json.Nodes

let printUsage () =
    printfn "Usage: query_api.fsx [-i <dump-file>] <mode> [<value>]"
    printfn ""
    printfn "  -i, --input    Dump JSON path (default: /tmp/sfd_api_dump.json)"
    printfn ""
    printfn "Modes:"
    printfn "  --type <Name>        Full detail of one type (all members)"
    printfn "  --find <substring>   Types whose type name OR any member name contains it"
    printfn "  --member <name>      Search members by name across all types"
    printfn "  --ns <namespace>     List type names in a namespace (prefix match)"
    printfn "  --where <TypeName>   Everything that references a type (reverse lookup)"
    printfn "  --list               List all type names"
    printfn "  --index              Flat list of every type and member name"
    printfn "  -h, --help           Show this help and exit"

// Argument parsing. The dump path can come from -i/--input, or as the first
// positional argument.

let rec parseArgs (input: string) ((mode, value): string * string) (args: string list) =
    match args with
    | [] -> input, (mode, value)
    | ("-i" | "--input") :: v :: rest -> parseArgs v (mode, value) rest
    | ("-h" | "--help") :: _ -> "", ("--help", "")
    | ("--type" | "--find" | "--member" | "--ns" | "--where") :: v :: rest ->
        parseArgs input (args.Head, v) rest
    | ("--list" | "--index") :: rest -> parseArgs input (args.Head, "") rest
    | token :: rest ->
        if mode = "" && File.Exists token then
            parseArgs token (mode, value) rest  // bare positional = dump file
        else
            eprintfn "Warning: ignoring unrecognized argument '%s'" token
            parseArgs input (mode, value) rest

let scriptArgs = fsi.CommandLineArgs |> List.ofArray |> List.skip 1
let input, (mode, value) = parseArgs "/tmp/sfd_api_dump.json" ("", "") scriptArgs

if mode = "--help" || mode = "" then
    printUsage ()
    exit 0

// Helpers over JsonNode. Note: JsonNode indexers return null for a missing key,
// so no exceptions need handling.

let getStr (n: JsonNode) (key: string) =
    match n.[key] with
    | null -> ""
    | :? JsonValue as v ->
        try v.GetValue<string>() with _ -> v.ToString()
    | other -> other.ToString()

let asArray (n: JsonNode) (key: string) =
    match n.[key] with
    | :? JsonArray as arr -> Some arr
    | _ -> None

let typeDisplay (t: JsonObject) =
    let ns = getStr t "namespace"
    let name = getStr t "name"
    if String.IsNullOrEmpty ns then name else ns + "." + name

let memberNames (o: JsonObject) (key: string) =
    match asArray o key with
    | Some arr ->
        [ for item in arr ->
            match item with
            | :? JsonObject as m -> getStr m "name"
            | _ -> "" ]
    | None -> []

let allMemberNames (t: JsonObject) =
    List.concat
        [ memberNames t "methods"
          memberNames t "properties"
          memberNames t "fields"
          memberNames t "events"
          memberNames t "enums" ]

let emit (results: JsonArray) =
    let doc = JsonObject()
    let q = JsonObject()
    q["mode"] <- JsonValue.Create(mode)
    q["value"] <- JsonValue.Create(value)
    doc["query"] <- q
    doc["count"] <- JsonValue.Create(results.Count)
    doc["results"] <- results
    printfn "%s" (doc.ToJsonString(JsonSerializerOptions(WriteIndented = true)))

let notFound () =
    let doc = JsonObject()
    doc["count"] <- JsonValue.Create(0)
    doc["message"] <- JsonValue.Create(sprintf "no results for %s %s" mode value)
    printfn "%s" (doc.ToJsonString())

let main () =
    let doc =
        try JsonNode.Parse(File.ReadAllText input)
        with ex ->
            eprintfn "Error: cannot read dump '%s': %s" input ex.Message
            exit 1

    let types =
        match doc.["types"] with
        | :? JsonArray as arr -> [ for item in arr -> item :?> JsonObject ]
        | _ ->
            eprintfn "Error: no 'types' array in '%s'" input
            exit 1

    match mode with
    | "--type" ->
        let hits =
            types
            |> List.filter (fun t ->
                String.Equals(getStr t "name", value, StringComparison.OrdinalIgnoreCase)
                || String.Equals(typeDisplay t, value, StringComparison.OrdinalIgnoreCase))
        if hits.IsEmpty then notFound ()
        else
            let arr = JsonArray()
            for t in hits do arr.Add(JsonNode.Parse(t.ToJsonString()))
            emit arr

    | "--find" ->
        let results = JsonArray()
        for t in types do
            let members = allMemberNames t
            let typeHit = (getStr t "name").Contains(value, StringComparison.OrdinalIgnoreCase)
            let matched = members |> List.filter (fun m -> m.Contains(value, StringComparison.OrdinalIgnoreCase)) |> List.distinct
            if typeHit || not matched.IsEmpty then
                let o = JsonObject()
                o["type"] <- JsonValue.Create(typeDisplay t)
                o["kind"] <- JsonValue.Create(getStr t "kind")
                let ma = JsonArray()
                for m in matched do ma.Add(JsonValue.Create m)
                o["matchedMembers"] <- ma
                results.Add(o)
        if results.Count = 0 then notFound ()
        else emit results

    | "--member" ->
        let results = JsonArray()
        for t in types do
            let matched = allMemberNames t |> List.filter (fun m -> m.Contains(value, StringComparison.OrdinalIgnoreCase)) |> List.distinct
            for m in matched do
                let o = JsonObject()
                o["containingType"] <- JsonValue.Create(typeDisplay t)
                o["name"] <- JsonValue.Create(m)
                results.Add(o)
        if results.Count = 0 then notFound ()
        else emit results

    | "--ns" ->
        let names =
            types
            |> List.filter (fun t ->
                String.IsNullOrEmpty value
                || (getStr t "namespace").StartsWith(value, StringComparison.OrdinalIgnoreCase))
            |> List.map typeDisplay
            |> List.sort
        let arr = JsonArray()
        for n in names do arr.Add(JsonValue.Create n)
        emit arr

    | "--where" ->
        // Collect every spot a type name may appear (base, interfaces, member
        // return/param/field/event types) as raw signature text.
        let referencedText (t: JsonObject) =
            let sb = System.Text.StringBuilder()
            let add (v: JsonNode) =
                match v with
                | :? JsonValue as jv -> sb.AppendLine(jv.GetValue<string>()) |> ignore
                | _ -> ()
            add t.["baseType"]
            match asArray t "interfaces" with
            | Some arr -> for i in arr do if not (isNull i) then sb.AppendLine(i.GetValue<string>()) |> ignore
            | None -> ()
            match asArray t "methods" with
            | Some arr ->
                for m in arr do
                    match m with
                    | :? JsonObject as mo ->
                        add (mo.["returnType"])
                        match asArray mo "parameters" with
                        | Some parr -> for p in parr do match p with :? JsonObject as po -> add (po.["ptype"]) | _ -> ()
                        | None -> ()
                    | _ -> ()
            | None -> ()
            match asArray t "properties" with
            | Some arr ->
                for pr in arr do
                    match pr with
                    | :? JsonObject as po -> add (po.["ptype"])
                    | _ -> ()
            | None -> ()
            match asArray t "fields" with
            | Some arr ->
                for f in arr do
                    match f with
                    | :? JsonObject as fo -> add (fo.["ftype"])
                    | _ -> ()
            | None -> ()
            match asArray t "events" with
            | Some arr ->
                for e in arr do
                    match e with
                    | :? JsonObject as eo -> add (eo.["handlerType"])
                    | _ -> ()
            | None -> ()
            sb.ToString()
        let results = JsonArray()
        for t in types do
            if (referencedText t).Contains(value, StringComparison.OrdinalIgnoreCase) then
                let o = JsonObject()
                o["type"] <- JsonValue.Create(typeDisplay t)
                results.Add(o)
        if results.Count = 0 then notFound ()
        else emit results

    | "--list" ->
        let arr = JsonArray()
        for t in types |> List.sortBy typeDisplay do arr.Add(JsonValue.Create(typeDisplay t))
        emit arr

    | "--index" ->
        let typeNames = types |> List.map typeDisplay |> List.sort
        let memberNamesAll = types |> List.collect allMemberNames |> List.distinct |> List.sort
        let o = JsonObject()
        o["typeCount"] <- JsonValue.Create(typeNames.Length)
        let ta = JsonArray()
        for n in typeNames do
            ta.Add(JsonValue.Create n) |> ignore
        o["types"] <- ta
        o["memberCount"] <- JsonValue.Create(memberNamesAll.Length)
        let ma = JsonArray()
        for n in memberNamesAll do
            ma.Add(JsonValue.Create n) |> ignore
        o["memberNames"] <- ma
        let doc = JsonObject()
        doc["query"] <- JsonValue.Create("--index")
        doc["count"] <- JsonValue.Create(1)
        doc["results"] <- o
        printfn "%s" (doc.ToJsonString(JsonSerializerOptions(WriteIndented = true)))

    | _ ->
        eprintfn "Unknown mode: %s" mode
        exit 1

main ()