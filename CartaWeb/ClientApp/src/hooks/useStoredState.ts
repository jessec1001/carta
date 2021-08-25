import {
  Dispatch,
  SetStateAction,
  useCallback,
  useEffect,
  useState,
} from "react";

/**
 * Returns a stateful value, and a function to update it. This stateful value is stored under local storage with a
 * specified key and updated alongside the stateful value. If storage is not available (for instance if a web
 * environment is not running), functions identically to {@link useState}.
 * @param initialValue The initial value to be assigned to the state.
 * @param storageKey The key in local storage to store the state value under.
 */
const useStoredState = <T>(
  initialValue: T | (() => T),
  storageKey: string,
  storage?: Storage
): [T, Dispatch<SetStateAction<T>>] => {
  // We use local storage as the default storage. The hook user can pick another storage if desired.
  const stateStorageArea = storage ?? localStorage;

  // Based on whether this is running client-side or server-side, we may not have access to the local storage
  // or otherwise. In this case, we should use the same behavior as a default state hooks as a fallback.
  const defaultValue = () => {
    let json = stateStorageArea.getItem(storageKey);

    // If we couldn't find the value in local storage, we use the initial value.
    if (json === null) {
      // Notice that the initial value could be a function in which case we call it to get our value.
      const initial =
        typeof initialValue === "function"
          ? (initialValue as () => T)()
          : initialValue;

      // We store this value into local storage to initialize it in this case.
      json = JSON.stringify(initial);
      stateStorageArea.setItem(storageKey, json);

      return initial;
    }

    // If we found the value in local storage, assume it is parsed into the correct type.
    let value: T;
    try {
      value = JSON.parse(json);
    } catch {
      value = "" as unknown as T;
    }
    return value;
  };

  // Find the initial value from the local storage if possible or use the provided initial value otherwise.
  const [stateValue, setStateValue] = useState<T>(defaultValue);

  // Once we've loaded the state for the first time from local storage, we only need to worry about saving.
  // We wrap the original dispatch function in another function that saves the value to local storage.
  const handleValue: Dispatch<SetStateAction<T>> = useCallback(
    (value: T | ((prevValue: T) => T)) => {
      setStateValue((prevValue: T) => {
        // Compute the next value depending on whether we are using a callback function or a simple value.
        const nextValue =
          typeof value === "function"
            ? (value as (prevValue: T) => T)(prevValue)
            : value;

        // We check if the value has changed. If it has, we save it to local storage.
        if (!Object.is(prevValue, nextValue)) {
          const json = JSON.stringify(nextValue);
          stateStorageArea.setItem(storageKey, json);
        }

        // We return our newly computed value.
        return nextValue;
      });
    },
    [storageKey, stateStorageArea]
  );

  // Add an event listener to detect changes to the storage.
  // If our value has been modified, we should set the state.
  useEffect(() => {
    const storageHandler = ({ key, storageArea }: StorageEvent) => {
      if (key === storageKey && storageArea === stateStorageArea) {
        const json = storageArea.getItem(key);
        const value = JSON.parse(json ?? "");

        // Notice that we do not call the dispatch function so that we can avoid infinite recursion.
        setStateValue(value);
      }
    };

    window.addEventListener("storage", storageHandler);
    return () => window.removeEventListener("storage", storageHandler);
  }, [storageKey, stateStorageArea]);

  // Return the same signature as the use state React hook.
  return [stateValue, handleValue];
};

export default useStoredState;
