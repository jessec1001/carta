export interface SelectorAll {
  type: "all";
}
export interface SelectorNone {
  type: "none";
}
export interface SelectorOr {
  type: "or";
  selectors: Selector[];
}
export interface SelectorAnd {
  type: "and";
  selectors: Selector[];
}
export interface SelectorInclude {
  type: "include";
  ids: string[];
}
export interface SelectorExclude {
  type: "exclude";
  ids: string[];
}
export interface SelectorExpanded {
  type: "expanded";
}
export interface SelectorCollapsed {
  type: "collapsed";
}
export interface SelectorVertexName {
  type: "vertexName";
  pattern: string;
}
export interface SelectorPropertyName {
  type: "propertyName";
  pattern: string;
}
export interface SelectorPropertyRange {
  type: "propertyRange";
  property: string;
  minimum?: number;
  maximum?: number;
}
export interface SelectorDescendants {
  type: "descendants";
  ids: string[];
  includeRoots?: boolean;
  depth?: number;
  traversal?: "preorder" | "postorder";
}
export interface SelectorAncestors {
  type: "ancestors";
  ids: string[];
  includeRoots?: boolean;
  depth?: number;
  traversal?: "preorder" | "postorder";
}
export interface SelectorChildren {
  type: "children";
  ids: string[];
  includeRoots?: boolean;
  depth?: number;
  traversal?: "preorder" | "postorder";
}
export interface SelectorParents {
  type: "parents";
  ids: string[];
  includeRoots?: boolean;
  depth?: number;
  traversal?: "preorder" | "postorder";
}
export interface SelectorDegree {
  type: "degree";
  inDegree?: number;
  outDegree?: number;
}
export interface SelectorRoots {
  type: "roots";
  inDegree?: number;
  outDegree?: number;
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
  | SelectorAncestors
  | SelectorChildren
  | SelectorParents
  | SelectorDegree
  | SelectorRoots;
export default Selector;
