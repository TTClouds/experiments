namespace RaftSharp.Core

type NodeId = NodeId of string
type LogTerm = LogTerm of int
type LogIndex = LogIndex of int
type LogEntry<'command> = LogEntry of 'command
type PersistedLogEntry<'command> = PersistedLogEntry of 'command * LogTerm

module PersistedLogEntry =
  let create command term = PersistedLogEntry(command, term)

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
    r.term
  let setTerm term (r: AppendEntriesRequest<_>) =
    { r with term = term }
  let leaderId (r: AppendEntriesRequest<_>) =
    r.leaderId
  let setLeaderId leaderId (r: AppendEntriesRequest<_>) =
    { r with leaderId = leaderId }
  let prevLogIndex (r: AppendEntriesRequest<_>) =
    r.prevLogIndex
  let setPrevLogIndex prevLogIndex (r: AppendEntriesRequest<_>) =
    { r with prevLogIndex = prevLogIndex }
  let prevLogTerm (r: AppendEntriesRequest<_>) =
    r.prevLogTerm
  let setPrevLogTerm prevLogTerm (r: AppendEntriesRequest<_>) =
    { r with prevLogTerm = prevLogTerm }
  let leaderCommit (r: AppendEntriesRequest<_>) =
    r.leaderCommit
  let setLeaderCommit leaderCommit (r: AppendEntriesRequest<_>) =
    { r with leaderCommit = leaderCommit }
  let entries (r: AppendEntriesRequest<_>) =
    r.entries
  let setEntries entries (r: AppendEntriesRequest<_>) =
    { r with entries = entries }
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
    r.term
  let setTerm term (r: AppendEntriesResponse) =
    { r with term = term }
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

// Persistent state on all servers:
// (Updated on stable storage before responding to RPCs)
type PersistentNodeState<'command> = {
    // latest term server has seen (initialized to 0 on first boot,
    // increases monotonically)
    currentTerm: LogTerm
    // candidateId that received vote in current term (or null if none)
    votedFor: NodeId
    // log entries; each entry contains command for state machine,
    // and term when entry was received by leader (first index is 1)
    log: PersistedLogEntry<'command> list
}

// Volatile state on all servers:
type VolatileNodeState = {
    // index of highest log entry known to be committed
    // (initialized to 0, increases monotonically)
    commitIndex: LogTerm
    // index of highest log entry applied to state machine
    // (initialized to 0, increases monotonically)
    lastApplied: LogIndex
}

// Volatile state on leaders:
// (Reinitialized after election)
type VolatileLeaderState = {
    // for each server, index of the next log entry to send to that server
    // (initialized to leader last log index + 1)
    nextIndex: Map<NodeId, LogIndex>
    // for each server, index of highest log entry known to be replicated on server
    // (initialized to 0, increases monotonically)
    matchIndex: Map<NodeId, LogIndex>
}

type NodeState<'command> = {
  persistent: PersistentNodeState<'command>
  state: VolatileNodeState
  leader: VolatileLeaderState option
}

