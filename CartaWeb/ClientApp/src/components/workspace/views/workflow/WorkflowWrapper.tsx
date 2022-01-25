import { useAPI } from "hooks";
import { Operation, Job, OperationType, Workflow } from "library/api";
import { useEffect } from "react";
import { useState } from "react";
import { FunctionComponent } from "react";
import WorkflowContext from "./WorkflowContext";

interface WorkflowWrapperProps {
  workflow: Workflow;
  operation: Operation;
  job?: Job;

  onOperationUpdate?: (operation: Operation) => void;
  onWorkflowUpdate?: (workflow: Workflow) => void;
  onJobUpdate?: (job: Job) => void;
}

const WorkflowWrapper: FunctionComponent<WorkflowWrapperProps> = ({
  workflow,
  operation,
  job,
  onOperationUpdate,
  onWorkflowUpdate,
  onJobUpdate,
  children,
}) => {
  const { operationsAPI, workflowsAPI } = useAPI();
  const [operations, setOperations] = useState<Operation[]>([]);
  const [operationTypes, setOperationTypes] = useState<OperationType[]>([]);
  const [input, setInput] = useState<Record<string, any>>({});
  const [output, setOutput] = useState<Record<string, any>>({});
  const [auto, setAuto] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [fileField, setFileField] = useState<string | null>(null);
  const [jobId, setJobId] = useState<string | null>(null);

  const [selected, setSelected] = useState<Operation[]>([]);
  const [connecting, setConnecting] = useState<{
    operation: string;
    field: string;
    side: "input" | "output";
  } | null>(null);

  // Get the operation types for all operations.
  useEffect(() => {
    operationsAPI
      .getOperationTypes()
      .then((operationTypes) => setOperationTypes(operationTypes));
  }, [operationsAPI]);

  // Get the operations for the workflow.
  useEffect(() => {
    const fetchNewOperations = async () => {
      // Notice that if no operations are added, the state is not updated.
      let newOperations = operations;
      for (const operationId of workflow.operations) {
        // Check if the operation is already in the list.
        if (operations.find((op) => op.id === operationId)) continue;

        // Get the operation if it's not in the list.
        const operation = await operationsAPI.getOperation(operationId);
        newOperations = [...newOperations, operation];
      }

      // Update the state.
      setOperations(newOperations);
    };

    fetchNewOperations();
  }, [operations, operationsAPI, workflow.operations]);

  // TODO: Add delay and synchronicity to this.
  const handleExecuteWorkflow = async (): Promise<Job | null> => {
    let localInput = input ?? {};
    console.log(localInput, fileField);
    if (fileField) {
      delete localInput[fileField];
    }
    let job = await operationsAPI.executeOperation(
      operation.id,
      localInput ?? {}
    );
    if (fileField) {
      await operationsAPI.uploadFile(operation.id, job.id, file!);
    }
    job = await operationsAPI.getOperationJobWhenComplete(operation.id, job.id);
    setOutput(job.result!);
    setJobId(job.id);
    return job;
  };
  const handleAutoUpdate = (value: boolean) => {
    if (value) handleExecuteWorkflow();
    setAuto(value);
  };

  return (
    <WorkflowContext.Provider
      value={{
        workflow,
        workflowOperation: operation,
        input,
        output,
        file,
        fileField,
        setFile,
        setFileField,
        jobId,
        setJobId,
        autoUpdate: auto,
        setInputField: (key: string, value: any) => {
          setInput((input) => ({ ...input, [key]: value }));
        },
        getOutputField: (key: string) => {
          return output[key];
        },
        setAutoUpdate: handleAutoUpdate,
        operations: operations,
        operationTypes: operationTypes,
        selected: selected,
        connecting: connecting,
        select: (operations: Operation[]) => {
          setSelected(operations);
        },
        executeWorkflow: handleExecuteWorkflow,

        addOperation: async (type: string, subtype: string | null) => {
          let operationInstance = await operationsAPI.createOperation(
            type,
            subtype
          );
          operationInstance = await operationsAPI.getOperation(
            operationInstance.id
          );
          setOperations((operations) => {
            return [...operations, operationInstance];
          });
          if (workflow) {
            await workflowsAPI.addWorkflowOperation(
              workflow.id,
              operationInstance.id
            );
            const workflowUpdated = await workflowsAPI.getWorkflow(workflow.id);
            if (onWorkflowUpdate) onWorkflowUpdate(workflowUpdated);
            const operationUpdated = await operationsAPI.getOperation(
              operation.id
            );
            if (onOperationUpdate) onOperationUpdate(operationUpdated);
          }
          if (auto) handleExecuteWorkflow();
          return operationInstance;
        },
        updateOperation: async (operationId: string, defaults: any) => {
          let operationUpdated = await operationsAPI.updateOperation({
            id: operationId,
            default: defaults,
          });
          operationUpdated = await operationsAPI.getOperation(operationId);
          setOperations((operations) => {
            const operationIndex = operations.findIndex(
              (operation) => operation.id === operationId
            );
            operations[operationIndex] = operationUpdated;
            return [...operations];
          });
          if (auto) handleExecuteWorkflow();
          const workflowOperationUpdated = await operationsAPI.getOperation(
            operation.id
          );
          if (onOperationUpdate) onOperationUpdate(workflowOperationUpdated);
          return operationUpdated;
        },
        removeOperation: async (operationId: string) => {
          if (workflow) {
            await workflowsAPI.removeWorkflowOperation(
              workflow.id,
              operationId
            );
            const workflowUpdated = await workflowsAPI.getWorkflow(workflow.id);
            if (onWorkflowUpdate) onWorkflowUpdate(workflowUpdated);
            const workflowOperationUpdated = await operationsAPI.getOperation(
              operation.id
            );
            if (onOperationUpdate) onOperationUpdate(workflowOperationUpdated);
          }
          await operationsAPI.deleteOperation(operationId);
          setOperations((operations) =>
            operations.filter((operation) => operation.id !== operationId)
          );
          if (auto) handleExecuteWorkflow();
        },

        tryConnect: async (
          operationId: string,
          field: string,
          side: "input" | "output"
        ) => {
          if (workflow) {
            if (connecting) {
              if (connecting.side !== side) {
                // Form connection.
                const source =
                  connecting.side === "output"
                    ? {
                        operation: connecting.operation,
                        field: connecting.field,
                      }
                    : { operation: operationId, field: field };
                const target =
                  connecting.side === "input"
                    ? {
                        operation: connecting.operation,
                        field: connecting.field,
                      }
                    : { operation: operationId, field: field };

                const connection = await workflowsAPI.suggestWorkflowConnection(
                  workflow.id,
                  {
                    source,
                    target,
                  }
                );
                await workflowsAPI.addWorkflowConnection(workflow.id, {
                  source: connection.source,
                  target: connection.target,
                  multiplex: connection.multiplex,
                });
                const workflowUpdated = await workflowsAPI.getWorkflow(
                  workflow.id
                );
                if (onWorkflowUpdate) onWorkflowUpdate(workflowUpdated);
                const workflowOperationUpdated =
                  await operationsAPI.getOperation(operation.id);
                if (onOperationUpdate)
                  onOperationUpdate(workflowOperationUpdated);
                setConnecting(null);
              }
            } else {
              setConnecting({
                operation: operationId,
                field: field,
                side: side,
              });
            }
          }
        },
        cancelConnect: () => {
          setConnecting(null);
        },
        addConnection: () => {},
        removeConnection: async (connectionId: string) => {
          if (workflow) {
            await workflowsAPI.removeWorkflowConnection(
              workflow.id,
              connectionId
            );
            const workflowUpdated = await workflowsAPI.getWorkflow(workflow.id);
            if (onWorkflowUpdate) onWorkflowUpdate(workflowUpdated);
            const workflowOperationUpdated = await operationsAPI.getOperation(
              operation.id
            );
            if (onOperationUpdate) onOperationUpdate(workflowOperationUpdated);
          }
        },
      }}
    >
      {children}
    </WorkflowContext.Provider>
  );
};

export default WorkflowWrapper;
