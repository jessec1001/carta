import { Identifiable } from "../base";

interface Job<TResult = Record<string, any>> extends Identifiable {
  completed: boolean;
  value: Record<string, any>;
  result?: TResult;

  // TODO: tasks: JobTask[];
}

export default Job;
