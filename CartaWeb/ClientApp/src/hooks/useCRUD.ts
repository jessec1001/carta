import { useCallback, useEffect, useMemo, useRef, useState } from "react";

/** Specifies asynchronous CRUD operations for a particular data type assume to be in a collection. */
interface DataCRUD<TData> {
  /**
   * Fetches all of the data type in the collection.
   * @returns A collection of data.
   */
  fetch: () => Promise<TData[]>;
  /**
   * Adds the specified data item.
   * @param data The data item to add.
   * @returns The added data item.
   */
  add: (data: TData) => Promise<TData>;
  /**
   * Removes the specified data item.
   * @param data The data item to remove.
   * @returns Nothing.
   */
  remove: (data: TData) => Promise<void>;
  /**
   * Updates the specified data item.
   * @param data The data item to update.
   * @returns The updated data item.
   */
  update: (data: TData) => Promise<TData>;
}
/** A collection of data of a particular type. If `null`, specifies that the data has not yet been loaded. */
type DataValue<TData> = TData[] | null;

/**
 * Creates new fetching operations and state storage for a particular data type. Connects fetching operations together
 * to provide automatic refreshing for data collections, optimized adding and removing, and management of data
 * structures and storage.
 *
 * @param fetcher The fetching functions to use to request CRUD operations on data.
 * @param refetch Whether to refetch the entire collection of data when any data is modified. Defaults to `false`.
 * @param refresh A refresh interval for the data specified in milliseconds.
 * If not specified, data will not automatically refresh.
 * @template TData The type of data.
 * @template TID The property name specifying the identifier of the data type.
 * @returns A tuple containing data and data operations respectively.
 */
const useCRUD = <TData>(
  fetcher: DataCRUD<TData>,
  id: keyof TData = "id" as any,
  refetch: boolean = false,
  refresh?: number
): [DataValue<TData>, DataCRUD<TData>] => {
  // TODO: Use sequential requests to make sure that fetch method always returns most recent version of collection.
  // TODO: Incorporate error detection into this hook to provide an addition error state in addition to loading.
  // TODO: Implement some better way to obtain a singular element from the data collection.

  // The mounted reference helps to cancel asynchronous requests once the component has been unmounted.
  const mountedRef = useRef<boolean>(true);
  // This is the data stored for the specified data type.
  const [data, setData] = useState<TData[] | null>(null);

  /**
   * Fetches the entire collection of data.
   */
  const fetch = useCallback(() => {
    fetcher.fetch().then((value) => {
      if (mountedRef.current) setData(value);
    });
  }, [fetcher]);

  // We update the mounted value when the component unmounts.
  useEffect(() => {
    return () => {
      mountedRef.current = false;
    };
  }, []);

  // We need to initially fetch the data.
  useEffect(() => {
    fetch();
  }, [fetch]);

  // If we have a refresh time specified, we periodically refetch the data.
  useEffect(() => {
    if (refresh !== undefined) {
      const refreshInterval = setInterval(() => fetch, refresh);
      return () => clearInterval(refreshInterval);
    }
  }, [fetch, refresh]);

  // We construct a set of fetcher functions that perform operations connected together logically.
  const fetcherConnected: DataCRUD<TData> = useMemo(() => {
    return {
      // Fetch method operates as default.
      fetch: fetcher.fetch,

      // Other operations modify data so they change state.
      add: async (dataItem: TData) => {
        const result = await fetcher.add(dataItem);
        if (refetch) fetch();
        else {
          setData((data) => {
            // If the data collection is null when trying to add, just rely on getting the entire collection.
            if (data === null) {
              fetch();
              return null;
            }

            // If the data collection is non-null, operate in a more optimized way.
            const newData = [...data, result];
            return newData;
          });
        }
        return result;
      },
      remove: async (dataItem: TData) => {
        await fetcher.remove(dataItem);
        if (refetch) fetch();
        else {
          setData((data) => {
            // If the data collection is null when trying to add, just rely on getting the entire collection.
            if (data === null) {
              fetch();
              return null;
            }

            // If the data collection is non-null, operate in a more optimized way.
            const newData = data.filter(
              (newDataItem) => newDataItem[id] !== dataItem[id]
            );
            return newData.length === data.length ? data : newData;
          });
        }
      },
      update: async (dataItem: TData) => {
        const result = await fetcher.update(dataItem);
        if (refetch) fetch();
        else {
          setData((data) => {
            // If the data collection is null when trying to add, just rely on getting the entire collection.
            if (data === null) {
              fetch();
              return null;
            }

            // If the data collection is non-null, operate in a more optimizaed way.
            const newData = [...data];
            const newDataIndex = newData.findIndex(
              (newDataItem) => newDataItem[id] === dataItem[id]
            );
            if (newDataIndex >= 0) {
              newData[newDataIndex] = result;
            } else {
              newData.push(result);
            }
            return newData;
          });
        }
        return result;
      },
    };
  }, [refetch, fetch, fetcher, id]);

  return [data, fetcherConnected];
};

export default useCRUD;
export type { DataCRUD, DataValue };
