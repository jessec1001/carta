// TODO: Remove old style API references.
import DataApi from "library/api/data";
import GeneralApi from "library/api/general";
import MetaApi from "library/api/meta";
import WorkflowApi from "library/api/workflows";

import BaseAPI from "./BaseAPI";
import DataAPI from "./DataAPI";
import UserAPI from "./UserAPI";
import OperationsAPI from "./OperationsAPI";
import WorkflowsAPI from "./WorkflowsAPI";
import WorkspaceAPI from "./WorkspaceAPI";

export {
  BaseAPI,
  DataAPI,
  UserAPI,
  OperationsAPI,
  WorkflowsAPI,
  WorkspaceAPI,
  GeneralApi,
  MetaApi,
  WorkflowApi,
  DataApi,
};
export * from "./data";
export * from "./user";
export * from "./operations";
export * from "./workflows";
export * from "./workspace";
