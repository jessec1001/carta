interface OperationType {
  type: string;
  subtype: string | null;

  selector: string | null;
  visualization: string | null;

  display: string;
  description: string | null;
  tags: string[];
}

export default OperationType;
