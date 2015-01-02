// ----------------------------------------------------------------------------
// Helper functions
// ----------------------------------------------------------------------------

module List = 
  /// Merge two sequences by pairing elements for which
  /// the specified predicate returns the same key
  let pairBy f first second = 
    let d1 = dict [ for o in first -> f o, o ]
    let d2 = dict [ for o in second -> f o, o ]
    let keys = set (Seq.concat [ d1.Keys; d2.Keys ])
    let asOption = function true, v -> Some v | _ -> None
    [ for k in keys -> 
        k, asOption (d1.TryGetValue(k)), asOption (d2.TryGetValue(k)) ]

  /// For all element of the list, returns a list where this
  /// element is picked from the list (together with list of 
  /// all the other elements)
  let individuals list = 
    let rec loop acc list = seq {
      match list with
      | x::xs -> 
          yield x, (List.rev acc)@xs
          yield! loop (x::acc) xs
      | [] -> () }
    loop [] list

  let pickOne f list =
    let rec loop acc = function
      | x::xs -> 
          match f x with 
          | Some r -> r, (List.rev acc) @ xs
          | None -> loop (x::acc) xs
      | [] -> failwith "pickOne: No matching item found."
    loop [] list       

// ----------------------------------------------------------------------------
// RDF data model
// ----------------------------------------------------------------------------

type Value = Value of string
type Entity = Entity of string
type Relation = Relation of string

type Source = Entity
type Target = TargetVal of Value | TargetEnt of Entity
type RDF = list<Source * Relation * Target>

// Mini-DSL that makes it possible to write 'e1 <-- r --> e2' and 'e1 >-- r --> e2'
let (<--) source ctor = ctor source true 
let (>--) source ctor = ctor source false 
let (-->) relation target source bidir : list<Source * Relation * Target> = 
  [ yield source, relation, TargetEnt target 
    if bidir then 
      yield target, relation, TargetEnt source ]
let (-->>) relation target source bidir : list<Source * Relation * Target> = 
  [ yield source, relation, TargetVal (Value target)
    if bidir then failwith "Relation to value cannot be bidirectional." ]
    
// ----------------------------------------------------------------------------
// F# type model
// ----------------------------------------------------------------------------

type Type = 
  | Top
  | Bottom
  | Primitive 
  | Record of list<string * bool * Type>
  | List of Type

let rec commonSubType t1 t2 =
  match t1, t2 with
  | Primitive, Primitive -> Primitive
  | Top, other | other, Top -> other
  | List t1, List t2 -> List (commonSubType t1 t2)
  | Record r1, Record r2 ->
      Record (List.pairBy (fun (n, _, _) -> n) r1 r2 |> List.map (fun (name, fst, snd) ->
        match fst, snd with
        // If one is missing, return the other, but optional
        | Some (_, _, t), None | None, Some (_, _, t) -> (name, true, t)
        // If both are available, we merge their types
        | Some (_, o1, t1), Some (_, o2, t2) -> (name, o1 || o2, commonSubType t1 t2)
        | None, None -> failwith "Assertion: pairBy returned None, None"))
  | _ -> Bottom

let commonSubTypeOfAll = List.fold commonSubType Top

// ----------------------------------------------------------------------------
// Mapping RDF to F# types
// ----------------------------------------------------------------------------

type Mapping = Set<Relation> * Set<Relation>

/// Given a source entity and relation name, return
/// all targets that are linked to the source entity
let lookup (rdf:RDF) source rel =
  rdf |> List.choose (fun (f, r, t) ->
    if f = source && r = rel then Some t else None)

