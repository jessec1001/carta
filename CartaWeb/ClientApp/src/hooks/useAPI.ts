import React, { useMemo, useRef } from "react";
import { DataAPI, UserAPI, WorkflowAPI, WorkspaceAPI } from "library/api";
import useStoredState from "./useStoredState";

/**
 * Returns the values of current API references with any options that may be specified to construct these APIs.
 * @returns References to APIs.
 */
const useAPI = () => {
  // Define the API references.
  let dataAPIRef: React.MutableRefObject<DataAPI>;
  let userAPIRef: React.MutableRefObject<UserAPI>;
  let workspaceAPIRef: React.MutableRefObject<WorkspaceAPI>;
  let workflowAPIRef: React.MutableRefObject<WorkflowAPI>;

  // Specificially for the data API, we need to incorporate the integration keys.
  const [hyperthoughtKey] = useStoredState("", "hyperthoughtKey");
  const hyperthoughtResource = useMemo(
    () => ({
      source: "HyperThought",
      parameters: new Map([["api", hyperthoughtKey]]),
    }),
    [hyperthoughtKey]
  );

  // Create the dynamic API references.
  // Make sure to update the data API when its resource identifiers change.
  dataAPIRef = useRef(new DataAPI());
  dataAPIRef.current = useMemo(() => {
    const dataResourceIdentifiers = [hyperthoughtResource];
    return new DataAPI(dataResourceIdentifiers);
  }, [hyperthoughtResource]);

  // Create the static API references.
  userAPIRef = useRef(new UserAPI());
  workspaceAPIRef = useRef(new WorkspaceAPI());
  workflowAPIRef = useRef(new WorkflowAPI());

  // Return their current value.
  return {
    dataAPI: dataAPIRef.current,
    userAPI: userAPIRef.current,
    workspaceAPI: workspaceAPIRef.current,
    workflowAPI: workflowAPIRef.current,
  };
};

export default useAPI;
