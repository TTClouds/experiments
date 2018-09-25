namespace RaftSharp.Core.Types

type NodeId = NodeId of string
type LogTerm = LogTerm of int
type LogIndex = LogIndex of int
type LogEntry<'command> = LogEntry of 'command
type PersistedLogEntry<'command> = PersistedLogEntry of 'command * LogTerm

module PersistedLogEntry =
  let create command term = PersistedLogEntry(command, LogTerm term)

// Invoked by leader to replicate log entries (§5.3);
// also used as heartbeat (§5.2).
type AppendEntriesRequest<'command> = {
    // leader’s term
    term: LogTerm
    // so follower can redirect clients
    leaderId: NodeId
    // index of log entry immediately preceding new ones
    prevLogIndex: LogIndex
    // term of prevLogIndex entry
    prevLogTerm: LogTerm
    // log entries to store (empty for heartbeat; may send more than one for efficiency)
    entries: LogEntry<'command> list
    // leader’s commitIndex
    leaderCommit: LogIndex
}

module AppendEntriesRequest =
  let term (r: AppendEntriesRequest<_>) =
    r.term |> fun (LogTerm term) -> term
  let setTerm term (r: AppendEntriesRequest<_>) =
    { r with term = LogTerm term }
  let leaderId (r: AppendEntriesRequest<_>) =
    r.leaderId |> fun (NodeId id) -> id
  let setLeaderId leaderId (r: AppendEntriesRequest<_>) =
    { r with leaderId = NodeId leaderId }
  let prevLogIndex (r: AppendEntriesRequest<_>) =
    r.prevLogIndex |> fun (LogIndex index) -> index
  let setPrevLogIndex prevLogIndex (r: AppendEntriesRequest<_>) =
    { r with prevLogIndex = LogIndex prevLogIndex }
  let prevLogTerm (r: AppendEntriesRequest<_>) =
    r.prevLogTerm |> fun (LogTerm term) -> term
  let setPrevLogTerm prevLogTerm (r: AppendEntriesRequest<_>) =
    { r with prevLogTerm = LogTerm prevLogTerm }
  let leaderCommit (r: AppendEntriesRequest<_>) =
    r.leaderCommit |> fun (LogIndex index) -> index
  let setLeaderCommit leaderCommit (r: AppendEntriesRequest<_>) =
    { r with leaderCommit = LogIndex leaderCommit }
  let entries (r: AppendEntriesRequest<_>) =
    r.entries
  let setEntries entries (r: AppendEntriesRequest<_>) =
    { r with entries = entries |> Seq.map LogEntry |> Seq.toList }
  let appendEntries entries (r: AppendEntriesRequest<_>) =
    { r with entries = entries |> List.append r.entries }

  let heartbeat leaderId term prevLogIndex prevLogTerm leaderCommit =
    {
      term = LogTerm term
      leaderId = NodeId leaderId
      prevLogIndex = LogIndex prevLogIndex
      prevLogTerm = LogTerm prevLogTerm
      entries = []
      leaderCommit = LogIndex leaderCommit
    }

  let create leaderId term prevLogIndex prevLogTerm leaderCommit entries =
    heartbeat leaderId term prevLogIndex prevLogTerm leaderCommit
    |> setEntries entries

  let empty<'command> : AppendEntriesRequest<'command> =
    heartbeat "" 0 0 0 0

type AppendEntriesResponse = {
    // currentTerm, for leader to update itself
    term: LogTerm
    // true if follower contained entry matching prevLogIndex and prevLogTerm
    success: bool
}

module AppendEntriesResponse =
  let term (r: AppendEntriesResponse) =
    r.term |> fun (LogTerm term) -> term
  let setTerm term (r: AppendEntriesResponse) =
    { r with term = LogTerm term }
  let success (r: AppendEntriesResponse) =
    r.success
  let setSuccess success (r: AppendEntriesResponse) =
    { r with success = success }

  let create term success =
    {
      term = LogTerm term
      success = success
    }

  let empty: AppendEntriesResponse = create 0 false
  let successful term: AppendEntriesResponse = create 0 true
  let failed term: AppendEntriesResponse = create 0 false

// Invoked by candidates to gather votes (§5.2).
type RequestVoteRequest = {
    // candidate’s term
    term: LogTerm
    // candidate requesting vote
    candidateId: NodeId
    // index of candidate’s last log entry (§5.4)
    lastLogIndex: LogIndex
    // term of candidate’s last log entry (§5.4)
    lastLogTerm: LogTerm
}

module RequestVoteRequest =
  let term (r: RequestVoteRequest) =
    r.term
  let setTerm term (r: RequestVoteRequest) =
    { r with term = term }
  let candidateId (r: RequestVoteRequest) =
    r.candidateId
  let setCandidateId candidateId (r: RequestVoteRequest) =
    { r with candidateId = candidateId }
  let lastLogIndex (r: RequestVoteRequest) =
    r.lastLogIndex
  let setLastLogIndex lastLogIndex (r: RequestVoteRequest) =
    { r with lastLogIndex = lastLogIndex }
  let lastLogTerm (r: RequestVoteRequest) =
    r.lastLogTerm
  let setLastLogTerm lastLogTerm (r: RequestVoteRequest) =
    { r with lastLogTerm = lastLogTerm }

  let create term candidateId lastLogIndex lastLogTerm : RequestVoteRequest =
    {
      term = LogTerm term
      candidateId = NodeId candidateId
      lastLogIndex = LogIndex lastLogIndex
      lastLogTerm = LogTerm lastLogTerm
    }

  let empty: RequestVoteRequest = create 0 "" 0 0

type RequestVoteResponse = {
    // currentTerm, for candidate to update itself
    term: LogTerm
    // true means candidate received vote
    voteGranted: bool
}

module RequestVoteResponse =
  let term (r: RequestVoteResponse) =
    r.term
  let setTerm term (r: RequestVoteResponse) =
    { r with term = term }
  let voteGranted (r: RequestVoteResponse) =
    r.voteGranted
  let setVoteGranted voteGranted (r: RequestVoteResponse) =
    { r with voteGranted = voteGranted }

  let create term voteGranted: RequestVoteResponse =
    {
      term = LogTerm term
      voteGranted = voteGranted
    }

  let empty: RequestVoteResponse = create 0 false
  let granted term: RequestVoteResponse = create 0 true
  let notGranted term: RequestVoteResponse = create 0 false

type NodeRequest<'command> =
  | AppendEntries of AppendEntriesRequest<'command>
  | RequestVote of RequestVoteRequest
