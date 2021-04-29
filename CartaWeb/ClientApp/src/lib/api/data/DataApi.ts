import { Graph } from "lib/types/graph";
import Selector from "lib/types/selectors";
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
  static async getGraphPropertiesAsync({
    source,
    resource,
    parameters,
  }: {
    source: string;
    resource: string;
    parameters?: Record<string, any>;
  }) {}
  @GeneralApi.route("GET", "api/data/{source}/{resource}/{selector}")
  static async getGraphSelectionAsync({
    source,
    resource,
    selector,
    parameters,
  }: {
    source: string;
    resource: string;
    selector: Selector;
    parameters?: Record<string, any>;
  }) {
    const { type, ...props } = selector;
    return (await GeneralApi.requestGeneralAsync({
      ...this.retrieveAuxiliaryParameters(source, resource),
      source,
      resource,
      ...parameters,
      selector: type,
      ...props,
    })) as Graph;
  }
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

  static getGraphRootsAsync({
    source,
    resource,
    parameters,
  }: {
    source: string;
    resource: string;
    parameters?: Record<string, any>;
  }) {
    return this.getGraphSelectionAsync({
      source,
      resource,
      selector: {
        type: "roots",
      },
      parameters,
    });
  }
  static async getGraphVertexAsync({
    source,
    resource,
    id,
    parameters,
  }: {
    source: string;
    resource: string;
    id: string;
    parameters?: Record<string, any>;
  }) {
    return this.getGraphSelectionAsync({
      source,
      resource,
      selector: {
        type: "include",
        ids: [id],
      },
      parameters,
    });
  }
  static async getGraphChildrenAsync({
    source,
    resource,
    id,
    parameters,
  }: {
    source: string;
    resource: string;
    id: string;
    parameters?: Record<string, any>;
  }) {
    return this.getGraphSelectionAsync({
      source,
      resource,
      selector: {
        type: "children",
        ids: [id],
      },
      parameters,
    });
  }
}

export default DataApi;
