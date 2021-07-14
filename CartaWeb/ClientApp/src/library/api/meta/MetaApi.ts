import { GeneralApi } from "library/api";
import { JsonSchema } from "library/schema";
import { MetaCollection, MetaTypeEntry } from "./types";

class MetaApi {
  @GeneralApi.route("GET", "api/meta")
  static async getEndpointsAsync() {
    return (await GeneralApi.requestGeneralAsync()) as MetaCollection[];
  }

  @GeneralApi.route("GET", "api/meta/actors")
  static async getActorsAsync() {
    return (await GeneralApi.requestGeneralAsync()) as Record<
      string,
      MetaTypeEntry
    >;
  }
  @GeneralApi.route("GET", "api/meta/actors/{actor}")
  static async getActorDefaultAsync(params: { actor: string }) {
    return (await GeneralApi.requestGeneralAsync(params)) as any;
  }
  @GeneralApi.route("GET", "api/meta/actors/{actor}/schema")
  static async getActorSchemaAsync(params: { actor: string }) {
    return (await GeneralApi.requestGeneralAsync(params)) as JsonSchema;
  }

  @GeneralApi.route("GET", "api/meta/selectors")
  static async getSelectorsAsync() {
    return (await GeneralApi.requestGeneralAsync()) as Record<
      string,
      MetaTypeEntry
    >;
  }
  @GeneralApi.route("GET", "api/meta/selectors/{selector}")
  static async getSelectorDefaultAsync(params: { selector: string }) {
    return (await GeneralApi.requestGeneralAsync(params)) as any;
  }
  @GeneralApi.route("GET", "api/meta/selectors/{selector}/schema")
  static async getSelectorSchemaAsync(params: { selector: string }) {
    return (await GeneralApi.requestGeneralAsync(params)) as JsonSchema;
  }
}

export default MetaApi;
