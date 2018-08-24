module Chessie.ErrorHandling.Validation.Tests

open NUnit.Framework
open FsUnit
open Chessie.ErrorHandling
open Chessie.ErrorHandling.Validation
open Chessie.ErrorHandling.Validation.TrialInfix
open Chessie.ErrorHandling.Validation.TesterInfix

let shouldBeOk v =
    match v with
    | Ok _ -> ()
    | _ -> failwith (sprintf "Expected result to be Ok but %A found" v)

let shouldBeBad v =
    match v with
    | Bad _ -> ()
    | _ -> failwith (sprintf "Expected result to be Bad but %A found" v)

let shouldBeOkWith value v =
    match v with
    | Ok (a, _) when a = value -> ()
    | _ -> failwith (sprintf "Expected result to be Ok(%A) but %A found" value v)

let shouldBeBadWithAll es v =
    match v with
    | Bad es' ->
        let rec loop ls =
            match ls with
            | [] -> ()
            | e :: ls ->
                if es' |> List.contains e |> not then
                    failwith (sprintf "Expected result to be Bad with error %A but %A found" e es')
                else
                    loop ls
        loop es
    | _ -> failwith (sprintf "Expected result to be Bad but %A found" v)

[<Test>]
let ``Trial.ignore``() =
    Ok ("", [1; 2; 3])
    |> Trial.ignore
    |> should equal (Ok ((), [1; 2; 3]))

[<Test>]
let ``Trial.combine: Ok + Ok``() =
    Trial.combine (fun a b -> String.concat "_" [a; b]) (Ok ("A", [1; 2; 3])) (Ok ("B", [4; 5; 6]))
    |> should equal (Ok ("A_B", [1; 2; 3; 4; 5; 6]))

[<Test>]
let ``Trial.combine: Ok + Bad``() =
    Trial.combine (fun a b -> String.concat "_" [a; b]) (Ok ("A", [1; 2; 3])) (Bad [4; 5; 6])
    |> should equal (Bad [4; 5; 6]: Result<string, _>)

[<Test>]
let ``Trial.combine: Bad + Ok``() =
    Trial.combine (fun a b -> String.concat "_" [a; b]) (Bad [1; 2; 3]) (Ok ("B", [4; 5; 6]))
    |> should equal (Bad [1; 2; 3]: Result<string, _>)

[<Test>]
let ``Trial.combine: Bad + Bad``() =
    Trial.combine (fun a b -> String.concat "_" [a; b]) (Bad [1; 2; 3]) (Bad [4; 5; 6])
    |> should equal (Bad [1; 2; 3; 4; 5; 6]: Result<string, _>)

[<Test>]
let ``Trial.infix combine: Ok <<+> Ok``() =
    Ok ("A", [1; 2; 3]) <<+> Ok ("B", [4; 5; 6])
    |> should equal (Ok ("A", [1; 2; 3; 4; 5; 6]))

[<Test>]
let ``Trial.infix combine: Ok <+>> Ok``() =
    Ok ("A", [1; 2; 3]) <+>> Ok ("B", [4; 5; 6])
    |> should equal (Ok ("B", [1; 2; 3; 4; 5; 6]))

[<Test>]
let ``Converter.result``() =
    "answer?"
    |> Converter.result 42
    |> shouldBeOkWith 42

[<Test>]
let ``Converter.resultErrors``() =
    "answer?"
    |> Converter.resultErrors ["There is no answer"; "you know?"]
    |> shouldBeBadWithAll ["There is no answer"; "you know?"]

[<Test>]
let ``Converter.resultError``() =
    "answer?"
    |> Converter.resultError "There is no answer"
    |> shouldBeBadWithAll ["There is no answer"]

[<Test>]
let ``Converter.id``() =
    "answer?"
    |> Converter.id
    |> shouldBeOkWith "answer?"

[<Test>]
let ``Converter.bind: Ok >>= Ok``() =
    let conv = 
        Converter.resultWith [1; 2; 3] "A" 
        |> Converter.bind (Converter.resultWith [4; 5; 6] "B" )
    "answer?"
    |> conv
    |> should equal (Ok ("B", [1; 2; 3; 4; 5; 6]))

[<Test>]
let ``Converter.bind: Ok >>= Bad``() =
    let conv = 
        Converter.resultWith [1; 2; 3] "A" 
        |> Converter.bind (Converter.resultErrors [4; 5; 6]: Converter<_, string, _>)
    "answer?"
    |> conv
    |> should equal (Bad [4; 5; 6]: Result<string, _>)

[<Test>]
let ``Converter.bind: Bad >>= Ok``() =
    let conv = 
        (Converter.resultErrors [1; 2; 3]: Converter<_, string, _>)
        |> Converter.bind (Converter.resultWith [4; 5; 6] "B" )
    "answer?"
    |> conv
    |> should equal (Bad [1; 2; 3]: Result<string, _>)

[<Test>]
let ``Converter.bind: Bad >>= Bad``() =
    let conv = 
        (Converter.resultErrors [1; 2; 3]: Converter<_, string, _>)
        |> Converter.bind (Converter.resultErrors [4; 5; 6]: Converter<_, string, _>)
    "answer?"
    |> conv
    |> should equal (Bad [1; 2; 3]: Result<string, _>)

[<Test>]
let ``Testers complex 1``() =
    let tester = Testers.mustNotBeEmpty <||> Testers.mustNotBeShorterThan 4
    do "" |> tester |> should equal (fail "must not be empty": Result<unit, _>)
    do "hi" |> tester |> should equal (fail "must not be shorter than 4 chars": Result<unit, _>)
    do "hello" |> tester |> should equal (ok (): Result<_, string>)

