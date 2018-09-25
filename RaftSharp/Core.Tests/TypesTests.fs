module RaftSharp.Core.Types.Tests

open NUnit.Framework
open FsUnit

[<Test>]
let ``PersistedLogEntry.create``() =
  do PersistedLogEntry.create "Execute" 5
    |> should equal (PersistedLogEntry("Execute", LogTerm 5))

[<Test>]
let ``AppendEntriesRequest.heartbeat``() =
  let req = AppendEntriesRequest.heartbeat "Node1" 5 1000 3 1003
  do req |> AppendEntriesRequest.leaderId |> should equal "Node1"
  do req |> AppendEntriesRequest.term |> should equal 5
  do req |> AppendEntriesRequest.prevLogIndex |> should equal 1000
  do req |> AppendEntriesRequest.prevLogTerm |> should equal 3
  do req |> AppendEntriesRequest.leaderCommit |> should equal 1003
  do req |> AppendEntriesRequest.entries |> List.isEmpty |> should be True

[<Test>]
let ``AppendEntriesRequest.create``() =
  let req =
    seq { yield PersistedLogEntry.create "Execute" 5 }
    |> AppendEntriesRequest.create "Node1" 5 1000 3 1003
  do req |> AppendEntriesRequest.leaderId |> should equal "Node1"
  do req |> AppendEntriesRequest.term |> should equal 5
  do req |> AppendEntriesRequest.prevLogIndex |> should equal 1000
  do req |> AppendEntriesRequest.prevLogTerm |> should equal 3
  do req |> AppendEntriesRequest.leaderCommit |> should equal 1003
  do req |> AppendEntriesRequest.entries
         |> should equal [ PersistedLogEntry.create "Execute" 5 |> LogEntry ]

[<Test>]
let ``AppendEntriesRequest.empty``() =
  let req = AppendEntriesRequest.empty
  do req |> AppendEntriesRequest.leaderId |> should equal ""
  do req |> AppendEntriesRequest.term |> should equal 0
  do req |> AppendEntriesRequest.prevLogIndex |> should equal 0
  do req |> AppendEntriesRequest.prevLogTerm |> should equal 0
  do req |> AppendEntriesRequest.leaderCommit |> should equal 0
  do req |> AppendEntriesRequest.entries |> List.isEmpty |> should be True