let provider (rdf:RDF) mappings = 

  /// Given a relation and a list of targets referenced by it,
  /// generates fields of a record that are produced for the relation
  let rec provideRelated ((asval, astyp) as mappings) (Relation relName as rel) targets =

    if Set.contains rel asval then
      // Turn the related entities into a value-level member
      // If there is multiple targets then emit list, otherwise value
      // (take the name of the relation and append 's' if it is collection)
      match targets with 
      | [] -> failwith "Assertion: grouping should always return non-empty lists."
      | [it] -> [ relName, false, provideTarget mappings it ]
      | its -> [ relName + "s", false, List.map (provideTarget mappings) its |> commonSubTypeOfAll |> List ]        

    elif Set.contains rel astyp then
      // Turn the related entities into type-level member(s)
      // We assume all the related entities 'x' are entities with a relation 
      // 'x --name--> String' that gives the name of the field we want to generate.
      let targetEntities = targets |> Seq.choose (function TargetEnt u -> Some u | _ -> None)
      if (Seq.length targetEntities) <> (Seq.length targets) then 
        failwithf "Cannot turn '%s' into type level as it does not refer to an entity." relName

      // For every target, get the name and generate the type
      [ for target in targetEntities -> 
          match lookup rdf target (Relation "name") with
          | [TargetVal (Value name)] -> name, false, provideNode mappings target
          | [_] -> failwithf "Cannot turn '%s' into type level because name is not a primitive value." relName
          | [] -> failwithf "Cannot turn '%s' into type level because it does not have any name." relName
          | _ -> failwithf "Cannot turn '%s' into type level because it does not have unique name." relName ]
    
    else
      // Ignore the relation
      []

  /// Returns a type for a target (which is either primitive value or a record)
  and provideTarget mappings = function
    | TargetVal _ -> Primitive
    | TargetEnt ent -> provideNode mappings ent

  /// Given a specific node in the RDF, generates a record that
  /// represents the node (by grouping relations and mapping each
  /// relation separately using 'provideRelated')
  and provideNode mappings root = 
    query { for source, relation, target in rdf do
            where (source = root)
            groupValBy target relation into targets
            select (provideRelated mappings targets.Key (List.ofSeq targets)) }
    |> Seq.concat
    |> List.ofSeq |> Record

  provideNode mappings

// ----------------------------------------------------------------------------
// Querying
// ----------------------------------------------------------------------------

// The specification of a query. Query is a list of triples that
// specify relation between variables (a graph pattern) 
type Var = Var of int
type QueryRelation = Var * Relation * Var
type Query = QueryRelation list

// The solution is formed by "paths" through the graph that 
// lead to the entities that are the result of the query
// (These are not really paths, because they can "jump back")
type Path = 
  | Path of Entity * (Relation list * Path list)

// Run a simple SPARQL-like query and return paths to results
let queryRdf (rdf:RDF) (q:Query) root = 
  let rec loop (solved:Map<Var, Entity>) = function
    | [] -> [], []
    | (q:Query) ->
        // Select a target variable from the query that we can solve
        // (meaning that we have assignments for the source variables 
        // for all relations that determine the target variable)
        let qt, determinants = 
          query { for qs, rel, qt in q do
                  groupValBy (qs, rel, qt) qt into g
                  where (g |> Seq.forall (fun (qs, _, _) -> solved.ContainsKey qs))
                  select (g.Key, List.ofSeq g) 
                  head }

        // Remove the query triples that we're using now
        let remove = set determinants
        let q = q |> List.filter (remove.Contains >> not)
       
        // For every relation that determines the solution, get the
        // assignment of the source varaible and use it to get all targets
        // Then take the intersection of the targets (meaning that 
        // we get the target that matches all current assignments)
        let targets =       
          [ for (qs, rel, qt) in determinants ->
             lookup rdf (solved.[qs]) rel |> Seq.choose (function
               | TargetEnt t -> Some t  | _ -> None) |> set ]
          |> Set.intersectMany |> List.ofSeq

        // For every target, process the rest of the query
        [ for _, r, _ in determinants -> r ],
        [ for target in targets ->
            let solved = Map.add qt target solved
            Path(target, loop solved q) ]

  // Initially, we just know the root node
  let ents = Map.ofSeq [ Var 0, root ]
  Path(root, loop ents q)

// ----------------------------------------------------------------------------
// Test querying on simple 2D WorldBank like data set

let wb2d : RDF = [
  yield! (Entity "root") >-- (Relation "ind") --> (Entity "ind_1")
  yield! (Entity "root") >-- (Relation "ind") --> (Entity "ind_2")
  yield! (Entity "root") >-- (Relation "ctr") --> (Entity "ctr_1")
  yield! (Entity "root") >-- (Relation "ctr") --> (Entity "ctr_2")
  for i, j in [1,1; 1,2; 2,1; 2,2] do
    let ind = "ind_" + string i
    let ctr = "ctr_" + string j
    let ent = "ent_" + string i + "_" + string j
    yield! (Entity ctr) >-- (Relation "val") --> (Entity ent)
    yield! (Entity ind ) >-- (Relation "val") --> (Entity ent)
  ]

