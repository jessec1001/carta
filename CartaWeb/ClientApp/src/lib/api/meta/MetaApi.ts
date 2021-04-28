import queryString from "query-string";
import { ApiException } from "lib/exceptions";
import { MetaCollection } from "./types";

class MetaApi {
  static async getEndpointsAsync() {
    const url: string = queryString.stringifyUrl({
      url: `api/meta`,
    });

    const response = await fetch(url);
    if (response.ok) {
      return (await response.json()) as MetaCollection[];
    } else {
      throw new ApiException(response, "Failed to get metadata.");
    }
  }
}

export default MetaApi;
