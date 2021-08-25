import { useCallback, useRef } from "react";

/**
 * Returns a asynchronous request-making function that calls a specified callback when each request is completed. The
 * requests are guaranteed to execute sequentially.
 * @param callback The function that is called back after each successful request.
 * @returns A function that should be called to execute new asynchronous sequential requests.
 */
const useSequentialRequest = <T>(callback: (value: T) => void) => {
  // This tracks current requests to make sure that the previous request is executed fully before the next.
  const promise = useRef<Promise<void>>(Promise.resolve());

  const makeRequest = useCallback(
    (request: Promise<T>) => {
      // If there is an existing request, we tack on a new request to the end.
      // This ensures that the callbacks are called sequentially.
      promise.current = promise.current.then(async () => {
        const value = await request;
        callback(value);
        return;
      });
    },
    [callback]
  );
  return makeRequest;
};

export default useSequentialRequest;
