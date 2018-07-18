import { HandleCommandResult, MessageType } from './interfaces';

export function createMessage(eventType: string): MessageType {
  return { eventType };
}

export function successHandling<TEvent extends MessageType = MessageType>(
  // tslint:disable-next-line:readonly-array
  ...events: TEvent[]
): HandleCommandResult<TEvent> {
  return {
    events,
    handlingType: 'success'
  };
}

export function errorHandling<TEvent extends MessageType = MessageType>(
  error: Error
): HandleCommandResult<TEvent> {
  return {
    error,
    handlingType: 'error'
  };
}
