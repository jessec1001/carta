export interface SelectorAll {
  type: "all";
}
export interface SelectorNone {
  type: "none";
}
export interface SelectorOr {
  type: "or",
  selectors: Selector[]
}
export interface SelectorAnd {
  type: "and",
  selectors: Selector[]
}
export interface SelectorInclude {
  type: "include vertex",
  ids: string[]
}
export interface SelectorExclude {
  type: "exclude vertex",
  ids: string[]
}
export interface SelectorExpanded {
  type: "expanded"
}
export interface SelectorCollapsed {
  type: "collapsed"
}
export interface SelectorVertexName {
  type: "vertex name";
  pattern: string;
}
export interface SelectorPropertyName {
  type: "property name";
  pattern: string;
}
export interface SelectorPropertyRange {
  type: "property range";
  property: string;
  minimum?: number;
  maximum?: number;
}
export interface SelectorDescendants {
  type: "vertex descendants";
}
export interface SelectorAncestors {
  type: "vertex ancestors";
}

type Selector =
  | SelectorAll
  | SelectorNone
  | SelectorOr
  | SelectorAnd
  | SelectorInclude
  | SelectorExclude
  | SelectorExpanded
  | SelectorCollapsed
  | SelectorVertexName
  | SelectorPropertyName
  | SelectorPropertyRange
  | SelectorDescendants
  | SelectorAncestors;
export default Selector;
