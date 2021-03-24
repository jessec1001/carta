import queryString from "query-string";
import {
  dataGetGraph,
  dataGetSources,
  dataGetResources,
  dataGetChildren,
  dataGetVertex,
} from "./api/data";

export * from "./api/data";
export * from "./api/meta";

interface RequestInfo {
  method: string;
  path: string;
  endpoint: (
    values: Record<string, string>,
    parameters?: Record<string, any>
  ) => Promise<any>;
}

const knownRequests: RequestInfo[] = [
  {
    method: "get",
    path: "api/data",
    endpoint: (parameters?: Record<string, any>) => dataGetSources(parameters),
  },
  {
    method: "get",
    path: "api/data/{source}",
    endpoint: (
      { source }: Record<string, string>,
      parameters?: Record<string, any>
    ) => dataGetResources(source, parameters),
  },
  {
    method: "get",
    path: "api/data/{source}/{resource}",
    endpoint: (
      { source, resource }: Record<string, string>,
      parameters?: Record<string, any>
    ) => dataGetGraph(source, resource, parameters),
  },
  {
    method: "get",
    path: "api/data/{source}/{resource}/props",
    endpoint: (
      { source, resource }: Record<string, string>,
      parameters?: Record<string, any>
    ) => {
      const {id, ...params} = parameters as any;
      return dataGetVertex(source, resource, id, params)
    }
  },
  {
    method: "get",
    path: "api/data/{source}/{resource}/children",
    endpoint: (
      {source, resource}: Record<string, string>,
      parameters?: Record<string, any>
    ) => {
      const {id, ...params} = parameters as any;
      return dataGetChildren(source, resource, id, params)
    }
  }
];

export async function generalRequest(
  path: string,
  parameters?: Record<string, any>,
  body?: any,
  method?: string
) {
  method = method ?? "GET";
  const url: string = queryString.stringifyUrl({
    url: path,
    query: parameters,
  });

  const response = await fetch(url, {
    method: method,
    body: body,
    headers: {
      "Content-Type": "application/json",
    },
  });
  return await response.json();
}

export async function unknownRequest(
  path: string,
  parameters?: Record<string, any>,
  body?: any,
  method?: string
): Promise<any> {
  method = method ?? "GET";
  for (let k = 0; k < knownRequests.length; k++) {
    const request: RequestInfo = knownRequests[k];
    const pathRegex = `^${request.path.replace(/\{(.*?)\}/g, "(?<$1>[^/]*?)")}$`;
    const match = path.match(new RegExp(pathRegex));
    console.log(path, pathRegex, match);
    if (method === request.method && match !== null) {
      return request.endpoint(match.groups ?? {}, parameters);
    }
  }
  return generalRequest(path, parameters, body, method);
}
