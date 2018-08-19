module TodoList.TodoListView

open Cqrs

type State = {
    isActive: bool
    title: TodoListTitle
}

let init = None

let apply state event =
    match state, event with
    | None, WasCreated title ->
        UpsertRecord <| { isActive = true; title = title }

    | Some st, TitleWasUpdated newTitle ->
        UpsertRecord <| { st with title = newTitle }

    | Some st, WasArchived ->
        UpsertRecord <| { st with isActive = false }

    | _, _ ->
        SkipRecord


