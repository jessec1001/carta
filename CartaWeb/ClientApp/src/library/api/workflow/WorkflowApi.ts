import { GeneralApi } from "lib/api";
import { Workflow, WorkflowOperation } from "lib/types/workflow";

class WorkflowApi {
  // #region Workflow CRUD
  @GeneralApi.route("GET", "api/workflow")
  static async getWorfklowsAsync() {
    return (await GeneralApi.requestGeneralAsync()) as Workflow[];
  }
  @GeneralApi.route("GET", "api/workflow/{workflowId}")
  static async getWorkflowAsync({ workflowId }: { workflowId: string }) {
    return (await GeneralApi.requestGeneralAsync({ workflowId })) as Workflow;
  }
  @GeneralApi.route("POST", "api/workflow")
  static async createWorkflowAsync({ workflow }: { workflow: Workflow }) {
    return (await GeneralApi.requestGeneralAsync(
      {},
      { body: JSON.stringify(workflow) }
    )) as Workflow;
  }
  @GeneralApi.route("DELETE", "api/workflow/{workflowId}")
  static async deleteWorkflowAsync({ workflowId }: { workflowId: string }) {
    return (await GeneralApi.requestGeneralAsync({ workflowId })) as null;
  }
  @GeneralApi.route("PATCH", "api/workflow/{workflowId}")
  static async updateWorkflowAsync({ workflow }: { workflow: Workflow }) {
    return (await GeneralApi.requestGeneralAsync(
      { workflowId: workflow.id },
      { body: JSON.stringify(workflow) }
    )) as Workflow;
  }
  // #endregion

  // #region Workflow Operation CRUD
  @GeneralApi.route("GET", "api/workflow/{workflowId}/operations")
  static async getWorkflowOperationsAsync({
    workflowId,
  }: {
    workflowId: string;
  }) {
    return (await GeneralApi.requestGeneralAsync({
      workflowId,
    })) as WorkflowOperation[];
  }
  @GeneralApi.route("GET", "api/workflow/{workflowId}/operations/{index}")
  static async getWorkflowOperationAsync({
    workflowId,
    index,
  }: {
    workflowId: string;
    index: number;
  }) {
    return (await GeneralApi.requestGeneralAsync({
      workflowId,
      index,
    })) as WorkflowOperation;
  }
  @GeneralApi.route("POST", "api/workflow/{workflowId}/operations/{index?}")
  static async insertWorkflowOperationAsync({
    operation,
    workflowId,
    index,
  }: {
    operation: WorkflowOperation;
    workflowId: string;
    index?: number;
  }) {
    return (await GeneralApi.requestGeneralAsync(
      { workflowId, index: index ?? "" },
      { body: JSON.stringify(operation) }
    )) as WorkflowOperation;
  }
  @GeneralApi.route("DELETE", "api/workflow/{workflowId}/operations/{index?}")
  static async removeWorkflowOperationAsync({
    workflowId,
    index,
  }: {
    workflowId: string;
    index?: number;
  }) {
    return (await GeneralApi.requestGeneralAsync({
      workflowId,
      index: index ?? "",
    })) as null;
  }
  @GeneralApi.route("PATCH", "api/workflow/{workflowId}/operations{index}")
  static async updateWorkflowOperationAsync({
    operation,
    workflowId,
    index,
  }: {
    operation: WorkflowOperation;
    workflowId: string;
    index: number;
  }) {
    return (await GeneralApi.requestGeneralAsync(
      {
        workflowId,
        index,
      },
      { body: JSON.stringify(operation) }
    )) as WorkflowOperation;
  }
  // #endregion
}

export default WorkflowApi;
