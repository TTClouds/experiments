module TodoList.ValueTypes.Tests

open NUnit.Framework
open FsUnit
open Chessie.ErrorHandling
open Chessie.ErrorHandling.Validation.Tests
open TodoList

[<Test>]
let ``ValueTypes.validateTitle: A normal title should be valid``() =
    ValueTypes.validateTitle "Hola Mundo!!!"
    |> shouldBeOkWith ()

[<Test>]
let ``ValueTypes.validateTitle: An empty title should be invalid``() =
    ValueTypes.validateTitle ""
    |> should equal (Bad ["title", "must not be empty"]: Result<unit, _>)

[<Test>]
let ``ValueTypes.validateTitle: An short title should be invalid``() =
    ValueTypes.validateTitle "hi"
    |> should equal (Bad ["title", "must not be shorter than 10 chars"]: Result<unit, _>)

[<Test>]
let ``ValueTypes.validateTitle: An long title should be invalid``() =
    ValueTypes.validateTitle (String.replicate 100 "a")
    |> should equal (Bad ["title", "must not be longer than 60 chars"]: Result<unit, _>)