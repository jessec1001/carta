import { generalRequest } from "../carta";
import { Workflow, WorkflowOperation } from "../types/workflow";

export async function workflowGet(parameters?: Record<string, any>) {
  return (await generalRequest(
    `api/workflow`,
    parameters,
    undefined,
    "get"
  )) as Workflow[];
}
export async function workflowCreate(
  workflow: Workflow,
  parameters?: Record<string, any>
) {
  return (await generalRequest(
    `api/workflow`,
    parameters,
    workflow,
    "post"
  )) as Workflow;
}
export async function workflowOperationAppend(
  workflowId: number,
  operation: WorkflowOperation,
  parameters?: Record<string, any>
) {
  return (await generalRequest(
    `api/workflow/${workflowId}/operations`,
    parameters,
    operation,
    "post"
  )) as WorkflowOperation;
}
export async function workflowOperationRemove(
  workflowId: number,
  index?: number,
  parameters?: Record<string, any>
) {
  return await generalRequest(
    `api/workflow/${workflowId}/operations/${index === undefined ? "" : index}`,
    parameters,
    undefined,
    "delete"
  );
}
