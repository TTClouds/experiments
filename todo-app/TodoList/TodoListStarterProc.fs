module TodoList.TodoListStarterProc

open Chessie.ErrorHandling

let execute selfSend event = async {
    match event with
    | WasCreated title ->
        do! selfSend (DoUpdateTitle <| sprintf "%s (welcome)" title)
    | _ ->
        do()
}
