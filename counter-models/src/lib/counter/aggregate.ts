import { errorHandling, successHandling } from '../helpers';
import { HandleCommandResult } from '../interfaces';
import { CounterCommand } from './commands';
import { CounterEvent, WasDecremented, WasIncremented } from './events';

export interface CounterState {
  readonly sum: number;
}

export function initState(): CounterState {
  return { sum: 0 };
}

export function handleCommand(
  _: CounterState,
  command: CounterCommand
): HandleCommandResult<CounterEvent> {
  switch (command.eventType) {
    case 'com.example.counter.command.increment':
      return successHandling<WasIncremented>({
        eventType: 'com.example.counter.event.was-incremented'
      });

    case 'com.example.counter.command.decrement':
      return successHandling<WasDecremented>({
        eventType: 'com.example.counter.event.was-decremented'
      });

    default:
      return errorHandling(
        new Error(`Unknown command type: ${(command as any).eventType}`)
      );
  }
}

export function applyEvent(
  state: CounterState,
  event: CounterEvent
): CounterState {
  switch (event.eventType) {
    case 'com.example.counter.event.was-incremented':
      return { ...state, sum: state.sum + 1 };

    case 'com.example.counter.event.was-decremented':
      return { ...state, sum: state.sum - 1 };

    default:
      return state;
  }
}
