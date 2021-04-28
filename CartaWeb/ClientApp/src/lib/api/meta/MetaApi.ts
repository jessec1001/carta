import queryString from "query-string";
import { ApiException } from "lib/exceptions";
import { ApiCollection } from "../../types/meta";

class MetaApi {
  static async getEndpointsAsync() {
    const url: string = queryString.stringifyUrl({
      url: `api/meta`,
    });

    const response = await fetch(url);
    if (response.ok) {
      return (await response.json()) as ApiCollection[];
    } else {
      throw new ApiException(response, "Failed to get metadata.");
    }
  }
}

export default MetaApi;
