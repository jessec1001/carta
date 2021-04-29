import { GeneralApi } from "lib/api";
import { MetaCollection } from "./types";

class MetaApi {
  @GeneralApi.route("GET", "api/meta")
  static async getEndpointsAsync() {
    return (await GeneralApi.requestGeneralAsync()) as MetaCollection[];
  }
}

export default MetaApi;
