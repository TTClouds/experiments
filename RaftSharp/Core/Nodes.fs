namespace RaftSharp.Core.Nodes
open RaftSharp.Core.Types
open System

type InitializeNodeRequest<'command> = {
  state: 'command
  currentNodeId: NodeId
  nodes: NodeId Set
}

type InitializeNodeResponse = {
  success: bool
}

type WriteCommandRequest<'command> = {
  command: 'command
}

type WriteCommandResponse = {
  success: bool
}

type ElectionTimeoutRequest = {
  test: bool
}

type ElectionTimeoutResponse = {
  success: bool
}

type VoteConcludedRequest = {
  test: bool
}

type VoteConcludedResponse = {
  success: bool
}

type SendHeartbeatsRequest = {
  test: bool
}

type SendHeartbeatsResponse = {
  success: bool
}

type Sink<'a> = 'a -> unit

type RpcCommand<'command> =
  | AppendEntries of AppendEntriesRequest<'command> * Sink<AppendEntriesResponse>
  | RequestVote of RequestVoteRequest * Sink<RequestVoteResponse>

type InternalCommand<'command> =
  | InitializeNode of InitializeNodeRequest<'command> * Sink<InitializeNodeResponse>
  | ElectionTimeout of ElectionTimeoutRequest * Sink<ElectionTimeoutResponse>
  | VoteConcluded of VoteConcludedRequest * Sink<VoteConcludedResponse>
  | SendHeartbeats of SendHeartbeatsRequest * Sink<SendHeartbeatsResponse>
  | WriteCommand of WriteCommandRequest<'command> * Sink<WriteCommandResponse>

type NodeCommand<'command> =
  | RpcCommand of RpcCommand<'command>
  | InternalCommand of InternalCommand<'command>

type NodeEnvironment<'command, 'fsm> = {
  tellSelf: TimeSpan option -> Sink<InternalCommand<'command>>
  tellNode: NodeId -> Sink<RpcCommand<'command>>
  empty: 'fsm
  write: 'command -> 'fsm -> 'fsm
}

module Node =

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

  module PersistentNodeState =
    let currentTerm (r: PersistentNodeState<_>) =
      r.currentTerm
    let setCurrentTerm currentTerm (r: PersistentNodeState<_>) =
      { r with currentTerm = currentTerm }
    let votedFor (r: PersistentNodeState<_>) =
      r.votedFor
    let setVotedFor votedFor (r: PersistentNodeState<_>) =
      { r with votedFor = votedFor }
    let log (r: PersistentNodeState<_>) =
      r.log
    let setLog log (r: PersistentNodeState<_>) =
      { r with log = log }
    let appendLog log (r: PersistentNodeState<_>) =
      { r with log = List.append r.log log }

    let empty<'command> = {
      currentTerm = LogTerm 0
      votedFor = NodeId ""
      log = []
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

  module VolatileNodeState =
    let empty = {
      commitIndex = LogTerm 0
      lastApplied = LogIndex 0
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

  module VolatileLeaderState =
    let empty = {
      nextIndex = Map.empty
      matchIndex = Map.empty
    }

  type NodeState<'fsm> = {
    fsm: 'fsm
    common: VolatileNodeState
    current: NodeConfig
  }

  and NodeConfig =
    | LeaderNode of VolatileLeaderState
    | FollowerNode
    | CandidateNode

  module NodeState =
    let empty fsm = {
      fsm = fsm
      common = VolatileNodeState.empty
      current = FollowerNode
    }

  let defineNode<'command, 'fsm> (env: NodeEnvironment<'command, 'fsm>) =
    ()

module InMailbox =
  open Node

  let startNode env =
    MailboxProcessor.Start(fun mb ->
      let rec loop state = async {
        let! command = (mb: MailboxProcessor<_>).Receive()

        match command, state with
        | InternalCommand (WriteCommand (req, sink)), { NodeState.fsm = fsm } ->
          let fsm' = env.write req.command fsm
          let state' = { state with fsm = fsm' }
          return! loop <| state'

        // TODO: check all paths are covered

      }

      let state = NodeState.empty env.empty
      loop state
    )
