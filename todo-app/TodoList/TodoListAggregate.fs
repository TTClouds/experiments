module TodoList.TodoListAggregate

open Chessie.ErrorHandling

type State =
    | Nothing
    | Active of ActiveState
    | Archived

and ActiveState = {
    title: TodoListTitle
}

let init = Nothing

let apply state event =
    match state, event with
    | Nothing, WasCreated title ->
        Active { title = title }

    | Active st, TitleWasUpdated newTitle ->
        Active { st with title = newTitle }

    | Active _, WasArchived ->
        Archived

    | _, _ ->
        state

let execute state command =
    match state, command with
    | Nothing, DoCreate title ->
        [ WasCreated title ] 
        |> ok

    | Active st, DoUpdateTitle title ->
        if st.title = title then [] else [ TitleWasUpdated title ] 
        |> ok
    
    | Active _, DoArchive ->
        [ WasArchived ]
        |> ok
    
    | Archived, DoArchive ->
        []
        |> ok

    | _, _ ->
        fail ("", sprintf "Incompatible state %A to receive command %A" state command)