// ----------------------------------------------------------------------------

let provide (rdf:RDF) (query:Query) root = 
  
  let providePaths = function 
    | [] -> []
    | [Path(ent, (rels, paths))] ->

  let path = queryRdf rdf query root
  ent, rels, paths

(* queryRdf wb2d*)
provide wb2d 
              [ Var 0, (Relation "ind"), Var 1
                Var 0, (Relation "ctr"), Var 2
                Var 1, (Relation "val"), Var 3
                Var 2, (Relation "val"), Var 3  ] (Entity "root")

// ----------------------------------------------------------------------------
// Creating RDF from databases
// ----------------------------------------------------------------------------


type Table = 
  { Columns : list<string>
    Values : list<list<string>> }

let fromDatabase table : RDF = 
  // Columns are represented as relations
  let rels = [ for s in table.Columns -> Relation s ]
  // For every row, we create entity using the primary key
  // (here just index) and add values using column relations
  let data =
    table.Values 
    |> List.mapi (fun id row ->
      [ for rel, col in List.zip rels row do
          yield! (Entity("#" + string id)) >-- rel -->> col ])
    |> List.concat
  // We add root entity that references all table values
  let rows = 
    table.Values 
    |> List.mapi (fun id _ -> 
        (Entity "root") >-- (Relation "value") --> (Entity("#" + string id)) )
    |> List.concat
  data @ rows
      
let dbsample = 
  { Columns = [ "id"; "name" ]
    Values  = [ ["1"; "tomas"]
                ["2"; "jan"  ] ] }

// ----------------------------------------------------------------------------

let _ =
  // Map everything to values gives 'list<{ id:α; name:α }>'
  let mappings : Mapping = 
    set [ Relation "value"; Relation "id"; Relation "name" ], 
    set []

  // Map rows to types gives '{ tomas:{id:α; name:α}; jan:{id:α; name:α} }'
  let mappings : Mapping =
    set [ Relation "id"; Relation "name" ], 
    set [ Relation "value" ]

  // Do the mapping
  let rdf = fromDatabase dbsample
  provider rdf mappings (Entity "root")

  queryRdf rdf [ Var 0, (Relation "value"), Var 1 ] (Entity "root")

// ----------------------------------------------------------------------------
// RDF for WorldBank-like data
// ----------------------------------------------------------------------------

let countries = [ "CZ"; "UK" ]
let indicators = [ "Population"; "GDP" ]
let years = [ "2000"; "2010" ]
let values = [| [| [| 10.0; 11.0 |]; [| 0.1; 1.0 |] |]
                [| [| 60.0; 66.0 |]; [| 0.6; 6.0 |] |] |]

// Generate relations from 'root' to all keys along all dimensions
// Key is an entity with 'name' relation specifying the actual value
let dimensions = 
  [ countries, "country", "#c";
    indicators, "indicator", "#i";
    years, "year", "#y" ]

let keys =
  [ for list, rel, prefix in dimensions do
      for i, name in List.zip [0 .. list.Length - 1] list do
        let target = Entity(prefix + (string i))
        yield! target >-- (Relation "name") -->> name
        yield! (Entity "root") >-- (Relation rel) --> target ]

let data = 
  [ for c in 0 .. countries.Length - 1 do
    for i in 0 .. indicators.Length - 1 do  
    for y in 0 .. years.Length - 1 do
      let entid = sprintf "e%d%d%d" c i y
      let value = string (values.[c].[i].[y])
      yield! (Entity entid) >-- (Relation "data") -->> value
      yield! (Entity ("#c" + string c)) >-- (Relation "value") --> (Entity entid)
      yield! (Entity ("#i" + string i)) >-- (Relation "value") --> (Entity entid)
      yield! (Entity ("#y" + string y)) >-- (Relation "value") --> (Entity entid)
  ]

let worldbank = keys @ data

// ----------------------------------------------------------------------------

let _ =
  let mappings : Mapping =
    set [Relation "name"; Relation "value"],
    set [Relation "country"]

  provider worldbank mappings (Entity "root")  

  queryRdf worldbank 
            [ Var 0, (Relation "country"), Var 1
              Var 0, (Relation "indicator"), Var 2
              Var 0, (Relation "year"), Var 3
              Var 1, (Relation "value"), Var 4
              Var 2, (Relation "value"), Var 4
              Var 3, (Relation "value"), Var 4 ] (Entity "root")

 