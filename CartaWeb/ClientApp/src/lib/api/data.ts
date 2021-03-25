import { generalRequest } from "../carta";

import { Graph } from "../types/graph";

function getStoredParameters(
  source?: string,
  resource?: string
): Record<string, any> {
  const parameters: Record<string, any> = {};
  if (source && source.toLowerCase() === "hyperthought") {
    const hyperthoughtApiKey = localStorage.getItem("hyperthoughtKey");
    parameters["api"] = hyperthoughtApiKey;
  }
  return parameters;
}

export async function dataGetSources(parameters?: Record<string, any>) {
  return (await generalRequest(`api/data`, {
    ...getStoredParameters(),
    ...parameters,
  })) as string[];
}
export async function dataGetResources(
  source: string,
  parameters?: Record<string, any>
) {
  return (await generalRequest(`api/data/${source}`, {
    ...getStoredParameters(source),
    ...parameters,
  })) as string[];
}

/**
 * Gets a graph specified by source and resource from the Carta API.
 * @param source The primary source of the the graph to be retrieved. For instance, "synthetic" or "hyperthought".
 * @param resource The secondary source of the graph to be retrieved. For instance, the name or unique identifier in the source.
 * @param parameters The extra parameters to pass on to the API call.
 */
export async function dataGetGraph(
  source: string,
  resource: string,
  parameters?: Record<string, any>
) {
  return (await generalRequest(`api/data/${source}/${resource}`, {
    ...getStoredParameters(source, resource),
    ...parameters,
  })) as Graph;
}
/**
 * Gets a vertex specified by source and resource, and a unique vertex identifier from the Carta API.
 * @param source The primary source of the the graph to be retrieved. For instance, "synthetic" or "hyperthought".
 * @param resource The secondary source of the graph to be retrieved. For instance, the name or unique identifier in the source.
 * @param id The identifer for the vertex to retrieve.
 * @param parameters The extra parameters to pass on to the API call.
 */
export async function dataGetVertex(
  source: string,
  resource: string,
  id: string,
  parameters?: Record<string, any>
) {
  return (await generalRequest(`api/data/${source}/${resource}/props`, {
    ...getStoredParameters(source, resource),
    ...parameters,
    id,
  })) as Graph;
}
/**
 * Gets the children of a vertex specified by source and resource, and a unique vertex identifier from the Carta API.
 * @param source The primary source of the the graph to be retrieved. For instance, "synthetic" or "hyperthought".
 * @param resource The secondary source of the graph to be retrieved. For instance, the name or unique identifier in the source.
 * @param id The identifer for the vertex to retrieve the children of.
 * @param parameters The extra parameters to pass on to the API call.
 */
export async function dataGetChildren(
  source: string,
  resource: string,
  id: string,
  parameters?: Record<string, any>
) {
  return (await generalRequest(`api/data/${source}/${resource}/children`, {
    ...getStoredParameters(source, resource),
    ...parameters,
    id,
  })) as Graph;
}
