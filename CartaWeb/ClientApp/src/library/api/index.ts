// TODO: Remove old style API references.
import GeneralApi from "library/api/general";
import MetaApi from "library/api/meta";

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
};
export * from "./data";
export * from "./user";
export * from "./operations";
export * from "./workflows";
export * from "./workspace";
