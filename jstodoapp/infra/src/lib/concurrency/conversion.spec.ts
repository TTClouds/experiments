// tslint:disable:no-expression-statement
// tslint:disable:object-literal-sort-keys
import test from 'ava';
import { from, InteropObservable, Observable, Subscribable } from 'rxjs';
import {
  iterableOf,
  matchArray,
  matchArrayLike,
  matchConcurrent,
  matchInteropObservable,
  matchIterable,
  matchObservable,
  matchPromise,
  matchPromiseLike,
  matchSubscribable
} from './conversion';

const konst = <T>(v: T) => () => v;
const iterableLen = (it: Iterable<any>) => [...it].length;
const interopToPromise = <T>(p: InteropObservable<T>) =>
  new Promise<T>((r, x) =>
    p[Symbol.observable]().subscribe({
      next: v => r(v),
      error: e => x(e)
    })
  );
const subscribableToPromise = <T>(p: Subscribable<T>) =>
  new Promise<T>((r, x) =>
    p.subscribe({
      next: v => r(v),
      error: e => x(e)
    })
  );
const observableToPromise = <T>(p: Observable<T>) => p.toPromise();
const promiseOf = <T>(value: T) => () => Promise.resolve(value);
const promiseLikeOf = <T>(value: T): PromiseLike<T> => {
  const p = Promise.resolve(value);
  return { then: (f, r) => p.then(f, r) };
};
const subscribableOf = <T>(value: Observable<T>): Subscribable<T> => {
  return {
    subscribe: (n, e, c) =>
      typeof n === 'function'
        ? value.subscribe(n, e, c)
        : value.subscribe(n && n.next, n && n.error, n && n.complete)
  };
};
const interopObservableOf = <T>(value: Observable<T>): InteropObservable<T> => {
  return {
    [Symbol.observable]: () => subscribableOf(value)
  };
};

const namedCases = {
  array: konst('array'),
  arrayLike: konst('arrayLike'),
  iterable: konst('iterable'),
  promise: konst('promise'),
  promiseLike: konst('promiseLike'),
  observable: konst('observable'),
  subscribable: konst('subscribable'),
  interopObservable: konst('interopObservable'),
  concurrent: konst('concurrent'),
  unknown: konst('unknown')
};

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
  t.is(await matchPromise(p => p, promiseOf(-1))(Promise.resolve(42)), 42);
});

test('matchPromise: On non-promise should use second case', async t => {
  t.is(await matchPromise(p => p, promiseOf(-1))('non-promise'), -1);
});

test('matchPromiseLike: On promise should use first case', async t => {
  t.is(await matchPromiseLike(p => p, promiseOf(-1))(Promise.resolve(42)), 42);
});

test('matchInteropObservable: On observable should use first case', async t => {
  t.is(
    await matchInteropObservable<number>(interopToPromise, promiseOf(-1))(
      from([1, 2, 3])
    ),
    1
  );
});

test('matchInteropObservable: On non-observable should use second case', async t => {
  t.is(
    await matchInteropObservable<number>(interopToPromise, promiseOf(-1))(
      'non-observable'
    ),
    -1
  );
});

test('matchSubscribable: On observable should use first case', async t => {
  t.is(
    await matchSubscribable<number>(subscribableToPromise, promiseOf(-1))(
      from([1, 2, 3])
    ),
    1
  );
});

test('matchSubscribable: On non-observable should use second case', async t => {
  t.is(
    await matchSubscribable<number>(subscribableToPromise, promiseOf(-1))(
      'non-observable'
    ),
    -1
  );
});

test('matchObservable: On observable should use first case', async t => {
  t.is(
    await matchObservable<number>(observableToPromise, promiseOf(-1))(
      from([1, 2, 3])
    ),
    3
  );
});

test('matchObservable: On non-observable should use second case', async t => {
  t.is(
    await matchObservable<number>(observableToPromise, promiseOf(-1))(
      'non-observable'
    ),
    -1
  );
});

