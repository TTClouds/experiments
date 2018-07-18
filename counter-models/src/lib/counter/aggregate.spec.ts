// tslint:disable:no-expression-statement
// tslint:disable:no-if-statement
// tslint:disable:no-let
import { test } from 'ava';
import { createMessage } from '../helpers';
import { canHandleCommand, canNotHandleCommand } from '../testing-helpers';
import { handleCommand, initState } from './aggregate';
import { decrement, increment } from './commands';
import { wasDecremented, wasIncremented } from './events';

test('initState returns the initial counter state', t => {
  const state = initState();
  t.truthy(state);
  t.is(state.sum, 0);
});

const canHandle = canHandleCommand(handleCommand, initState);
const canNotHandle = canNotHandleCommand(handleCommand, initState);

canHandle('increment', increment(), [wasIncremented()]);
canHandle('decrement', decrement(), [wasDecremented()]);

canNotHandle(
  'unknown',
  createMessage('com.example.counter.command.unknown'),
  'Unknown command type: com.example.counter.command.unknown'
);
