namespace Cqrs

type SingleRecordOp<'v> =
    | SkipRecord
    | DeleteRecord
    | UpsertRecord of 'v