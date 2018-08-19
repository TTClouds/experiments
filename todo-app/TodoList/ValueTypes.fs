namespace TodoList


type TodoListTitle = string

module ValueTypes =
    open Chessie.ErrorHandling
    open Chessie.ErrorHandling.Validation

    let validateTitle = 
        mustNotBeEmpty
        |> Tester.pipe (mustNotBeShorterThan 10)
        |> Tester.pipe (mustNotBeLongerThan 60)
        |> Tester.named "title"
