import axios from "axios";
import fileDownload from "js-file-download";
import queryString from "query-string";
import { flattenSchema } from "library/schema";
import { Job, Operation, OperationSchema, OperationType } from "./operations";
import BaseAPI from "./BaseAPI";

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
    filterTags?: string[],
    workspaceId?: string
  ): Promise<OperationType[]> {
    const url = queryString.stringifyUrl({
      url: `${this.getApiUrl()}/types`,
      query: {
        name: filterName,
        tags: filterTags,
        workspace: workspaceId,
      },
    });
    const response = await fetch(url, this.defaultFetchParameters("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation types.",
      ["application/json"]
    );

    return await this.readJSON<OperationType[]>(response);
  }

  // #region Operations CRUD
  public async getOperation(
    operationId: string,
    includeSchema: boolean = true
  ): Promise<Operation> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(url, this.defaultFetchParameters("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation.",
      ["application/json"]
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
    const response = await fetch(
      url,
      this.defaultFetchParameters("POST", operation)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to create operation.",
      ["application/json"]
    );

    return await this.readJSON<Operation>(response);
  }
  public async updateOperation(
    operation: Partial<Operation>
  ): Promise<Operation> {
    const url = this.getOperationUrl(operation.id!);
    const response = await fetch(
      url,
      this.defaultFetchParameters("PATCH", operation)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update operation.",
      ["application/json"]
    );

    return await this.readJSON<Operation>(response);
  }
  public async deleteOperation(operationId: string): Promise<void> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(url, this.defaultFetchParameters("DELETE"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete operation.",
      ["application/json"]
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
      this.defaultFetchParameters("POST", {
        ...inputs,
        authentication: authentication,
      })
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to execute operation.",
      ["application/json"]
    );

    return await this.readJSON<Job>(response);
  }
  public async getOperationJobs(operationId: string): Promise<Job[]> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(
      `${url}/jobs`,
      this.defaultFetchParameters("GET")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation jobs.",
      ["application/json"]
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
      this.defaultFetchParameters("GET")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation job.",
      ["application/json"]
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
    await axios({
      url: `${this.getOperationUrl(operationId)}/jobs/${jobId}/upload`,
      method: "POST",
      data: file,
      onUploadProgress: (progressEvent) => {
        onProgress?.(progressEvent.loaded / progressEvent.total);
      },
    });
  }
  public async downloadJobFile(
    operationId: string,
    jobId: string,
    field: string
  ): Promise<void> {
    // Use axios to get the file.
    const response = await axios({
      url: `${this.getOperationUrl(
        operationId
      )}/jobs/${jobId}/${field}/download`,
      method: "GET",
      responseType: "blob",
    });
    fileDownload(response.data, field);
  }
  // #endregion

  // #region Operations Schema
  public async getOperationSchema(
    operationId: string
  ): Promise<OperationSchema> {
    const url = this.getOperationUrl(operationId);
    const response = await fetch(
      `${url}/schema`,
      this.defaultFetchParameters("GET")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch operation input schema.",
      ["application/json"]
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
