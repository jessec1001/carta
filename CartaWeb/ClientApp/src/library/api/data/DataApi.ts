import Logging, { LogSeverity, LogWidget } from "library/logging";
import { HyperthoughtAuthenticationWidget } from "library/logging/widgets";
import { Graph } from "library/api/data";
import { Selector } from "library/api/workflow";
import GeneralApi from "library/api/general";
import { ApiException } from "library/exceptions";

class DataApi {
  private static retrieveAuxiliaryParameters(
    source?: string,
    resource?: string
  ) {
    const requiredEntries: Record<string, string> = {};
    if (source?.toLowerCase() === "hyperthought")
      requiredEntries["api"] = "hyperthoughtKey";
    const parameters: Record<string, any> = {};
    Object.entries(requiredEntries).forEach(([paramKey, storageKey]) => {
      const paramValue = localStorage.getItem(storageKey);
      if (paramValue !== null) parameters[paramKey] = paramValue;
    });

    if (Object.keys(parameters).length > 0)
      Logging.log({
        severity: LogSeverity.Debug,
        source: "Data API",
        title: "Auxiliary Parameters",
        message: "Obtained stored parameters.",
        data: parameters,
      });

    return parameters;
  }

  private static async requestGeneralAsync(
    apiParameters?: Record<string, any>,
    fetchParameters?: RequestInit,
    url?: string
  ) {
    try {
      // Try to make the standard request.
      const data = await GeneralApi.requestGeneralAsync({
        apiParameters,
        fetchParameters,
        url,
      });
      return data;
    } catch (err) {
      // If there was an API error, check to see if it can be acted on.
      if (err instanceof ApiException) {
        console.log(err);
        const { source } = apiParameters as {
          source?: string;
          resource?: string;
        };
        if (source?.toLowerCase() === "hyperthought") {
          if (err.status === 401 || err.status === 403) {
            // We log the error using the HyperThought authentication error widget.
            Logging.log({
              severity: LogSeverity.Warning,
              source: "Data API",
              title: "HyperThought&trade; Authentication Required",
              widget: HyperthoughtAuthenticationWidget(),
              sticky: true,
            } as LogWidget);
            throw err;
          }
        }

        // If we did not handle the error before, we emit a general error message.
        Logging.log({
          severity: LogSeverity.Error,
          source: "Data API",
          title: "API Error",
          message: err.message ?? "Failed to retrieve data.",
          data: err,
        });
      }
      throw err;
    }
  }

  @GeneralApi.route("GET", "api/data")
  static async getSourcesAsync() {
    return (await DataApi.requestGeneralAsync({
      ...DataApi.retrieveAuxiliaryParameters(),
    })) as string[];
  }
  @GeneralApi.route("GET", "api/data/{source}")
  static async getResourcesAsync({ source }: { source: string }) {
    return (await DataApi.requestGeneralAsync({
      ...DataApi.retrieveAuxiliaryParameters(source),
      source,
    })) as string[];
  }
  // @GeneralApi.route("GET", "api/data/{source}/{resource}")
  // static async getGraphPropertiesAsync({
  //   source,
  //   resource,
  //   parameters,
  // }: {
  //   source: string;
  //   resource: string;
  //   parameters?: Record<string, any>;
  // }) {}
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
    return (await DataApi.requestGeneralAsync({
      ...DataApi.retrieveAuxiliaryParameters(source, resource),
      source,
      resource,
      ...parameters,
      selector: type,
      ...props,
    })) as Graph;
  }
  // @GeneralApi.route("POST", "api/data/user")
  // static async createGraphAsync({ graph }: { graph: Graph }) {
  //   return (await GeneralApi.requestGeneralAsync(
  //     {
  //       ...DataApi.retrieveAuxiliaryParameters("user"),
  //       source: "user",
  //     },
  //     { body: JSON.stringify(graph) }
  //   )) as Graph;
  // }
  // @GeneralApi.route("DELETE", "api/data/user/{resource}")
  // static async deleteGraphAsync({ resource }: { resource: string }) {
  //   return (await GeneralApi.requestGeneralAsync({
  //     ...DataApi.retrieveAuxiliaryParameters("user", resource),
  //     source: "user",
  //     resource: resource,
  //   })) as null;
  // }
  static getGraphRootsAsync({
    source,
    resource,
    parameters,
  }: {
    source: string;
    resource: string;
    parameters?: Record<string, any>;
  }) {
    return DataApi.getGraphSelectionAsync({
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
    return DataApi.getGraphSelectionAsync({
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
    return DataApi.getGraphSelectionAsync({
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
