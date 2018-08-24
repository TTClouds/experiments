module TodoList.Commands.Tests

open NUnit.Framework
open Chessie.ErrorHandling.Validation.Tests
open TodoList

[<Test>]
let ``TodoListCommand.validate: DoCreate with invalid title should be invalid``() =
    DoCreate ""
    |> TodoListCommand.validate
    |> shouldBeBadWithAll [("title", "must not be empty")]

[<Test>]
let ``TodoListCommand.validate: DoCreate with valid title should be valid``() =
    DoCreate "Hello World!!!"
    |> TodoListCommand.validate
    |> shouldBeOk

[<Test>]
let ``TodoListCommand.validate: DoUpdateTitle with invalid title should be invalid``() =
    DoUpdateTitle ""
    |> TodoListCommand.validate
    |> shouldBeBadWithAll [("title", "must not be empty")]

[<Test>]
let ``TodoListCommand.validate: DoUpdateTitle with valid title should be valid``() =
    DoUpdateTitle "Hello World!!!"
    |> TodoListCommand.validate
    |> shouldBeOk

[<Test>]
let ``TodoListCommand.validate: DoArchive should be valid``() =
    DoArchive
    |> TodoListCommand.validate
    |> shouldBeOk
