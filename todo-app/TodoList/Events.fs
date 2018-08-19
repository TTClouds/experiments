namespace TodoList

type TodoListEvent =
    | WasCreated of TodoListTitle
    | TitleWasUpdated of TodoListTitle
    | WasArchived
