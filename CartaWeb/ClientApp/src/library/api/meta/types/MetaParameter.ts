export default interface MetaParameter {
  name: string;
  type: string;
  format: "route" | "query" | "body";
  description?: string;
}
