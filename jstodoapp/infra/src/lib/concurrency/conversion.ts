// tslint:disable:no-if-statement
// tslint:disable:no-expression-statement
// tslint:disable:no-submodule-imports
// tslint:disable:object-literal-sort-keys
import {
  from,
  InteropObservable,
  Observable,
  ObservableInput,
  Subscribable
} from 'rxjs';
import { first } from 'rxjs/operators';

export type ConcurrentLike<T> = ObservableInput<T>; // Subscribable | PromiseLike | InteropObservable | ArrayLike | Iterable

export function isObservable<T = any>(value: unknown): value is Observable<T> {
  return value instanceof Observable;
}

export function isPromise<T = any>(value: unknown): value is Promise<T> {
  return value instanceof Promise;
}

export function isSubscribable<T = any>(
  value: unknown
): value is Subscribable<T> {
  return !!value && typeof (value as any).subscribe === 'function';
}

export function isPromiseLike<T = any>(
  value: unknown
): value is PromiseLike<T> {
  return !!value && typeof (value as any).then === 'function';
}

export function isArrayLike<T = any>(value: unknown): value is ArrayLike<T> {
  return !!value && typeof (value as any).length === 'number';
}

export function isInteropObservable<T = any>(
  value: unknown
): value is InteropObservable<T> {
  return !!value && typeof (value as any)[Symbol.observable] === 'function';
}

export function isIterable<T = any>(value: unknown): value is Iterable<T> {
  return !!value && typeof (value as any)[Symbol.iterator] === 'function';
}

const matchUnknown = <R = unknown>(unknown?: (v: unknown) => R) => (
  value: unknown
): R => {
  if (unknown) {
    return unknown(value);
  }
  throw new TypeError(`Unmatched case for: ${value}`);
};

const matchTest = <I = unknown, R = unknown>(
  test: (value: unknown) => value is I,
  positive?: (s: I) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  positive && test(value) ? positive(value) : matchUnknown(unknown)(value);

export const matchArrayLike = <T = unknown, R = unknown>(
  arrayLike?: (s: ArrayLike<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<ArrayLike<T>, R>(isArrayLike, arrayLike, unknown)(value);

export const matchIterable = <T = unknown, R = unknown>(
  iterable?: (s: Iterable<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<Iterable<T>, R>(isIterable, iterable, unknown)(value);

export const matchArray = <T = unknown, R = unknown>(
  array?: (s: ReadonlyArray<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<ReadonlyArray<T>, R>(Array.isArray, array, unknown)(value);

export const matchPromiseLike = <T = unknown, R = unknown>(
  promiseLike?: (s: PromiseLike<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<PromiseLike<T>, R>(isPromiseLike, promiseLike, unknown)(value);

export const matchPromise = <T = unknown, R = unknown>(
  promise?: (s: Promise<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<Promise<T>, R>(isPromise, promise, unknown)(value);

export const matchInteropObservable = <T = unknown, R = unknown>(
  interopObservable?: (s: InteropObservable<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<InteropObservable<T>, R>(
    isInteropObservable,
    interopObservable,
    unknown
  )(value);

export const matchSubscribable = <T = unknown, R = unknown>(
  subscribable?: (s: Subscribable<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<Subscribable<T>, R>(isSubscribable, subscribable, unknown)(value);

export const matchObservable = <T = unknown, R = unknown>(
  observable?: (s: Observable<T>) => R,
  unknown?: (v: unknown) => R
) => (value: unknown): R =>
  matchTest<Observable<T>, R>(isObservable, observable, unknown)(value);

export const matchConcurrent = <T = unknown, R = unknown>(cases: {
  readonly observable?: (s: Observable<T>) => R;
  readonly subscribable?: (s: Subscribable<T>) => R;
  readonly interopObservable?: (s: InteropObservable<T>) => R;
  readonly promise?: (s: Promise<T>) => R;
  readonly promiseLike?: (s: PromiseLike<T>) => R;
  readonly array?: (s: ReadonlyArray<T>) => R;
  readonly arrayLike?: (s: ArrayLike<T>) => R;
  readonly iterable?: (s: Iterable<T>) => R;
  readonly concurrent?: (v: ConcurrentLike<T>) => R;
  readonly unknown?: (v: unknown) => R;
}) => {
  const opts = {
    array: cases.array || cases.arrayLike || cases.iterable || cases.concurrent,
    arrayLike: cases.arrayLike || cases.concurrent,
    iterable: cases.iterable || cases.concurrent,
    observable: cases.observable || cases.subscribable || cases.concurrent,
    subscribable: cases.subscribable || cases.concurrent,
    interopObservable: cases.interopObservable || cases.concurrent,
    promise: cases.promise || cases.promiseLike || cases.concurrent,
    promiseLike: cases.promiseLike || cases.concurrent
  };
  return (value: unknown): R => {
    if (opts.array && Array.isArray(value)) {
      return opts.array(value);
    } else if (opts.arrayLike && isArrayLike(value)) {
      return opts.arrayLike(value);
    } else if (opts.iterable && isIterable(value)) {
      return opts.iterable(value);
    } else if (opts.promise && isPromise(value)) {
      return opts.promise(value);
    } else if (opts.promiseLike && isPromiseLike(value)) {
      return opts.promiseLike(value);
    } else if (opts.observable && isObservable(value)) {
      return opts.observable(value);
    } else if (opts.interopObservable && isInteropObservable(value)) {
      return opts.interopObservable(value);
    } else if (opts.subscribable && isSubscribable(value)) {
      return opts.subscribable(value);
    } else {
      return matchUnknown(cases.unknown)(value);
    }
  };
};

export const toObservable = <T>(value: ConcurrentLike<T>): Observable<T> =>
  matchConcurrent<T, Observable<T>>({
    observable: obs => obs,
    concurrent: v => from(v)
  })(value);

export const toPromise = <T>(value: ConcurrentLike<T>): Promise<T> =>
  matchConcurrent<T, Promise<T>>({
    promise: prom => prom,
    concurrent: obs => from(obs).toPromise()
  })(value);

export const toPromiseFirst = <T>(value: ConcurrentLike<T>): Promise<T> =>
  matchConcurrent<T, Promise<T>>({
    promise: prom => prom,
    concurrent: obs =>
      from(obs)
        .pipe(first())
        .toPromise()
  })(value);
