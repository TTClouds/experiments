// tslint:disable:no-expression-statement
// tslint:disable:object-literal-sort-keys
import test from 'ava';
import { from } from 'rxjs';
import {
  matchArray,
  matchArrayLike,
  matchInteropObservable,
  matchIterable,
  matchPromise,
  matchPromiseLike
} from './conversion';

const iterableLen = (it: Iterable<any>) => [...it].length;

test('matchArray: On array should use first case', t => {
  t.is(matchArray(arr => arr.length, () => -1)([1, 2, 3]), 3);
});

test('matchArray: On non-array should use second case', t => {
  t.is(matchArray(arr => arr.length, () => -1)('Non-array'), -1);
});

test('matchArrayLike: On array should use first case', t => {
  t.is(matchArrayLike(arr => arr.length, () => -1)([1, 2, 3]), 3);
});

test('matchArrayLike: On array-like should use first case', t => {
  t.is(matchArrayLike(arr => arr.length, () => -1)('array-like'), 10);
});

test('matchArrayLike: On non-array-like should use second case', t => {
  t.is(matchArrayLike(arr => arr.length, () => -1)(42), -1);
});

test('matchIterable: On array should use first case', t => {
  t.is(matchIterable(iterableLen, () => -1)([1, 2, 3]), 3);
});

test('matchIterable: On array-like should use first case', t => {
  t.is(matchIterable(iterableLen, () => -1)('array-like'), 10);
});

test('matchIterable: On non-array-like should use second case', t => {
  t.is(matchIterable(iterableLen, () => -1)(42), -1);
});

test('matchPromise: On promise should use first case', async t => {
  t.is(
    await matchPromise(p => p, () => Promise.resolve(-1))(Promise.resolve(42)),
    42
  );
});

test('matchPromise: On non-promise should use second case', async t => {
  t.is(
    await matchPromise(p => p, () => Promise.resolve(-1))('non-promise'),
    -1
  );
});

test('matchPromiseLike: On promise should use first case', async t => {
  t.is(
    await matchPromiseLike(p => p, () => Promise.resolve(-1))(
      Promise.resolve(42)
    ),
    42
  );
});

test('matchInteropObservable: On observable should use first case', async t => {
  t.is(
    await matchInteropObservable<number>(
      p =>
        new Promise((r, x) =>
          p[Symbol.observable]().subscribe({
            next: v => r(v),
            error: e => x(e)
          })
        ),
      () => Promise.resolve(-1)
    )(from([1, 2, 3])),
    1
  );
});
