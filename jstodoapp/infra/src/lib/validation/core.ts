// tslint:disable:no-if-statement
// tslint:disable:no-submodule-imports
import { from, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  ConcurrentLike,
  isConcurrent,
  matchUnknown,
  toObservable
} from '../concurrency';

export type SyncPrimitiveValidationErrors = ReadonlyArray<string>;
export interface SyncFieldsValidationErrors {
  readonly [field: string]: SyncValidationResult;
}
export type SyncListValidationErrors = ReadonlyArray<SyncValidationResult>;

export type UndefinedBoolean = boolean | undefined;

export interface SyncValidationResultBase {
  readonly isValid: UndefinedBoolean;
}

export const andUndefined = (a: UndefinedBoolean, b: UndefinedBoolean) =>
  a === undefined || b === undefined ? undefined : a && b;

export const orUndefined = (a: UndefinedBoolean, b: UndefinedBoolean) =>
  a === undefined || b === undefined ? undefined : a || b;

export const areValid = (values: ReadonlyArray<UndefinedBoolean>) =>
  values.reduce(andUndefined, true);

export interface SyncPrimitiveValidationResult
  extends SyncValidationResultBase {
  readonly kind: 'primitive';
  readonly errors: SyncPrimitiveValidationErrors;
}

export interface SyncFieldsValidationResult extends SyncValidationResultBase {
  readonly kind: 'fields';
  readonly fields: SyncFieldsValidationErrors;
}

export interface SyncListValidationResult extends SyncValidationResultBase {
  readonly kind: 'list';
  readonly list: SyncListValidationErrors;
}

export type SyncValidationResult =
  | SyncPrimitiveValidationResult
  | SyncFieldsValidationResult
  | SyncListValidationResult;

export const syncPrimitiveValidationResult = (
  errors?: SyncPrimitiveValidationErrors
): SyncPrimitiveValidationResult =>
  errors && errors.some(e => e.length > 0)
    ? {
        errors: errors.filter(e => e.length > 0),
        isValid: false,
        kind: 'primitive'
      }
    : { kind: 'primitive', isValid: true, errors: [] };

export const syncFieldsValidationResult = (
  fields?: SyncFieldsValidationErrors
): SyncFieldsValidationResult =>
  fields && Object.keys(fields).length > 0
    ? {
        fields,
        isValid: areValid(Object.keys(fields).map(k => fields[k].isValid)),
        kind: 'fields'
      }
    : { kind: 'fields', isValid: true, fields: {} };

export const syncListValidationResult = (
  list?: SyncListValidationErrors
): SyncListValidationResult =>
  list && list.length > 0
    ? {
        isValid: areValid(list.map(k => k.isValid)),
        kind: 'list',
        list
      }
    : { kind: 'list', isValid: true, list: [] };

export type SyncEasyValidationResult =
  | boolean
  | string
  | SyncPrimitiveValidationErrors
  | SyncValidationResult;

export type AsyncEasyValidationResult = ConcurrentLike<
  SyncEasyValidationResult
>;

export type EasyValidationResult =
  | SyncEasyValidationResult
  | AsyncEasyValidationResult;

export type EasyValidator<T> = (value: T) => EasyValidationResult;

export const isSyncValidationErrors = (
  value: unknown
): value is SyncPrimitiveValidationErrors =>
  Array.isArray(value) && value.every(v => typeof v === 'string');

export const isSyncValidationResult = (
  value: unknown
): value is SyncValidationResult =>
  value instanceof Object && 'isValid' in value;

export const isSyncPrimitiveValidationResult = (
  value: unknown
): value is SyncPrimitiveValidationResult =>
  isSyncValidationResult(value) && value.kind === 'primitive';

export const isSyncFieldsValidationResult = (
  value: unknown
): value is SyncFieldsValidationResult =>
  isSyncValidationResult(value) && value.kind === 'fields';

export const isSyncListValidationResult = (
  value: unknown
): value is SyncListValidationResult =>
  isSyncValidationResult(value) && value.kind === 'list';

export const matchSyncEasyValidationResult = <R = unknown>(cases: {
  readonly boolean?: (s: boolean) => R;
  readonly string?: (v: string) => R;
  readonly errors?: (v: SyncPrimitiveValidationErrors) => R;
  readonly result?: (v: SyncValidationResult) => R;
  readonly unknown?: (v: unknown) => R;
}) => (value: unknown) => {
  if (cases.boolean && typeof value === 'boolean') {
    return cases.boolean(value);
  } else if (cases.string && typeof value === 'string') {
    return cases.string(value);
  } else if (cases.result && isSyncValidationResult(value)) {
    return cases.result(value);
  } else if (cases.errors && isSyncValidationErrors(value)) {
    return cases.errors(value);
  } else {
    return matchUnknown(cases.unknown)(value);
  }
};

export const toSyncEasyValidationResult = (
  value: SyncEasyValidationResult
): SyncValidationResult =>
  matchSyncEasyValidationResult({
    boolean: b =>
      b
        ? syncPrimitiveValidationResult(['Error'])
        : syncPrimitiveValidationResult(),
    errors: es => syncPrimitiveValidationResult(es),
    result: r => r,
    string: s => syncPrimitiveValidationResult([s])
  })(value);

export const matchEasyValidationResult = <R = unknown>(cases: {
  readonly sync?: (s: SyncEasyValidationResult) => R;
  readonly concurrent?: (v: AsyncEasyValidationResult) => R;
  readonly unknown?: (v: unknown) => R;
}) => (value: unknown) => {
  if (cases.sync && isSyncValidationResult(value)) {
    return cases.sync(value);
  } else if (
    cases.concurrent &&
    isConcurrent<SyncEasyValidationResult>(value)
  ) {
    return cases.concurrent(value);
  } else {
    return matchUnknown(cases.unknown)(value);
  }
};

export const toAsyncEasyValidationResult = (
  value: EasyValidationResult
): AsyncEasyValidationResult =>
  matchEasyValidationResult({
    concurrent: c => c,
    sync: s => from([s])
  })(value);

export type ValidationResult = Observable<SyncValidationResult>;

export type Validator<T = unknown> = (value: T) => ValidationResult;

export const validator = <T = unknown>(
  easyValidator: EasyValidator<T>
): Validator<T> => (value: T) =>
  toObservable(toAsyncEasyValidationResult(easyValidator(value))).pipe(
    map(toSyncEasyValidationResult)
  );
