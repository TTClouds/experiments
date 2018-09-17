namespace RaftSharp.Core

type NodeId = string
type LogTerm = int
type LogIndex = int
type LogEntry<'command> = 'command
type PersistedLogEntry<'command> = {
    command: 'command
    term: LogTerm
}

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

type AppendEntriesResponse = {
    // currentTerm, for leader to update itself
    term: LogTerm
    // true if follower contained entry matching prevLogIndex and prevLogTerm
    success: bool
}

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

type RequestVoteResponse = {
    // currentTerm, for candidate to update itself
    term: LogTerm
    // true means candidate received vote
    voteGranted: bool
}

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

