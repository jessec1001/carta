import { MetaCollection } from "./meta";
import BaseAPI from "./BaseAPI";

/** Contains methods for accessing the Carta Meta API module. */
class MetaAPI extends BaseAPI {
  protected getApiUrl() {
    return "/api/meta";
  }

  /**
   * Retrieves all endpoints available to call.
   * @returns A list of endpoints.
   */
  public async getEndpointsAsync() {
    const url = this.getApiUrl();
    const response = await this.fetch(url, this.defaultFetchParameters("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch endpoint information.",
      ["application/json"]
    );

    const endpoints = await this.readJSON<MetaCollection[]>(response);
    return endpoints;
  }
}

export default MetaAPI;
