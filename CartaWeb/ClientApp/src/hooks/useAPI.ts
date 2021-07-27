import { useRef } from "react";
import { UserAPI, WorkspaceAPI } from "library/api";

/**
 * Returns the values of current API references with any options that may be specified to construct these APIs.
 * @returns References to APIs.
 */
const useAPI = () => {
  // Create the API references.
  const userAPIRef = useRef(new UserAPI());
  const workspaceAPIRef = useRef(new WorkspaceAPI());

  // Return their current value.
  return {
    userAPI: userAPIRef.current,
    workspaceAPI: workspaceAPIRef.current,
  };
};

export default useAPI;
