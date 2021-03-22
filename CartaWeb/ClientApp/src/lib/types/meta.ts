export interface ApiParameter {
  name: string;
  type: string;
  format: "route" | "query" | "body";
  description?: string;
}
export interface ApiRequest {
  name: string;
  arguments: Record<string, string>;
  body?: any;
}
export interface ApiEndpoint {
  path: string;
  method: string;
  description?: string;
  parameters: ApiParameter[];
  requests?: ApiRequest[];
  returns?: Record<number, string>;
}
export interface ApiCollection {
  name: string;
  description?: string;
  endpoints: ApiEndpoint[];
}
