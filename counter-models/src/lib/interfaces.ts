export interface ExtensionMap {
  readonly [key: string]: string | ExtensionMap;
}

export interface MessageType {
  readonly eventType: string;
  readonly cloudEventVersion?: string;
  readonly source?: string;
  readonly eventID?: string;
  readonly schemaURL?: string;
  readonly eventTime?: string;
  readonly contentType?: string;
  readonly extensions?: ExtensionMap;
  readonly data?: any;
}

export interface HandleCommandSuccess<T extends MessageType = MessageType> {
  readonly handlingType: 'success';
  readonly events: ReadonlyArray<T>;
}

export interface HandleCommandError {
  readonly handlingType: 'error';
  readonly error: Error;
}

export type HandleCommandResult<TEvent extends MessageType = MessageType> =
  | HandleCommandSuccess<TEvent>
  | HandleCommandError;

export type StateInitializer<TState = any> = () => TState;

export type CommandHandler<
  TCommand extends MessageType = MessageType,
  TEvent extends MessageType = MessageType,
  TState = any
> = (state: TState, command: TCommand) => HandleCommandResult<TEvent>;
