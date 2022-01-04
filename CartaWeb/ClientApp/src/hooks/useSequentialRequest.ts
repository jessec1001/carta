import { useCallback, useRef } from "react";

/**
 * Returns a asynchronous request-making function that calls a specified callback when each request is completed. The
 * requests are guaranteed to execute sequentially.
 * @param callback The function that is called back after each successful request.
 * @param cancels Whether later requests should cancel earlier requests and shortcut execution of the callback.
 * @returns A function that should be called to execute new asynchronous sequential requests.
 */
const useSequentialRequest = <T>(
  callback: (value: T) => void,
  cancels?: boolean
) => {
  // This tracks current requests to make sure that the previous request is executed fully before the next.
  // Additionally, if canceling is enabled, the identifier allows us to check if the callback should be called.
  const promise = useRef<Promise<void>>(Promise.resolve());
  const id = useRef<number>(0);

  const makeRequest = useCallback(
    (promiser: () => Promise<T>) => {
      // If there is an existing request, we tack on a new request to the end.
      // This ensures that the callbacks are called sequentially.
      id.current++;
      if (cancels) {
        promise.current = (async (idThis: number) => {
          const value = await promiser();
          if (id.current === idThis) callback(value);
          return;
        })(id.current);
      } else {
        promise.current = promise.current.then(async () => {
          const value = await promiser();
          callback(value);
          return;
        });
      }
    },
    [callback, cancels]
  );
  return makeRequest;
};

export default useSequentialRequest;
