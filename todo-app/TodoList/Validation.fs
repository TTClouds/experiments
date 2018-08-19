namespace Chessie.ErrorHandling.Validation

open Chessie.ErrorHandling
open System.Text.RegularExpressions

type Converter<'a, 'b, 'e> = 'a -> Result<'b, 'e>
type Corrector<'a, 'e> = Converter<'a, 'a, 'e>
type Tester<'a, 'e> = Converter<'a, unit, 'e>
type Validator<'a> = Tester<'a, string * string>

module Trial =
    let ignore t = Trial.lift ignore t

    let combine f t1 t2 =
        match t1, t2 with
        | Ok (v1, ws1), Ok (v2, ws2) -> Ok(f v1 v2, List.append ws1 ws2)
        | Ok _, Bad es -> Bad es
        | Bad es, Ok _ -> Bad es
        | Bad es1, Bad es2 -> Bad <| List.append es1 es2
    
    let combineAll f =
        Seq.reduce (fun s t -> combine f s t)
    
    let mapMsgs f = function
        | Ok(v, ws) -> Ok(v, f ws)
        | Bad es -> Bad(f es)
    
    let mapMsg f = mapMsgs (fun es -> List.map f es)
    
    let mapErrors f = function
        | Ok(v, ws) -> Ok(v, ws)
        | Bad es -> Bad(f es)
    
    let mapError f = mapErrors (fun es -> List.map f es)
    
    let mapWarnings f = function
        | Ok(v, ws) -> Ok(v, f ws)
        | Bad es -> Bad(es)
    
    let mapWarning f = mapWarnings (fun es -> List.map f es)
    

module Converter =
    let result b : Converter<'a, 'b, 'e> = 
        fun _ -> Ok(b, [])
    let resultErrors es : Converter<'a, 'b, 'e> = 
        fun _ -> Bad es
    let resultError e : Converter<'a, 'b, 'e> =
        resultErrors [e]

    let id a : Result<'a, 'e> = ok a

    let bind f (m: Converter<'a, 'b, 'e>) : Converter<'a, 'c, 'e> = fun a -> 
        match m a with
        | Ok(b, _) as ma -> Trial.combine (fun _ x -> x) ma (f b)
        | Bad es -> Bad es

    let map f (m: Converter<'a, 'b, 'e>) : Converter<'a, 'c, 'e> =
        bind (f >> ok) m

    let ignore (m: Converter<'a, 'b, 'e>) = map ignore m

    let compose (ma: Converter<'a, 'b, 'e>) (mb: Converter<'b, 'c, 'e>) =
        bind mb ma

    let combine (f: 'b -> 'c -> 'd) (cb: Converter<'a, 'b, 'e>) (cc: Converter<'a, 'c, 'e>): Converter<'a, 'd, 'e> = 
        fun a -> Trial.combine f (cb a) (cc a)
    
    let mapMsgs f c = fun a -> c a |> Trial.mapMsgs f
    
    let mapMsg f = mapMsgs (fun es -> List.map f es)
    
    let mapErrors f c = fun a -> c a |> Trial.mapErrors f
    
    let mapError f = mapErrors (fun es -> List.map f es)
    
    let mapWarnings f c = fun a -> c a |> Trial.mapWarnings f
    
    let mapWarning f = mapWarnings (fun es -> List.map f es)

module Corrector =
    let ofConverter (t: Converter<'a, 'b, 'e>) : Corrector<'a, 'e> =
        fun a -> t a |> Trial.lift (fun _ -> a)

module Tester =
    let ofConverter (c: Converter<'a, 'b, 'e>) : Tester<'a, 'e> =
        c |> Converter.ignore

    let pipe (next: Tester<'a, 'e>) (prev: Tester<'a, 'e>): Tester<'a, 'e> =
        Converter.combine (fun _ _ -> ()) prev next

    let named name (tester: Tester<'a, _>): Validator<'a> =
        tester |> Converter.mapMsg (fun e -> name, e)

    let prefixedWith prefixer (validator: Validator<'a>): Validator<'a> =
        validator |> Converter.mapMsg (fun (p, e) -> (prefixer p, e))

    let prefixed prefix =
        let prefixer p = sprintf "%s%s" prefix p
        prefixedWith prefixer

[<RequireQualifiedAccess>]
module TestersI18N =
    let conditional f cond: Tester<'a, 'e> = function
        | v when not <| cond v -> fail <| f()
        | _ -> ok()

    let mustNotBeEmpty f =
        (fun str -> String.length str > 0) |> conditional f 

    let mustNotBeBlank f =
        (fun str -> not <| Regex.IsMatch(str, @"^\s*$")) |> conditional f 

    let mustNotBeShorterThan f len =
        (fun str -> String.length str >= len) |> conditional f 

    let mustNotBeLongerThan f len =
        (fun str -> String.length str <= len) |> conditional f 

[<AutoOpen>]
module Testers =
    let mustNotBeEmpty = 
        TestersI18N.mustNotBeEmpty (fun () -> "must not be empty")
    
    let mustNotBeBlank = 
        TestersI18N.mustNotBeBlank (fun () -> "must not be blank")
    
    let mustNotBeShorterThan len =
        TestersI18N.mustNotBeShorterThan (fun () -> sprintf "must not be shorter than %d chars" len) len
    
    let mustNotBeLongerThan len =
        TestersI18N.mustNotBeLongerThan (fun () -> sprintf "must not be longer than %d chars" len) len
    
    