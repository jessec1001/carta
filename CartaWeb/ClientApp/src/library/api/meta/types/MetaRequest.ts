export default interface MetaRequest {
  name: string;
  arguments: Record<string, string>;
  body?: any;
}
