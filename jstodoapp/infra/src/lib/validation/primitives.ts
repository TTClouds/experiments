import { getAsValue } from '../concurrency';
import { validator } from './core';

export type Message<T> = string | undefined | ((v: T) => string | undefined);
export type Message1<T, T1> =
  | string
  | undefined
  | ((v: T, p1: T1) => string | undefined);
export type Message2<T, T1, T2> =
  | string
  | undefined
  | ((v: T, p1: T1, p2: T2) => string | undefined);

export const isNothing = (value: any) => value === undefined || value === null;

export const conditional = <T>(
  cond: (value: T) => boolean,
  message: Message<T>
) => (value: T) => (cond(value) ? true : getAsValue(message, value) || 'Error');

export const conditional1 = <T, T1>(
  cond: (value: T) => boolean,
  param1: T1,
  message: Message1<T, T1>
) => (value: T) =>
  cond(value) ? true : getAsValue(message, value, param1) || 'Error';

export const conditional2 = <T, T1, T2>(
  cond: (value: T) => boolean,
  param1: T1,
  param2: T2,
  message: Message2<T, T1, T2>
) => (value: T) =>
  cond(value) ? true : getAsValue(message, value, param1, param2) || 'Error';

export const isRequiredCond = (message?: Message<any>) =>
  conditional<any>(value => !isNothing(value), message || 'is required');

export const isRequired = (message?: Message<any>) =>
  validator<any>(isRequiredCond(message));

export const isNotEmptyCond = (message?: Message<string>) =>
  conditional<string>(
    value => isNothing(value) || value !== '',
    message || 'must not be empty'
  );

export const isNotEmpty = (message?: Message<string>) =>
  validator<string>(isNotEmptyCond(message));

export const isNotBlankCond = (message?: Message<string>) =>
  conditional<string>(
    value => isNothing(value) || value.trim() !== '',
    message || 'must not be blank'
  );

export const isNotBlank = (message?: Message<string>) =>
  validator<string>(isNotBlankCond(message));

export const mustHaveLengthCond = (
  minLength: number,
  maxLength: number,
  message?: Message2<string, number, number>
) =>
  conditional2<string, number, number>(
    value =>
      isNothing(value) ||
      (value.length >= minLength && value.length <= maxLength),
    minLength,
    maxLength,
    message ||
      ((str, min, max) =>
        `must have between ${min} and ${max} characters, but instead have ${
          str.length
        }`)
  );

export const mustHaveLength = (
  minLength: number,
  maxLength: number,
  message?: Message2<string, number, number>
) => validator<string>(mustHaveLengthCond(minLength, maxLength, message));

export const mustHaveMinLengthCond = (
  minLength: number,
  message?: Message1<string, number>
) =>
  conditional1<string, number>(
    value => isNothing(value) || value.length >= minLength,
    minLength,
    message ||
      ((str, min) =>
        `must have at least ${min} characters, but instead have ${str.length}`)
  );

export const mustHaveMinLength = (
  minLength: number,
  message?: Message1<string, number>
) => validator<string>(mustHaveMinLengthCond(minLength, message));

export const mustHaveMaxLengthCond = (
  maxLength: number,
  message?: Message1<string, number>
) =>
  conditional1<string, number>(
    value => isNothing(value) || value.length <= maxLength,
    maxLength,
    message ||
      ((str, max) =>
        `must have at most ${max} characters, but instead have ${str.length}`)
  );

export const mustHaveMaxLength = (
  maxLength: number,
  message?: Message1<string, number>
) => validator<string>(mustHaveMaxLengthCond(maxLength, message));
