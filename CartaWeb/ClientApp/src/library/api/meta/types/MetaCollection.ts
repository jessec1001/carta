import { MetaEndpoint } from ".";

export default interface MetaCollection {
  name: string;
  description?: string;
  endpoints: MetaEndpoint[];
}
