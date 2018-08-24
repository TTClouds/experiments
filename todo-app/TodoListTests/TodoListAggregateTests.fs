module TodoList.TodoListAggregate.Tests

open NUnit.Framework
open FsUnit
open Chessie.ErrorHandling
open Chessie.ErrorHandling.Validation.Tests
open TodoList

[<Test>]
let ``TodoListAggregate.init: should return Nothing``() =
    TodoListAggregate.init
    |> should equal Nothing

[<Test>]
let ``TodoListAggregate.apply: should work as expected``() =
    let title1 = "Hello World!!!"
    let title2 = "Good old title"
    let cases = [
        Nothing, WasCreated title1, should equal (Active { title = title1 })
        Nothing, TitleWasUpdated title1, should equal Nothing
        Nothing, WasArchived, should equal Nothing

        Active { title = title1 }, WasCreated title2, should equal (Active { title = title1 })
        Active { title = title1 }, TitleWasUpdated title2, should equal (Active { title = title2 })
        Active { title = title1 }, WasArchived, should equal Archived

        Archived, WasCreated title1, should equal Archived
        Archived, TitleWasUpdated title1, should equal Archived
        Archived, WasArchived, should equal Archived
    ]
    for s, e, r in cases do
        e |> TodoListAggregate.apply s |> r

[<Test>]
let ``TodoListAggregate.execute: should work as expected``() =
    let title1 = "Hello World!!!"
    let title2 = "Good old title"
    let cases = [
        Nothing, DoCreate title1, shouldBeOkWith [WasCreated title1]
        Nothing, DoUpdateTitle title1, shouldBeBad
        Nothing, DoArchive, shouldBeBad
        
        Active { title = title1 }, DoCreate title2, shouldBeBad
        Active { title = title1 }, DoUpdateTitle title2, shouldBeOkWith [TitleWasUpdated title2]
        Active { title = title1 }, DoUpdateTitle title1, shouldBeOkWith []
        Active { title = title1 }, DoArchive, shouldBeOkWith [WasArchived]

        Archived, DoCreate title1, shouldBeBad
        Archived, DoUpdateTitle title1, shouldBeBad
        Archived, DoArchive, shouldBeOkWith []
    ]
    for s, e, r in cases do
        e |> TodoListAggregate.execute s |> r
