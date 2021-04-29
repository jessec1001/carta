import { Graph } from "lib/types/graph";
import GeneralApi from "../general";

class DataApi {
  private static retrieveAuxiliaryParameters(
    source?: string,
    resource?: string
  ) {
    const requiredEntries: string[] = [];
    if (source?.toLowerCase() === "hyperthought")
      requiredEntries.push("hyperthoughtKey");

    const parameters: Record<string, any> = {};
    requiredEntries.forEach(
      (entry) => (parameters[entry] = localStorage.getItem(entry))
    );
    return parameters;
  }

  @GeneralApi.route("GET", "api/data")
  static async getSourcesAsync() {
    return (await GeneralApi.requestGeneralAsync({
      ...this.retrieveAuxiliaryParameters(),
    })) as string[];
  }
  @GeneralApi.route("GET", "api/data/{source}")
  static async getResourcesAsync({ source }: { source: string }) {
    return (await GeneralApi.requestGeneralAsync({
      ...this.retrieveAuxiliaryParameters(source),
      source,
    })) as string[];
  }

  @GeneralApi.route("GET", "api/data/{source}/{resource}")
  static async getGraphPropertiesAsync() {}
  @GeneralApi.route("GET", "api/data/{source}/{resource}/{selector}")
  static async getGraphSelectionAsync() {}
  @GeneralApi.route("POST", "api/data/user")
  static async createGraphAsync({ graph }: { graph: Graph }) {
    return (await GeneralApi.requestGeneralAsync(
      {
        ...this.retrieveAuxiliaryParameters("user"),
        source: "user",
      },
      { body: JSON.stringify(graph) }
    )) as Graph;
  }
  @GeneralApi.route("DELETE", "api/data/user/{resource}")
  static async deleteGraphAsync({ resource }: { resource: string }) {
    return (await GeneralApi.requestGeneralAsync({
      ...this.retrieveAuxiliaryParameters("user", resource),
      source: "user",
      resource: resource,
    })) as null;
  }

  static async getGraphRootsAsync() {}
  static async getGraphVertexAsync() {}
  static async getGraphChildrenAsync() {}
}

export default DataApi;
