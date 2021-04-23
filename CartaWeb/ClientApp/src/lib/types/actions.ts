interface ActionToNumber {
  type: "toNumber";
}
interface ActionIncrement {
  type: "increment";
  amount?: number;
}
interface ActionDecrement {
  type: "decrement";
  amount?: number;
}
interface ActionStringReplace {
  type: "stringReplace";
  pattern: string;
  replacement: string;
}
interface ActionMean {
  type: "mean";
}
interface ActionMedian {
  type: "median";
}
interface ActionStandardDeviation {
  type: "deviation";
}
interface ActionVariance {
  type: "variance";
}
interface ActionAggregate {
  type: "aggregate";
}
interface ActionPropagate {
  type: "propagate";
}
interface ActionReverseEdges {
  type: "reverseEdges";
}

type Action =
  | ActionToNumber
  | ActionIncrement
  | ActionDecrement
  | ActionStringReplace
  | ActionMean
  | ActionMedian
  | ActionStandardDeviation
  | ActionVariance
  | ActionAggregate
  | ActionPropagate
  | ActionReverseEdges;
export default Action;
