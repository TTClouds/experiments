namespace TodoList

type TodoListCommand =
    | DoCreate of TodoListTitle
    | DoUpdateTitle of TodoListTitle
    | DoArchive

module TodoListCommand =
    open Chessie.ErrorHandling

    let validate = function
    | DoCreate title -> 
        ValueTypes.validateTitle title

    | DoUpdateTitle title -> 
        ValueTypes.validateTitle title

    | DoArchive ->
        ok()
