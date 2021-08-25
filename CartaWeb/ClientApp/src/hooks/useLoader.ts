import { useCallback, useEffect, useState } from "react";
import useMounted from "./useMounted";

/**
 * Uses an asynchronous loading function to load some particular data into state for use elsewhere.
 * Returns the data, if loaded successfully, or an error, if loaded unsuccessfully.
 * @param load The loading function.
 * @param refreshInterval An optional interval that can be specified for automatically refreshing the data. If not
 * specified, the data will not be automatically refreshed.
 * @returns A tuple containing the data and error respectively. If still loading, both the data and error will be
 * `null`. If loading succeeded, the data will be set and the error will be `undefined`. If loading failed, the error
 * will be set and the data will be `undefined`.
 */
const useLoader = <TData>(
  load: () => Promise<TData>,
  refreshInterval?: number
): [TData | undefined, Error | undefined] => {
  // We store information about the data or error retrieved from the loading function.
  const mounted = useMounted();
  const [results, setResults] = useState<{ data?: TData; error?: Error }>({});
  const { data, error } = results;

  // This callback function allows us to safely execute an asynchronous function that sets state
  // by verifying whether the component is still mounted.
  const mountedLoad = useCallback(() => {
    load()
      .then((data) => mounted.current && setResults({ data }))
      .catch((error) => mounted.current && setResults({ error }));
  }, [load, mounted]);

  // When the component is initially mounted, or the loading function changes (rare), we load the data.
  useEffect(() => {
    mountedLoad();
  }, [mountedLoad]);

  // If a refresh interval was specified, we will update the data using the loader function periodically.
  useEffect(() => {
    if (refreshInterval !== undefined) {
      const intervalId = setInterval(mountedLoad, refreshInterval);
      return () => clearInterval(intervalId);
    }
  }, [mountedLoad, refreshInterval]);

  // We return the current data and error.
  return [data, error];
};

export default useLoader;
