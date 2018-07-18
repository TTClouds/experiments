// tslint:disable:no-expression-statement
// tslint:disable:no-if-statement
// tslint:disable:no-let
import { test } from 'ava';
import { CommandHandler, MessageType, StateInitializer } from './interfaces';

export const canHandleCommand = <
  TCommand extends MessageType = MessageType,
  TEvent extends MessageType = MessageType,
  TState = any
>(
  handleCommand: CommandHandler<TCommand, TEvent, TState>,
  initState: StateInitializer<TState>
) => (
  commandName: string,
  command: TCommand,
  expectedEvents: ReadonlyArray<TEvent>,
  state?: TState
): void => {
  test(`handleCommand can handle an ${commandName} command`, t => {
    const fromState = state || initState();
    const result = handleCommand(fromState, command);
    t.truthy(result);
    t.is(result.handlingType, 'success');
    if (result.handlingType === 'success') {
      t.is(result.events.length, expectedEvents.length);
      for (let index = 0; index < result.events.length; index++) {
        const ev = result.events[index];
        const expEv = expectedEvents[index];
        t.deepEqual(ev, expEv);
      }
    }
  });
};

export const canNotHandleCommand = <
  TCommand extends MessageType = MessageType,
  TState = any
>(
  handleCommand: CommandHandler<TCommand, MessageType, TState>,
  initState: StateInitializer<TState>
) => (
  commandName: string,
  command: MessageType,
  expectedErrorMessage: string,
  state?: TState
): void => {
  test(`handleCommand can handle an ${commandName} command`, t => {
    const fromState = state || initState();
    const result = handleCommand(fromState, command as TCommand);
    t.truthy(result);
    t.is(result.handlingType, 'error');
    if (result.handlingType === 'error') {
      t.truthy(result.error);
      t.is(result.error.message, expectedErrorMessage);
    }
  });
}
