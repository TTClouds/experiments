namespace TodoList

open Chessie.ErrorHandling.Validation.TesterInfix

type TodoListTitle = string

module ValueTypes =
    open Chessie.ErrorHandling.Validation

    let validateTitle = 
        mustNotBeEmpty <||> (mustNotBeShorterThan 10 <&&> mustNotBeLongerThan 60)
        |> Tester.named "title"
