import { useCallback, useEffect, useState } from "react";

/**
 * The promisable type represents a value where any element or subproperty can be obtained by calling a function that
 * returns a promise. For any particular type, it represents one of the following:
 * 1. A simple value.
 * 2: A function that returns a promise for a simple value of the same type.
 * 3. An object where each subproperty is a promisable value.
 * 4. An array of promisable values matching the original array type.
 */
type Promisable<TData> =
  | PromisableValue<TData>
  | PromisableArray<TData>
  | PromisableObject<TData>
  | PromisableFunction<TData>;

type PromisableValue<TData> = TData;
type PromisableArray<TData> = TData extends (infer TElement)[]
  ? Promisable<TElement>[]
  : never;
type PromisableObject<TData> = {
  [key in keyof TData]: Promisable<TData[key]>;
};
type PromisableFunction<TData> = () => Promise<Promisable<TData>>;

/**
 * The promised type changes the value of any promisable field or value to undefined, the original type, or an error.
 */
type Promised<
  TPromise extends Promisable<TData>,
  TData
> = TPromise extends PromisableValue<TData>
  ? TData
  : TPromise extends () => Promise<infer TSubpromise>
  ? TSubpromise extends Promisable<TData>
    ? Promised<TSubpromise, TData> | Error | undefined
    : never
  : TData extends (infer TElement)[]
  ? TPromise extends (infer TPromiseElement)[]
    ? TPromiseElement extends Promisable<TElement>
      ? Promised<TPromiseElement, TElement>[]
      : never
    : never
  : TPromise extends PromisableObject<TData>
  ? {
      [key in keyof TData]: Promised<TPromise[key], TData[key]>;
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
 * @param buffered Whether the value should be buffered. This means that if the value is being refreshed, the current value will not be updated until the refresh is complete.
 * @param refreshInterval The interval at which to refresh the value.
 * @returns A tuple containing the value and a function to refresh the value.
 */
const useNestedAsync = <TPromise extends Promisable<TData>, TData>(
  promise: TPromise,
  buffered?: boolean,
  refreshInterval?: number
): [Promised<TPromise, TData>, () => Promise<Promised<TPromise, TData>>] => {
  // Defines how to retrieve the default value for a promise.
  const defaults = useCallback(
    <TPromise extends Promisable<TData>, TData>(
      promise: TPromise
    ): Promised<TPromise, TData> => {
      if (typeof promise === "function")
        return undefined as Promised<TPromise, TData>;
      else if (Array.isArray(promise)) {
        const promiseArray = promise as PromisableArray<TData>;
        const array: any[] = [];
        for (const element of promiseArray) {
          array.push(defaults(element));
        }
        return array as Promised<TPromise, TData>;
      } else if (
        typeof promise === "object" &&
        Object.getPrototypeOf(promise) === Object.prototype
      ) {
        const promiseObject = promise as PromisableObject<TData>;
        const object: Record<string, any> = {};
        Object.entries(promiseObject).forEach(([key, value]) => {
          object[key] = defaults(value);
        });
        return object as Promised<TPromise, TData>;
      } else return promise as Promised<TPromise, TData>;
    },
    []
  );

  // We store the result of the promise and we update it when possible.
  // We use a default value to initialize the state.
  const [result, setResult] = useState<Promised<TPromise, TData>>(
    defaults<TPromise, TData>(promise)
  );

  // Defines how asynchronous functions are resolved.
  const resolve = useCallback(
    async <TPromise extends Promisable<TData>, TData>(
      promise: TPromise,
      path: (string | number)[] = []
    ): Promise<Promised<TPromise, TData>> => {
      // We store the resolved value here.
      let resolved: Promised<TPromise, TData>;

      try {
        if (typeof promise === "function") {
          const promiseFunction = promise as PromisableFunction<TData>;
          try {
            const value = await promiseFunction();
            if (!buffered)
              setResult((result) => nestedCopy(result, path, defaults(value)));
            resolved = (await resolve(value, path)) as Promised<
              TPromise,
              TData
            >;
          } catch (error: any) {
            if (!buffered)
              setResult((result) => nestedCopy(result, path, defaults(error)));
            resolved = (await resolve(error, path)) as Promised<
              TPromise,
              TData
            >;
          }
        } else if (Array.isArray(promise)) {
          const promiseArray = promise as PromisableArray<TData>;
          resolved = (await Promise.all(
            promiseArray.map(
              async (subpromise, index) =>
                await resolve(subpromise as any, [...path, index])
            )
          )) as Promised<TPromise, TData>;
        } else if (
          typeof promise === "object" &&
          Object.getPrototypeOf(promise) === Object.prototype
        ) {
          const promiseObject = promise as PromisableObject<TData>;
          resolved = Object.fromEntries(
            await Promise.all(
              Object.entries(promiseObject).map(async ([key, subpromise]) => [
                key,
                await resolve(subpromise as any, [...path, key]),
              ])
            )
          ) as Promised<TPromise, TData>;
        } else {
          const promiseValue = promise as PromisableValue<TData>;
          resolved = promiseValue as Promised<TPromise, TData>;
        }
      } catch (error: any) {
        console.log("ERROR", promise, path);
        throw error;
      }

      // If the value is buffered, we only update the value if the promise is resolved.
      if (buffered && !path.length)
        setResult((result) => nestedCopy(result, path, resolved));

      // We return the resolved value.
      return resolved;
    },
    [buffered, defaults]
  );

  // When the component is initially mounted, or the promise changes (rare), we load the data.
  const refresh = useCallback(
    async () => resolve<TPromise, TData>(promise),
    [resolve, promise]
  );

  // When the hook is mounted, we load the data.
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
export type { Promisable, Promised };
