import { MessageType } from '../interfaces';

export interface WasIncremented extends MessageType {
  readonly eventType: 'com.example.counter.event.was-incremented';
}

export interface WasDecremented extends MessageType {
  readonly eventType: 'com.example.counter.event.was-decremented';
}

export type CounterEvent = WasIncremented | WasDecremented;

export function wasIncremented(): WasIncremented {
  return {
    eventType: 'com.example.counter.event.was-incremented'
  };
}

export function wasDecremented(): WasDecremented {
  return {
    eventType: 'com.example.counter.event.was-decremented'
  };
}
