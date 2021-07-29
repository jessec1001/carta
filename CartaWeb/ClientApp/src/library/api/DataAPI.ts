import queryString from "query-string";
import BaseAPI from "./BaseAPI";
import { DataResourceIdentifier } from "./data";

/** Contains methods for accesssing the Carta Data API module. */
class DataAPI extends BaseAPI {
  private defaultResourceIdentifiers: DataResourceIdentifier[];

  /**
   * @param resourceIdentifiers The default resource identifiers to incorporate into API requests.
   */
  constructor(resourceIdentifiers: DataResourceIdentifier[] = []) {
    super();

    this.defaultResourceIdentifiers = resourceIdentifiers;
  }

  protected getApiUrl() {
    return "/api/data";
  }
  protected getResourceUrl(source: string, resource: string) {
    const encodedSource = encodeURIComponent(source);
    const encodedResource = encodeURIComponent(resource);
    return `${this.getApiUrl()}/${encodedSource}/${encodedResource}`;
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

    const filteredResourceIdentifiers: DataResourceIdentifier[] =
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
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to fetch data sources."
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
      url: `${this.getApiUrl()}/${encodeURIComponent(source)}`,
      query: Object.fromEntries(this.getParameters(source)),
    });
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to fetch data resources."
    );

    return await this.readJSON<string[]>(response);
  }
}

export default DataAPI;
