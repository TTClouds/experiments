export interface ValidationImmediateResultData {
  readonly isValid: boolean | undefined;
}

export interface ValidationImmediateResult<T>
  extends ValidationImmediateResultData {
  readonly value: T;
}

export type Validator<T = unknown> = (value: T) => ValidationImmediateResult<T>;
