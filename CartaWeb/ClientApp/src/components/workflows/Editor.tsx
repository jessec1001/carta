import { FC, useCallback } from "react";
import { useAPI, useNestedAsync } from "hooks";
import { Operation, OperationType } from "library/api";
import { seconds } from "library/utility";
import { useWorkflows } from "./Context";
import EditorNode from "./EditorNode";

// TODO: Consider if the suboperations should be stored in the context.

const Editor: FC = () => {
  // We get the workflow to use for rendering its components.
  const { workflow } = useWorkflows();

  // Fetch the available types of operations.
  const { operationsAPI } = useAPI();
  const operationTypesFetch = useCallback(async () => {
    const types = await operationsAPI.getOperationTypes();
    return Object.fromEntries(types.map((type) => [type.type, type]));
  }, [operationsAPI]);
  const [operationTypes] = useNestedAsync<
    typeof operationTypesFetch,
    Record<string, OperationType>
  >(operationTypesFetch);

  // Fetch the suboperations of the workflow if possible.
  const suboperationsFetch = useCallback(async () => {
    if (workflow === undefined || workflow instanceof Error) return [];
    return workflow.operations.map((operationId) => async () => {
      const suboperation = await operationsAPI.getOperation(operationId);
      return suboperation;
    });
  }, [operationsAPI, workflow]);
  const [suboperations] = useNestedAsync<
    typeof suboperationsFetch,
    Operation[]
  >(suboperationsFetch, false, seconds(30));

  return (
    <>
      {!(suboperations === undefined || suboperations instanceof Error) &&
        suboperations.map((operation) => {
          if (operation === undefined || operation instanceof Error)
            return null;
          if (operationTypes === undefined || operationTypes instanceof Error)
            return null;
          return (
            <EditorNode
              key={operation.id}
              operation={operation}
              type={operationTypes[operation.type]}
            />
          );
        })}
    </>
  );
};

export default Editor;
