import { MetaParameter, MetaRequest } from ".";

export default interface MetaEndpoint {
  path: string;
  method: string;
  description?: string;
  parameters: MetaParameter[];
  requests?: MetaRequest[];
  returns?: Record<number, string>;
}
