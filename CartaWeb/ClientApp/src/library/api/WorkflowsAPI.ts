import BaseAPI from "./BaseAPI";
import { Operation } from "./operations";
import { Workflow, WorkflowConnection } from "./workflows";

// TODO: Add documentation.
/** Contains methods for accessing the Carta Workflow API module. */
class WorkflowsAPI extends BaseAPI {
  protected getApiUrl() {
    return "/api/workflows";
  }
  protected getWorkflowUrl(workflowId: string) {
    return `${this.getApiUrl()}/${workflowId}`;
  }

  // #region Workflows CRUD
  public async getWorkflow(workflowId: string): Promise<Workflow> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(url, this.defaultFetcher("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflow."
    );

    return await this.readJSON<Workflow>(response);
  }
  public async createWorkflow(workflow: Partial<Workflow>): Promise<Workflow> {
    const url = this.getApiUrl();
    const response = await fetch(url, this.defaultFetcher("POST", workflow));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to create workflow."
    );

    return await this.readJSON<Workflow>(response);
  }
  public async updateWorkflow(workflow: Workflow): Promise<Workflow> {
    const url = this.getWorkflowUrl(workflow.id);
    const response = await fetch(url, this.defaultFetcher("PATCH", workflow));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update workflow."
    );

    return await this.readJSON<Workflow>(response);
  }
  public async deleteWorkflow(workflowId: string): Promise<void> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete workflow."
    );
  }
  // #endregion

  // #region Workflows operations
  public async getWorkflowOperations(workflowId: string): Promise<Operation[]> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/operations`,
      this.defaultFetcher("GET")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflow operations."
    );

    return await this.readJSON<Operation[]>(response);
  }
  public async addWorkflowOperation(
    workflowId: string,
    operationId: string
  ): Promise<void> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/operations/${operationId}`,
      this.defaultFetcher("POST")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to add workflow operation."
    );
  }
  public async removeWorkflowOperation(
    workflowId: string,
    operationId: string
  ): Promise<void> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/operations/${operationId}`,
      this.defaultFetcher("DELETE")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to remove workflow operation."
    );
  }
  // #endregion

  // #region Workflows connections
  public async getWorkflowConnections(
    workflowId: string
  ): Promise<WorkflowConnection[]> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/connections`,
      this.defaultFetcher("GET")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflow connections."
    );

    return await this.readJSON<WorkflowConnection[]>(response);
  }
  public async addWorkflowConnection(
    workflowId: string,
    connection: Partial<WorkflowConnection>
  ): Promise<WorkflowConnection> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/connections`,
      this.defaultFetcher("POST", connection)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to add workflow connection."
    );

    return await this.readJSON<WorkflowConnection>(response);
  }
  public async updateWorkflowConnection(
    workflowId: string,
    connection: WorkflowConnection
  ): Promise<WorkflowConnection> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/connections/${connection.id}`,
      this.defaultFetcher("PATCH", connection)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update workflow connection."
    );

    return await this.readJSON<WorkflowConnection>(response);
  }
  public async removeWorkflowConnection(
    workflowId: string,
    connectionId: string
  ): Promise<void> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/connections/${connectionId}`,
      this.defaultFetcher("DELETE")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to remove workflow connection."
    );
  }

  public async suggestWorkflowConnection(
    workflowId: string,
    connection: Partial<WorkflowConnection>
  ): Promise<WorkflowConnection> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(
      `${url}/connections/suggest`,
      this.defaultFetcher("POST", connection)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to suggest workflow connection."
    );

    return await this.readJSON<WorkflowConnection>(response);
  }
  // #endregion

  // #region Template Workflows
  public async createBlankWorkflow(
    workflow: Partial<Workflow>
  ): Promise<Workflow> {
    // Create the workflow.
    const createdWorkflow = await this.createWorkflow(workflow);

    // Return the workflow.
    return createdWorkflow;
  }
  public async createDataWorkflow(
    data: { source: string; resource: string }[],
    workflow: Partial<Workflow>
  ): Promise<Workflow> {
    // Create the workflow.
    const createdWorkflow = await this.createWorkflow(workflow);

    // Return the workflow.
    return createdWorkflow;
  }
  // #endregion
}

export default WorkflowsAPI;
