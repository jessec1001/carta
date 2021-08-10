import BaseAPI from "./BaseAPI";
import {
  parseWorkflow,
  parseWorkflowVersion,
  Workflow,
  WorkflowDTO,
  WorkflowOperation,
  WorkflowVersion,
  WorkflowVersionDTO,
} from "./workflow";

/** Contains methods for accessing the Carta Workflow API module. */
class WorkflowAPI extends BaseAPI {
  protected getApiUrl() {
    return "/api/workflow";
  }
  protected getWorkflowUrl(workflowId: string) {
    return `${this.getApiUrl()}/${workflowId}`;
  }

  // #region Workflow Base
  /**
   * Retrieves all workflows that the current user has access to.
   * @returns A list of workflows.
   */
  public async getWorkflows(): Promise<Workflow[]> {
    const url = this.getApiUrl();
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflows."
    );

    return (await this.readJSON<WorkflowDTO[]>(response)).map(parseWorkflow);
  }
  /**
   * Retrieves a specific workflow that the current user has access to with an optionally specified version.
   * @param workflowId The unique identifier of the workflow.
   * @param versionNumber The optional version to retrieve. If not specified, defaults to the latest or temporary
   * version. If specified, will retrieve the globally accessible version and not the temporary version. This can be
   * used to determine whether the workflow is currently in a temporary state or not.
   * @returns The specified workflow.
   */
  public async getWorkflow(
    workflowId: string,
    versionNumber?: number
  ): Promise<Workflow> {
    let url = this.getWorkflowUrl(workflowId);
    if (versionNumber !== undefined)
      url = `${url}?workflowVersion=${versionNumber}`;
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflow."
    );

    return parseWorkflow(await this.readJSON<WorkflowDTO>(response));
  }
  /**
   * Creates a new workflow with the specified name and operations if specified.
   * @param workflow The workflow to create.
   * @returns The newly created workflow.
   */
  public async createWorkflow(
    workflow: Omit<Workflow, "id" | "versionNumber">
  ): Promise<Workflow> {
    const url = this.getApiUrl();
    const response = await fetch(url, this.defaultFetcher("POST", workflow));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to create workflow."
    );

    return parseWorkflow(await this.readJSON<WorkflowDTO>(response));
  }
  /**
   * Deletes an existing workflow.
   * @param workflowId The unique identifier of the workflow.
   */
  public async deleteWorkflow(workflowId: string): Promise<void> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete workflow."
    );
  }
  // #endregion

  // #region Workflow Operations
  /**
   * Retrieves all of the operations that make up a particular workflow.
   * @param workflowId The unique identifier of the workflow.
   * @param versionNumber The optional version to retrieve. If not specified, defaults to the latest or temporary
   * version. If specified, will retrieve the globally accessible version and not the temporary version. This can be
   * used to determine whether the workflow is currently in a temporary state or not.
   * @returns A list of ordered workflow operations.
   */
  public async getWorkflowOperations(
    workflowId: string,
    versionNumber?: number
  ): Promise<WorkflowOperation[]> {
    let url = `${this.getWorkflowUrl(workflowId)}/operations`;
    if (versionNumber !== undefined)
      url = `${url}?workflowVersion=${versionNumber}`;
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to get workflow operations."
    );

    return await this.readJSON<WorkflowOperation[]>(response);
  }
  /**
   * Retrieves an operation at a particular index that makes up a workflow.
   * @param workflowId The unique identifier of the workflow.
   * @param operationIndex The index of the operation.
   * @param versionNumber The optional version to retrieve. If not specified, defaults to the latest or temporary
   * version. If specified, will retrieve the globally accessible version and not the temporary version. This can be
   * used to determine whether the workflow is currently in a temporary state or not.
   * @returns The specified workflow operation.
   */
  public async getWorkflowOperation(
    workflowId: string,
    operationIndex: number,
    versionNumber?: number
  ): Promise<WorkflowOperation> {
    let url = `${this.getWorkflowUrl(workflowId)}/operations/${operationIndex}`;
    if (versionNumber !== undefined)
      url = `${url}?workflowVersion=${versionNumber}`;
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to get workflow operation."
    );

    return await this.readJSON<WorkflowOperation>(response);
  }
  /**
   * Inserts a new operation at a particular index into a workflow.
   * @param workflowId The unique identifier of the workflow.
   * @param operation The operation to insert.
   * @param index The index to insert the operation at. If not specified, will insert the operation at the end of the
   * list of workflow operations.
   * @returns The newly inserted operation.
   */
  public async insertWorkflowOperation(
    workflowId: string,
    operation: WorkflowOperation,
    index?: number
  ): Promise<WorkflowOperation> {
    let url = `${this.getWorkflowUrl(workflowId)}/operations`;
    if (index !== undefined) url = `${url}/${index}`;
    const response = await fetch(url, this.defaultFetcher("POST", operation));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to insert workflow operation."
    );

    return await this.readJSON<WorkflowOperation>(response);
  }
  /**
   * Removes an existing operation at a particular index from a workflow.
   * @param workflowId The unique identifier of the workflow.
   * @param index The index to remove the operation from. If not specified, will remove the operation at the back end of
   * the list of workflow operations.
   */
  public async removeWorkflowOperation(
    workflowId: string,
    index?: number
  ): Promise<void> {
    let url = `${this.getWorkflowUrl(workflowId)}/operations`;
    if (index !== undefined) url = `${url}/${index}`;
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete workflow operation."
    );
  }
  /**
   * Updates an existing operation at a particular index of a workflow.
   * @param workflowId The unique identifier of the workflow.
   * @param operation The operation to use to update an existing operation.
   * @param index The index to update the operation at.
   * @returns The updated operation.
   */
  public async updateWorkflowOperation(
    workflowId: string,
    operation: WorkflowOperation,
    index: number
  ): Promise<WorkflowOperation> {
    const url = `${this.getWorkflowUrl(workflowId)}/operations/${index}`;
    const response = await fetch(url, this.defaultFetcher("PATCH", operation));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update workflow operation."
    );

    return await this.readJSON<WorkflowOperation>(response);
  }
  // #endregion

  // #region Workflow Versions
  /**
   * Retrieves all of the versions associated with a particular workflow.
   * @param workflowId The unique identifier of the workflow.
   * @returns A list of workflow versions.
   */
  public async getWorkflowVersions(
    workflowId: string
  ): Promise<WorkflowVersion[]> {
    const url = `${this.getWorkflowUrl(workflowId)}/versions`;
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflow versions."
    );

    return (await this.readJSON<WorkflowVersionDTO[]>(response)).map(
      parseWorkflowVersion
    );
  }
  /**
   * Commits the temporary version of a workflow to a new global version of the workflow.
   *
   * If called when a temporary workflow is not currently available, returns a 404 error. This implies that a workflow
   * should be checked for temporariness before calling this method to deduce unambigious returns.
   * @param workflowId The unique identifier of the workflow.
   * @param versionDescription The description of the new version.
   * @returns The new workflow version.
   */
  public async commitWorkflowVersion(
    workflowId: string,
    versionDescription: string
  ): Promise<WorkflowVersion> {
    const url = `${this.getWorkflowUrl(workflowId)}/versions`;
    const response = await fetch(
      url,
      this.defaultFetcher("POST", versionDescription)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to commit workflow version."
    );

    return parseWorkflowVersion(
      await this.readJSON<WorkflowVersionDTO>(response)
    );
  }
  /**
   * Reverts a global workflow to a previous version.
   * @param workflowId The unique identifier of the workflow.
   * @param versionNumber The version number to revert to.
   * @returns The reverted workflow version.
   */
  public async revertWorkflowVersion(
    workflowId: string,
    versionNumber: number
  ): Promise<WorkflowVersion> {
    const url = `${this.getWorkflowUrl(
      workflowId
    )}/versions?workflowVersion=${versionNumber}`;
    const response = await fetch(url, this.defaultFetcher("PATCH"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to revert workflow version."
    );

    return parseWorkflowVersion(
      await this.readJSON<WorkflowVersionDTO>(response)
    );
  }
  // #endregion
}

export default WorkflowAPI;
