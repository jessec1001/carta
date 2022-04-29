import queryString from "query-string";
import BaseAPI from "./BaseAPI";
import { Graph, GraphResource } from "./data";

/** Contains methods for accesssing the Carta Data API module. */
class DataAPI extends BaseAPI {
  private defaultResourceIdentifiers: GraphResource[];

  /**
   * @param resourceIdentifiers The default resource identifiers to incorporate into API requests.
   */
  constructor(resourceIdentifiers: GraphResource[] = []) {
    super();
    this.defaultResourceIdentifiers = resourceIdentifiers;
  }

  public getApiUrl() {
    return "/api/data";
  }
  protected getResourceUrl(source: string, resource?: string) {
    const encodedSource = encodeURIComponent(source);
    const encodedResource = encodeURIComponent(resource ?? "");
    return resource
      ? `${this.getApiUrl()}/${encodedSource}/${encodedResource}`
      : `${this.getApiUrl()}/${encodedSource}`;
  }

  /**
   * Finds any parameters that should be specified for a specific data source and resource.
   * @param source The data source.
   * @param resource The data resource.
   * @returns A mapping of parameters for the data source and resource.
   */
  protected getParameters(
    source?: string,
    resource?: string
  ): Map<string, any> {
    const parameters = new Map<string, any>();

    const filteredResourceIdentifiers: GraphResource[] =
      this.defaultResourceIdentifiers.filter(
        (data) =>
          (data.source === undefined ||
            source?.toLowerCase() === data.source.toLowerCase()) &&
          (data.resource === undefined ||
            resource?.toLowerCase() === data.resource.toLowerCase())
      );
    filteredResourceIdentifiers.forEach((resourceIdentifier) => {
      resourceIdentifier.parameters.forEach((value, key) =>
        parameters.set(key, value)
      );
    });

    return parameters;
  }

  /**
   * Retrieves all data sources that the current user has access to.
   * @returns A list of data sources.
   */
  public async getSources(): Promise<string[]> {
    const url = queryString.stringifyUrl({
      url: `${this.getApiUrl()}`,
      query: Object.fromEntries(this.getParameters()),
    });
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch data sources.",
      ["application/json"]
    );

    return await this.readJSON<string[]>(response);
  }
  /**
   * Retrieves all data resources that the current user has access to within a specific data source.
   * @param source The data source to retrieve resources from.
   * @returns A list of data resources.
   */
  public async getResources(source: string): Promise<string[]> {
    const url = queryString.stringifyUrl({
      url: `${this.getResourceUrl(source)}`,
      query: Object.fromEntries(this.getParameters(source)),
    });
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch data resources.",
      ["application/json"]
    );

    return await this.readJSON<string[]>(response);
  }

  /**
   * Creates a new graph at the specified data source and resource.
   * @param source The data source.
   * @param graph The graph to create.
   * @returns The created graph.
   */
  public async postGraph(source: string, graph: Graph): Promise<Graph> {
    const url = queryString.stringifyUrl({
      url: `${this.getResourceUrl(source)}`,
      query: Object.fromEntries(this.getParameters(source)),
    });
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("POST", graph)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to post data resource.",
      ["application/json"]
    );

    return await this.readJSON<Graph>(response);
  }
  /**
   * Deletes a graph at the specified data source and resource.
   * @param source The data source.
   * @param resource The data resource.
   */
  public async deleteGraph(source: string, resource: string): Promise<void> {
    const url = queryString.stringifyUrl({
      url: `${this.getResourceUrl(source, resource)}`,
      query: Object.fromEntries(this.getParameters(source, resource)),
    });
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("DELETE")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete data resource."
    );
  }
  /**
   * Retrieves a graph at a particular data source and resource.
   * @param source The data source.
   * @param resource The data resource.
   * @param selector A selector to use to isolate the data to be retrieved from the graph.
   * @param parameters A mapping of parameters to use for the selector.
   * @returns The graph.
   */
  public async getGraph(
    source: string,
    resource: string,
    selector: string,
    parameters?: Record<string, any>
  ): Promise<Graph> {
    const url = queryString.stringifyUrl({
      url: `${this.getResourceUrl(source, resource)}/${encodeURIComponent(
        selector
      )}`,
      query: {
        ...Object.fromEntries(this.getParameters(source, resource)),
        ...parameters,
      },
    });
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch data resource.",
      ["application/json"]
    );

    return await this.readJSON<Graph>(response);
  }

  /**
   * Gets the template for an operation that loads a particular data source.
   * Should be used to programmatically create new operations that load data.
   * @param source The data source.
   * @param resource The data resource.
   * @returns Information about the operation.
   */
  public async getOperation(
    source: string,
    resource: string
  ): Promise<{
    type: string;
    subtype: string | null;
    default: Record<string, any>;
  }> {
    const url = queryString.stringifyUrl({
      url: `${this.getResourceUrl(source, resource)}/operation`,
      query: Object.fromEntries(this.getParameters(source, resource)),
    });
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch data operation.",
      ["application/json"]
    );

    return await this.readJSON<{
      type: string;
      subtype: string | null;
      default: Record<string, any>;
    }>(response);
  }
}

export default DataAPI;
