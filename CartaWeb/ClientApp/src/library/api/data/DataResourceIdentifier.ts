/** Represents a unique identifier for a data resource including its parameters. */
interface DataResourceIdentifier {
  /** The source of the data. If not specified, represents a wildcard. */
  source?: string;
  /** The resource of the data. If not specified, represents a wildcard. */
  resource?: string;

  /** The parameters set on the specified resource. */
  parameters: Map<string, any>;
}

export type { DataResourceIdentifier };
