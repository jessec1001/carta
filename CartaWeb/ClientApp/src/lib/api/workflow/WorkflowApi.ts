import { generalRequest } from "lib/carta";
import { Workflow, WorkflowOperation } from "lib/types/workflow";

class WorkflowApi {
  // #region Workflow CRUD
  static async getWorfklowsAsync() {
    return (await generalRequest(
      `api/workflow`,
      {},
      undefined,
      "get"
    )) as Workflow[];
  }
  static async getWorkflowAsync(workflowId: string) {
    return (await generalRequest(
      `api/workflow/${workflowId}`,
      {},
      undefined,
      "get"
    )) as Workflow;
  }
  static async createWorkflowAsync(workflow: Workflow) {
    return (await generalRequest(
      `api/workflow`,
      {},
      workflow,
      "post"
    )) as Workflow;
  }
  static async deleteWorkflowAsync(workflowId: string) {
    return (await generalRequest(
      `api/workflow/${workflowId}`,
      {},
      undefined,
      "delete"
    )) as null;
  }
  static async updateWorkflowAsync(workflow: Workflow) {
    return (await generalRequest(
      `api/workflow/${workflow.id}`,
      {},
      undefined,
      "patch"
    )) as Workflow;
  }
  // #endregion

  // #region Workflow Operation CRUD
  static async getWorkflowOperationsAsync(workflowId: string) {
    return (await generalRequest(
      `api/workflow/${workflowId}/operations`,
      {},
      undefined,
      "get"
    )) as WorkflowOperation[];
  }
  static async getWorkflowOperationAsync(workflowId: string, index: number) {
    return (await generalRequest(
      `api/workflow/${workflowId}/operations/${index}`,
      {},
      undefined,
      "get"
    )) as WorkflowOperation;
  }
  static async insertWorkflowOperationAsync(
    operation: WorkflowOperation,
    workflowId: string,
    index?: number
  ) {
    return (await generalRequest(
      `api/workflow/${workflowId}/operations/${index ?? ""}`,
      {},
      operation,
      "post"
    )) as WorkflowOperation;
  }
  static async appendWorkflowOperationAsync(
    operation: WorkflowOperation,
    workflowId: string
  ) {
    return await this.insertWorkflowOperationAsync(operation, workflowId);
  }
  static async removeWorkflowOperationAsync(
    workflowId: string,
    index?: number
  ) {
    return (await generalRequest(
      `api/workflow/${workflowId}/operations/${index ?? ""}`,
      {},
      undefined,
      "delete"
    )) as null;
  }
  static async truncateWorkflowOperationAsync(workflowId: string) {
    return await this.removeWorkflowOperationAsync(workflowId);
  }
  static async updateWorkflowOperationAsync(
    operation: WorkflowOperation,
    workflowId: string,
    index: number
  ) {
    return await generalRequest(
      `api/workflow/${workflowId}/operations/${index}`,
      {},
      operation,
      "patch"
    );
  }
  // #endregion
}

export default WorkflowApi;
