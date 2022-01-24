import queryString from "query-string";
import { flattenSchema, JsonSchema } from "library/schema";
import BaseAPI from "./BaseAPI";
import { Job, Operation, OperationSchema, OperationType } from "./operations";

class OperationsAPI extends BaseAPI {
  protected getApiUrl() {
    return "/api/operations";
  }
  protected getOperationUrl(operationId: string) {
    return `${this.getApiUrl()}/${operationId}`;
  }

  // TODO: Temporary and should be generalized by asking the server for the appropriate authentication to send.
  private static retrieveAuthentication(): Record<string, string> {
    // These designate the known authenatication pairings.
    const knownEntries: Record<string, string> = {
      hyperthought: "hyperthoughtKey",
    };

    // Check the local storage for each entry and add them to the authentication record.
    const authentication: Record<string, string> = {};
    for (const [auth, key] of Object.entries(knownEntries)) {
      const entry = localStorage.getItem(key);
      if (entry) authentication[auth] = JSON.parse(entry);
    }
    return authentication;
  }

  public async getOperationTypes(
    filterName?: string,
    filterTags?: string[]
  ): Promise<OperationType[]> {
    const url = queryString.stringifyUrl({
      url: `${this.getApiUrl()}/types`,
      query: {
        name: filterName,
        tags: filterTags,
      },
    });
    const response = await fetch(url, this.defaultFetcher("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation types."
    );

    return await this.readJSON<OperationType[]>(response);
  }

  // #region Operations CRUD
  public async getOperation(
    operationId: string,
    includeSchema: boolean = true
  ): Promise<Operation> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(url, this.defaultFetcher("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation."
    );

    const operation = await this.readJSON<Operation>(response);

    if (includeSchema)
      operation.schema = await this.getOperationSchema(operationId);

    return operation;
  }
  public async createOperation(
    operationType: string,
    operationSubtype: string | null,
    defaults?: Record<string, any>
  ): Promise<Operation> {
    const operation = {
      type: operationType,
      subtype: operationSubtype,
      default: defaults,
    };

    const url = this.getApiUrl();
    const response = await fetch(url, this.defaultFetcher("POST", operation));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to create operation."
    );

    return await this.readJSON<Operation>(response);
  }
  public async updateOperation(
    operation: Partial<Operation>
  ): Promise<Operation> {
    const url = this.getOperationUrl(operation.id!);
    const response = await fetch(url, this.defaultFetcher("PATCH", operation));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update operation."
    );

    return await this.readJSON<Operation>(response);
  }
  public async deleteOperation(operationId: string): Promise<void> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete operation."
    );
  }
  // #endregion

  // #region Operations Execution
  public async executeOperation(
    operationId: string,
    inputs: Record<string, any>
  ): Promise<Job> {
    const authentication = OperationsAPI.retrieveAuthentication();

    const url = this.getOperationUrl(operationId);
    const response = await fetch(
      `${url}/jobs`,
      this.defaultFetcher("POST", {
        ...inputs,
        authentication: authentication,
      })
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to execute operation."
    );

    return await this.readJSON<Job>(response);
  }
  public async getOperationJobs(operationId: string): Promise<Job[]> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(`${url}/jobs`, this.defaultFetcher("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation jobs."
    );

    return await this.readJSON<Job[]>(response);
  }
  public async getOperationJob(
    operationId: string,
    jobId: string
  ): Promise<Job> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(
      `${url}/jobs/${jobId}`,
      this.defaultFetcher("GET")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation job."
    );

    return await this.readJSON<Job>(response);
  }
  public async getOperationJobWhenComplete(
    operationId: string,
    jobId: string,
    interval: number = 1000
  ): Promise<Job> {
    let job: Job;
    do {
      job = await this.getOperationJob(operationId, jobId);
      await new Promise((resolve) => setTimeout(resolve, interval));
    } while (!job.completed);
    return job;
  }

  public async uploadFile(
    operationId: string,
    jobId: string,
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<void> {
    // TODO: Use axios to upload file.
  }
  // #endregion

  // #region Operations Schema
  public async getOperationSchema(
    operationId: string
  ): Promise<OperationSchema> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(`${url}/schema`, this.defaultFetcher("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation input schema."
    );

    const schema = await this.readJSON<OperationSchema>(response);
    Object.keys(schema.inputs).forEach((key) => {
      schema.inputs[key] = flattenSchema(schema.inputs[key]);
    });
    Object.keys(schema.outputs).forEach((key) => {
      schema.outputs[key] = flattenSchema(schema.outputs[key]);
    });
    return schema;
  }
  // #endregion
}

export default OperationsAPI;
