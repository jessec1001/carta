export interface ApiParameter {
  name: string;
  type: string;
  format: "route" | "query";
  description?: string;
}
export interface ApiRequest {
  name: string;
  arguments: Record<string, string>;
}
export interface ApiEndpoint {
  path: string;
  method: string;
  description?: string;
  parameters: ApiParameter[];
  requests: ApiRequest[];
  returns: Record<number, string>;
}
export interface ApiCollection {
  name: string;
  description?: string;
  endpoints: ApiEndpoint[];
}
