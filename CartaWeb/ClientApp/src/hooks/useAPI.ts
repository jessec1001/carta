import { useEffect, useMemo, useRef } from "react";
import { DataAPI, UserAPI, WorkspaceAPI } from "library/api";
import useStoredState from "./useStoredState";

/**
 * Returns the values of current API references with any options that may be specified to construct these APIs.
 * @returns References to APIs.
 */
const useAPI = () => {
  // Specificially for the data API, we need to incorporate the integration keys.
  const [hyperthoughtKey] = useStoredState("", "hyperthoughtKey");
  const hyperthoughtResource = useMemo(
    () => ({
      source: "HyperThought",
      resource: undefined,
      parameters: new Map([["api", hyperthoughtKey]]),
    }),
    [hyperthoughtKey]
  );

  const dataResourceIdentifiers = useMemo(
    () => [hyperthoughtResource],
    [hyperthoughtResource]
  );

  // Create the API references.
  const dataAPIRef = useRef(new DataAPI(dataResourceIdentifiers));
  const userAPIRef = useRef(new UserAPI());
  const workspaceAPIRef = useRef(new WorkspaceAPI());

  // Make sure to update the data API when its resource identifiers change.
  useEffect(() => {
    dataAPIRef.current = new DataAPI(dataResourceIdentifiers);
  }, [dataResourceIdentifiers]);

  // Return their current value.
  return {
    dataAPI: dataAPIRef.current,
    userAPI: userAPIRef.current,
    workspaceAPI: workspaceAPIRef.current,
  };
};

export default useAPI;
