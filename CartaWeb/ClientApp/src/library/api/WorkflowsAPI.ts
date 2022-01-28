import BaseAPI from "./BaseAPI";
import DataAPI from "./DataAPI";
import OperationsAPI from "./OperationsAPI";
import { Operation } from "./operations";
import { Workflow, WorkflowConnection, WorkflowTemplate } from "./workflows";

// TODO: Add documentation.
/** Contains methods for accessing the Carta Workflow API module. */
class WorkflowsAPI extends BaseAPI {
  private operationsApi: OperationsAPI;
  private dataApi: DataAPI;

  // TODO: Make it so every API has a reference to all the other APIs.
  /**
   * @param operationsApi A reference to the operations API.
   */
  public constructor(operationsApi: OperationsAPI, dataApi: DataAPI) {
    super();
    this.operationsApi = operationsApi;
    this.dataApi = dataApi;
  }

  protected getApiUrl() {
    return "/api/workflows";
  }
  protected getWorkflowUrl(workflowId: string) {
    return `${this.getApiUrl()}/${workflowId}`;
  }

  // #region Workflows CRUD
  public async getWorkflow(workflowId: string): Promise<Workflow> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(url, this.defaultFetchParameters("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workflow.",
      ["application/json"]
    );

    return await this.readJSON<Workflow>(response);
  }
  public async createWorkflow(workflow: Partial<Workflow>): Promise<Workflow> {
    const url = this.getApiUrl();
    const response = await fetch(
      url,
      this.defaultFetchParameters("POST", workflow)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to create workflow.",
      ["application/json"]
    );

    return await this.readJSON<Workflow>(response);
  }
  public async updateWorkflow(workflow: Workflow): Promise<Workflow> {
    const url = this.getWorkflowUrl(workflow.id);
    const response = await fetch(
      url,
      this.defaultFetchParameters("PATCH", workflow)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update workflow.",
      ["application/json"]
    );

    return await this.readJSON<Workflow>(response);
  }
  public async deleteWorkflow(workflowId: string): Promise<void> {
    const url = this.getWorkflowUrl(workflowId);
    const response = await fetch(url, this.defaultFetchParameters("DELETE"));

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
      this.defaultFetchParameters("GET")
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
      this.defaultFetchParameters("POST")
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
      this.defaultFetchParameters("DELETE")
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
      this.defaultFetchParameters("GET")
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
      this.defaultFetchParameters("POST", connection)
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
      this.defaultFetchParameters("PATCH", connection)
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
      this.defaultFetchParameters("DELETE")
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
      this.defaultFetchParameters("POST", connection)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to suggest workflow connection."
    );

    return await this.readJSON<WorkflowConnection>(response);
  }
  // #endregion

  // #region Template Workflows
  // TODO: Consider moving template creating endpoints to the backend.
  public async createWorkflowFromTemplate(
    workflow: Partial<Workflow>,
    template: WorkflowTemplate
  ): Promise<Workflow> {
    // Create the workflow.
    const createdWorkflow = await this.createWorkflow(workflow);

    // Create a mapping of the template operations identifiers to the created operations identifiers.
    const operationIdMapping = new Map<string, string>();

    // Create the operations.
    for (let k = 0; k < template.operations.length; k++) {
      const operationTemplate = template.operations[k];
      const operation = await this.operationsApi.createOperation(
        operationTemplate.type,
        operationTemplate.subtype,
        operationTemplate.default
      );
      operationIdMapping.set(operationTemplate.id, operation.id);
      await this.addWorkflowOperation(createdWorkflow.id, operation.id);
    }

    // Create the connections.
    // We need to make sure to use the mapping from template to created operation identifiers.
    for (let k = 0; k < template.connections.length; k++) {
      const connectionTemplate = template.connections[k];
      await this.addWorkflowConnection(createdWorkflow.id, {
        source: {
          operation: operationIdMapping.get(
            connectionTemplate.source.operation
          )!,
          field: connectionTemplate.source.field,
        },
        target: {
          operation: operationIdMapping.get(
            connectionTemplate.target.operation
          )!,
          field: connectionTemplate.target.field,
        },
      });
    }

    // Retrieve the updated workflow.
    return await this.getWorkflow(createdWorkflow.id);
  }
  /**
   * Creates a template for a blank workflow.
   * @returns A workflow template.
   */
  public async createBlankWorkflowTemplate(): Promise<WorkflowTemplate> {
    // A blank workflow has no operations nor connections.
    return {
      operations: [],
      connections: [],
    };
  }
  /**
   * Creates a template for a workflow that loads a collection of data as graphs, combines them, and visualizes them
   * as an output.
   * @param data The datasets to load.
   * @returns A workflow template.
   */
  public async createDataWorkflowTemplate(
    data: { source: string; resource: string }[]
  ): Promise<WorkflowTemplate> {
    // Initialize lists of operations and connections.
    const operations: WorkflowTemplate["operations"] = [];
    const connections: WorkflowTemplate["connections"] = [];

    // Add the output operation.
    const outputOperation: Operation = {
      id: operations.length.toString(),
      type: "workflowOutput",
      subtype: null,
      default: { Name: "Visualization" },
    };
    operations.push(outputOperation);

    // Add the visualization operation.
    const visOperation: Operation = {
      id: operations.length.toString(),
      type: "visualizeGraphPlot",
      subtype: null,
    };
    operations.push(visOperation);
    connections.push({
      id: "",
      source: { field: "Plot", operation: visOperation.id },
      target: { field: "Value", operation: outputOperation.id },
      multiplex: false,
    });

    // TODO: Add a combine graph operation.

    // Add each of the data operations.
    for (let k = 0; k < data.length; k++) {
      const datum = data[k];
      const datumTemplate = await this.dataApi.getOperation(
        datum.source,
        datum.resource
      );
      const datumOperation: Operation = {
        id: operations.length.toString(),
        type: datumTemplate.type,
        subtype: datumTemplate.subtype,
        default: datumTemplate.default,
      };
      operations.push(datumOperation);
      connections.push({
        id: "",
        source: { field: "Graph", operation: datumOperation.id },
        target: { field: "Graph", operation: visOperation.id },
        multiplex: false,
      });
    }

    // Return the workflow template.
    return {
      operations,
      connections,
    };
  }
  // #endregion
}

export default WorkflowsAPI;
