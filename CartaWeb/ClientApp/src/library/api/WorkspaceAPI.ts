import queryString from "query-string";
import BaseAPI from "./BaseAPI";
import {
  parseWorkspace,
  parseWorkspaceUser,
  Workspace,
  WorkspaceDTO,
  WorkspaceUser,
  WorkspaceUserDTO,
  parseWorkspaceChange,
  WorkspaceChangeDTO,
  WorkspaceChange,
  WorkspaceChangeType,
  parseWorkspaceOperation,
  WorkspaceOperationDTO,
  WorkspaceOperation,
} from "./workspace";

/** Contains methods for accessing the Carta Workspace API module. */
class WorkspaceAPI extends BaseAPI {
  public getApiUrl() {
    return "/api/workspace";
  }
  public getWorkspaceUrl(workspaceId: string) {
    return `${this.getApiUrl()}/${workspaceId}`;
  }

  // #region Workspace Base
  /**
   * Retrieves all workspaces that the current user has access to.
   * @param archived Whether we should search through archived or unarchived workspaces.
   * @returns A list of workspaces.
   */
  public async getWorkspaces(archived?: boolean): Promise<Workspace[]> {
    const url = queryString.stringifyUrl({
      url: this.getApiUrl(),
      query: { archived: archived },
    });
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspaces."
    );

