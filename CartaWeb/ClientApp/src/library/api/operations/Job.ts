import { Identifiable } from "../base";

interface Job extends Identifiable {
  completed: boolean;
  value: Record<string, any>;
  result: Record<string, any> | null;

  // TODO: tasks: JobTask[];
}

export default Job;
