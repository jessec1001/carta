import { useCallback, useEffect, useState } from "react";

/**
 * The promisable type represents a value where any element or subproperty can be obtained by calling a function that
 * returns a promise. For any particular type, it represents one of the following:
 * 1. A simple value.
 * 2: A function that returns a promise for a simple value of the same type.
 * 3. An object where each subproperty is a promisable value.
 * 4. An array of promisable values matching the original array type.
 */
type Promisable<TData, TTotal = TData> =
  | PromisableValue<TData>
  | PromisableFunction<TData, TTotal>
  | PromisableArray<TData, TTotal>
  | PromisableObject<TData, TTotal>;

type PromisableValue<TData> = TData;
type PromisableFunction<TData, TTotal> = (
  value: any
) => Promise<Promisable<TData, TTotal>>;
type PromisableArray<TData, TTotal> = TData extends (infer TElement)[]
  ? Promisable<TElement, TTotal>[]
  : never;
type PromisableObject<TData, TTotal> = {
  [key in keyof TData]: Promisable<TData[key], TTotal>;
};

/**
 * The promised type changes the value of any promisable field or value to undefined, the original type, or an error.
 */
type Promised<
  TPromise extends Promisable<TData, TTotal>,
  TData,
  TTotal = TData
> = TPromise extends PromisableValue<TData>
  ? TData
  : TPromise extends (value: any) => Promise<infer TSubpromise>
  ? TSubpromise extends Promisable<TData, TTotal>
    ? Promised<TSubpromise, TData, TTotal> | Error | undefined
    : never
  : TData extends (infer TElement)[]
  ? TPromise extends (infer TPromiseElement)[]
    ? TPromiseElement extends Promisable<TElement, TTotal>
      ? Promised<TPromiseElement, TElement, TTotal>[]
      : never
    : never
  : TPromise extends PromisableObject<TData, TTotal>
  ? {
      [key in keyof TData]: Promised<TPromise[key], TData[key], TTotal>;
    }
  : never;

/**
 * Performs a nested copy of a value and optionally sets the final value.
 * @param value The value to copy.
 * @param path The path to copy along.
 * @param set The value to set if any.
 * @returns The copied value.
 */
const nestedCopy = (value: any, path: (string | number)[], set?: any): any => {
  if (path.length === 0) return set === undefined ? value : set;
  const [head, ...tail] = path;
  if (typeof head === "string")
    return {
      ...value,
      [head]: nestedCopy(value[head], tail, set),
    };
  else if (typeof head === "number")
    return [
      ...value.slice(0, head),
      nestedCopy(value[head], tail, set),
      ...value.slice(head + 1),
    ];
  else return value;
};

/**
 * Loads a nested value asynchronously. This is particularly useful for loading values that need to be retrieved using
 * multiple endpoints or have parts of values that can remain unloaded for a short period of time.
 * @param promise The promise which may be a value, array, dictionary, or asynchronous function returning one of these.
 * @param refreshInterval The interval at which to refresh the value.
 * @returns A tuple containing the value and a function to refresh the value.
 */
const useNestedAsync = <TPromise extends Promisable<TData>, TData>(
  promise: TPromise,
  refreshInterval?: number
): [Promised<TPromise, TData>, () => Promise<void>] => {
  // Defines how to retrieve the default value for a promise.
  const defaultResolve = useCallback(
    <TPromise extends Promisable<TData, TTotal>, TData, TTotal = TData>(
      promise: TPromise
    ): Promised<TPromise, TData, TTotal> => {
      if (typeof promise === "function")
        return undefined as Promised<TPromise, TData, TTotal>;
      else if (Array.isArray(promise)) {
        const promiseArray = promise as PromisableArray<TData, TTotal>;
        const array: any[] = [];
        for (const element of promiseArray) {
          array.push(defaultResolve(element));
        }
        return array as Promised<TPromise, TData, TTotal>;
      } else if (typeof promise === "object") {
        const promiseObject = promise as PromisableObject<TData, TTotal>;
        const object: Record<string, any> = {};
        Object.entries(promiseObject).forEach(([key, value]) => {
          object[key] = defaultResolve(value);
        });
        return object as Promised<TPromise, TData, TTotal>;
      } else return promise as Promised<TPromise, TData, TTotal>;
    },
    []
  );

  // We store the result of the promise and we update it when possible.
  // We use a default value to initialize the state.
  const [result, setResult] = useState<Promised<TPromise, TData>>(
    defaultResolve<TPromise, TData>(promise)
  );

  // Resolves an actual promise and sets the result in state automatically.
  const resolve = useCallback(
    async <TPromise extends Promisable<TData, TTotal>, TData, TTotal = TData>(
      promise: TPromise,
      path: (string | number)[] = []
    ) => {
      if (typeof promise === "function") {
        const promiseFunction = promise as PromisableFunction<TData, TTotal>;
        setResult((result) => {
          promiseFunction(result)
            .then((value) =>
              setResult((result) => nestedCopy(result, path, value))
            )
            .catch((error: Error) =>
              setResult((result) => nestedCopy(result, path, error))
            );
          return nestedCopy(result, path, undefined);
        });
      } else if (Array.isArray(promise)) {
        const promiseArray = promise as PromisableArray<TData, TTotal>;
        setResult((result) => nestedCopy(result, path, []));
        await Promise.all(
          promiseArray.map(
            async (subpromise, index) =>
              await resolve(subpromise, [...path, index])
          )
        );
      } else if (typeof promise === "object") {
        const promiseObject = promise as PromisableObject<TData, TTotal>;
        setResult((result) => nestedCopy(result, path, {}));
        await Promise.all(
          Object.entries(promiseObject).map(
            async ([key, subpromise]) =>
              await resolve(subpromise, [...path, key])
          )
        );
      } else {
        const promiseValue = promise as PromisableValue<TData>;
        setResult((result) => {
          return nestedCopy(result, path, promiseValue);
        });
      }
    },
    []
  );

  // When the component is initially mounted, or the promise changes (rare), we load the data.
  const refresh = useCallback(async () => {
    await resolve<TPromise, TData>(promise);
  }, [resolve, promise]);
  useEffect(() => {
    refresh();
  }, [refresh]);

  // When the refresh interval is specified, we will update the data using the promise periodically.
  useEffect(() => {
    if (refreshInterval !== undefined) {
      const intervalId = setInterval(refresh, refreshInterval);
      return () => clearInterval(intervalId);
    }
  }, [refresh, refreshInterval]);

  // We return the result of the loading function and a manual refresh callback.
  return [result, refresh];
};

export default useNestedAsync;