    return (await this.readJSON<WorkspaceDTO[]>(response)).map(parseWorkspace);
  }
  /**
   * Retrieves all workspaces, along with their users and datasets, that the current user has access to.
   * @param archived Whether we should search through archived or unarchived workspaces.
   * @returns A list of workspaces.
   */
  public async getCompleteWorkspaces(archived?: boolean): Promise<Workspace[]> {
    // We query the base workspaces first to iterate over to retrieve more information.
    const workspaces = await this.getWorkspaces(archived);

    // We find the remaining additional information for each workspace second.
    return await Promise.all(
      workspaces.map((workspace) => {
        return (async () => {
          // We retrieve the additional information that we are missing for a particular workspace.
          const [users, operations, changes] = await Promise.all([
            this.getWorkspaceUsers(workspace.id),
            this.getWorkspaceOperations(workspace.id),
            this.getWorkspaceChanges(workspace.id),
          ]);

          // We combine after all has been retrieved for a particular workspace.
          return {
            ...workspace,
            users,
            operations,
            changes,
          };
        })();
      })
    );
  }
  /**
   * Retrieves a specific workspace that the current user has access to.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The specified workspace.
   */
  public async getWorkspace(workspaceId: string): Promise<Workspace> {
    const url = this.getWorkspaceUrl(workspaceId);
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch a workspace."
    );

    return parseWorkspace(await this.readJSON<WorkspaceDTO>(response));
  }
  /**
   * Retrieves a specific workspace, along with its users and datasets, that the current user has access to.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The specified workspace.
   */
  public async getCompleteWorkspace(workspaceId: string): Promise<Workspace> {
    // We retrieve each of the parts of a complete workspace at the same time.
    const [workspace, users, operations, changes] = await Promise.all([
      this.getWorkspace(workspaceId),
      this.getWorkspaceUsers(workspaceId),
      this.getWorkspaceOperations(workspaceId),
      this.getWorkspaceChanges(workspaceId),
    ]);

    // We combine after all has been retrieved.
    return {
      ...workspace,
      users,
      operations,
      changes,
    };
  }
  /**
   * Creates a new workspace with the specified name.
   * @param workspaceName The name of the new workspace.
   * @returns The newly created workspace.
   */
  public async createWorkspace(workspaceName: string): Promise<Workspace> {
    const url = `${this.getApiUrl()}/${encodeURIComponent(workspaceName)}`;
    const response = await this.fetch(url, this.defaultFetchParameters("POST"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to create workspace."
    );

    return parseWorkspace(await this.readJSON<WorkspaceDTO>(response));
  }
  /**
   * Creates a new workspace with the specified name and existing users and datasets.
   * @param workspace The workspace object to create.
   * @returns The newly created workspace.
   */
  public async createCompleteWorkspace(
    workspace: Workspace
  ): Promise<Workspace> {
    const newWorkspace = await this.createWorkspace(workspace.name!);

    // TODO: Add support for posting operations.
    // We add the users and datasets to the new workspace if included.
    const [users] = await Promise.all([
      workspace.users === undefined || workspace.users.length === 0
        ? workspace.users
        : this.addWorkspaceUsers(newWorkspace.id, workspace.users),
    ]);

    // Merge in the newly created resources from the server.
    newWorkspace.users = users;

    return newWorkspace;
  }
  /**
   * Archives a specific workspace so that it is no longer visible to the user.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The specified workspace after being archived.
   */
  public async archiveWorkspace(workspaceId: string): Promise<Workspace> {
    const url = queryString.stringifyUrl({
      url: this.getWorkspaceUrl(workspaceId),
      query: { archived: true },
    });
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("PATCH")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to archive workspace."
    );

    return parseWorkspace(await this.readJSON<WorkspaceDTO>(response));
  }
  /**
   * Unarchives a specific workspace so that it is visible to the user.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The specified workspace after being unarchived.
   */
  public async unarchiveWorkspace(workspaceId: string): Promise<Workspace> {
    const url = queryString.stringifyUrl({
      url: this.getWorkspaceUrl(workspaceId),
      query: { archived: false },
    });
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("PATCH")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to unarchive workspace."
    );

    return parseWorkspace(await this.readJSON<WorkspaceDTO>(response));
  }
  // #endregion

  // #region Workspace Changes
  /**
   * Retrieves a list of ordered changes that have occurred within a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param type Optional type specifying the type of changes to include. If not specified, all types of changes will be
   * included.
   * @param dateFrom Optional starting date for changes to include. If not specified, changes from the indefinite past
   * will be included.
   * @param dateTo Optional ending date for changes to include. If not specified, changes to the present will be
   * included.
   * @returns A list of workspace changes.
   */
  public async getWorkspaceChanges(
    workspaceId: string,
    type?: WorkspaceChangeType,
    dateFrom?: Date,
    dateTo?: Date
  ): Promise<WorkspaceChange[]> {
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/changes`,
      query: {
        type: type,
        dateFrom: dateFrom && dateFrom.toISOString(),
        dateTo: dateTo && dateTo.toISOString(),
      },
    });
    const response = await this.fetch(url, this.defaultFetchParameters());

    this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace changes."
    );

    return (await this.readJSON<WorkspaceChangeDTO[]>(response)).map(
      parseWorkspaceChange
    );
  }
  // #endregion

  // #region Workspace Users
  /**
   * Retrieves the users that have been added to or removed from a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The list of users of the workspace.
   */
  public async getWorkspaceUsers(
    workspaceId: string
  ): Promise<WorkspaceUser[]> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/users`;
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace users."
    );

    return (await this.readJSON<WorkspaceUserDTO[]>(response)).map(
      parseWorkspaceUser
    );
  }
  /**
   * Adds each user from a list to a workspace so that it is now accessible to them.
   * @param workspaceId The unique identifier of the workspace.
   * @param users The users to add to the workspace.
   * @returns The list of all users of the workspace after adding new users.
   */
  public async addWorkspaceUsers(
    workspaceId: string,
    users: WorkspaceUser[]
  ): Promise<WorkspaceUser[]> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/users`;
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("PATCH", users)
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to add users to workspace."
    );

    return (await this.readJSON<WorkspaceUserDTO[]>(response)).map(
      parseWorkspaceUser
    );
  }
  /**
   * Removes each user from a list from a workspace so that it is no longer accessible to them.
   * @param workspaceId The unique identifier of the workspace.
   * @param users The users to remove from the workspace.
   */
  public async removeWorkspaceUsers(
    workspaceId: string,
    users: WorkspaceUser[]
  ): Promise<void> {
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/users`,
      query: {
        users: users.map((user) => user.userInformation.id),
      },
    });
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("DELETE")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to remove users from workspace."
    );
  }
  // #endregion

  // #region Workspace Operations
  /**
   * Retrieves the operations that are contained within a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The list of operations of the workspace.
   */
  public async getWorkspaceOperations(
    workspaceId: string
  ): Promise<WorkspaceOperation[]> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/operations`;
    const response = await this.fetch(url, this.defaultFetchParameters("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace operations."
    );

    return (await this.readJSON<WorkspaceOperationDTO[]>(response)).map(
      parseWorkspaceOperation
    );
  }
  /**
   * Retrieves a specific operation that is contained within a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param operationId The unique identifier of the operation.
   * @returns The specified operation of the workspace.
   */
  public async getWorkspaceOperation(
    workspaceId: string,
    operationId: string
  ): Promise<WorkspaceOperation> {
    const url = `${this.getWorkspaceUrl(
      workspaceId
    )}/operations/${operationId}`;
    const response = await this.fetch(url, this.defaultFetchParameters("GET"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace operation."
    );

    return parseWorkspaceOperation(
      await this.readJSON<WorkspaceOperationDTO>(response)
    );
  }
  /**
   * Adds an operation to a workspace so that it is now accessible to members of the workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param operationId The unique identifier of the operation.
   * @returns The operation that was added to the workspace.
   */
  public async addWorkspaceOperation(
    workspaceId: string,
    operationId: string
  ): Promise<WorkspaceOperation> {
    const url = `${this.getWorkspaceUrl(
      workspaceId
    )}/operations/${operationId}`;
    const response = await this.fetch(url, this.defaultFetchParameters("POST"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to add operation to workspace."
    );

    return parseWorkspaceOperation(
      await this.readJSON<WorkspaceOperationDTO>(response)
    );
  }
  /**
   * Removes an operation from a workspace so that it is no longer accessible to members of the workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param operationId The unique identifier of the operation.
   */
  public async removeWorkspaceOperation(
    workspaceId: string,
    operationId: string
  ): Promise<void> {
    const url = `${this.getWorkspaceUrl(
      workspaceId
    )}/operations/${operationId}`;
    const response = await this.fetch(
      url,
      this.defaultFetchParameters("DELETE")
    );

    await this.ensureSuccess(
      response,
      "Error occurred while trying to remove operation from workspace."
    );
  }
  // #endregion
}

export default WorkspaceAPI;