test('matchConcurrent: On unknown should use unknown case', t => {
  t.is(matchConcurrent<number, string>(namedCases)(42), 'unknown');
});

test('matchConcurrent: On array should use array case', t => {
  t.is(matchConcurrent<number, string>(namedCases)([1, 2, 3]), 'array');
});

test('matchConcurrent: On arrayLike should use arrayLike case', t => {
  t.is(matchConcurrent<number, string>(namedCases)('array-like'), 'arrayLike');
});

test('matchConcurrent: On iterable should use iterable case', t => {
  t.is(
    matchConcurrent<number, string>(namedCases)(iterableOf([1, 2, 3])),
    'iterable'
  );
});

test('matchConcurrent: On promise should use promise case', t => {
  t.is(
    matchConcurrent<number, string>(namedCases)(Promise.resolve(42)),
    'promise'
  );
});

test('matchConcurrent: On promiseLike should use promiseLike case', t => {
  t.is(
    matchConcurrent<number, string>(namedCases)(promiseLikeOf(42)),
    'promiseLike'
  );
});

test('matchConcurrent: On observable should use observable case', t => {
  t.is(
    matchConcurrent<number, string>(namedCases)(from([1, 2, 3])),
    'observable'
  );
});

test('matchConcurrent: On subscribable should use subscribable case', t => {
  t.is(
    matchConcurrent<number, string>(namedCases)(
      subscribableOf(from([1, 2, 3]))
    ),
    'subscribable'
  );
});

test('matchConcurrent: On interopObservable should use interopObservable case', t => {
  t.is(
    matchConcurrent<number, string>(namedCases)(
      interopObservableOf(from([1, 2, 3]))
    ),
    'interopObservable'
  );
});

test('matchConcurrent: On array with arrayLike should use arrayLike case', t => {
  t.is(
    matchConcurrent<number, string>({
      arrayLike: namedCases.arrayLike,
      iterable: namedCases.iterable,
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })([1, 2, 3]),
    'arrayLike'
  );
});

test('matchConcurrent: On array with iterable should use iterable case', t => {
  t.is(
    matchConcurrent<number, string>({
      iterable: namedCases.iterable,
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })([1, 2, 3]),
    'iterable'
  );
});

test('matchConcurrent: On array with concurrent should use concurrent case', t => {
  t.is(
    matchConcurrent<number, string>({
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })([1, 2, 3]),
    'concurrent'
  );
});

// tslint:disable-next-line:no-if-statement
if (Symbol && Symbol.observable) {
  test.skip('matchConcurrent: On observable with interopObservable should use interopObservable case', t => {
    t.is(
      matchConcurrent<number, string>({
        interopObservable: namedCases.interopObservable,
        subscribable: namedCases.subscribable,
        concurrent: namedCases.concurrent,
        unknown: namedCases.unknown
      })(from([1, 2, 3])),
      'interopObservable'
    );
  });
}

test('matchConcurrent: On observable with subscribable should use subscribable case', t => {
  t.is(
    matchConcurrent<number, string>({
      subscribable: namedCases.subscribable,
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })(from([1, 2, 3])),
    'subscribable'
  );
});

test('matchConcurrent: On observable with concurrent should use concurrent case', t => {
  t.is(
    matchConcurrent<number, string>({
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })(from([1, 2, 3])),
    'concurrent'
  );
});

test('matchConcurrent: On promise with promiseLike should use promiseLike case', t => {
  t.is(
    matchConcurrent<number, string>({
      promiseLike: namedCases.promiseLike,
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })(Promise.resolve(42)),
    'promiseLike'
  );
});

test('matchConcurrent: On promise with concurrent should use concurrent case', t => {
  t.is(
    matchConcurrent<number, string>({
      concurrent: namedCases.concurrent,
      unknown: namedCases.unknown
    })(Promise.resolve(42)),
    'concurrent'
  );
});
