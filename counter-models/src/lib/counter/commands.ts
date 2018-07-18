import { MessageType } from "../interfaces";

export interface Increment extends MessageType {
  readonly eventType: 'com.example.counter.command.increment';
}

export interface Decrement extends MessageType {
  readonly eventType: 'com.example.counter.command.decrement';
}

export type CounterCommand = Increment | Decrement;

export function increment(): Increment {
  return {
    eventType: 'com.example.counter.command.increment'
  };
}

export function decrement(): Decrement {
  return {
    eventType: 'com.example.counter.command.decrement'
  };
}
