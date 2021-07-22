import BaseAPI from "./BaseAPI";
import {
  parseWorkspace,
  parseWorkspaceDataset,
  parseWorkspaceUser,
  Workspace,
  WorkspaceDTO,
  WorkspaceDataset,
  WorkspaceDatasetDTO,
  WorkspaceUser,
  WorkspaceUserDTO,
} from "./workspace";

class WorkspaceAPI extends BaseAPI {
  protected getApiUrl() {
    return "/api/workspace";
  }
  protected getWorkspaceUrl(workspaceId: string) {
    return `${this.getApiUrl()}/${workspaceId}`;
  }

  // #region Workspace Base
  /**
   * Retrieves all workspaces that the current user has access to.
   * @param archived Whether we should search through archived or unarchived workspaces.
   * @returns A list of workspaces.
   */
  public async getWorkspaces(archived?: boolean): Promise<Workspace[]> {
    const url = `${this.getApiUrl()}?archived=${archived ?? false}`;
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
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
          const [users, datasets] = await Promise.all([
            this.getWorkspaceUsers(workspace.id),
            this.getWorkspaceDatasets(workspace.id),
          ]);

          // We combine after all has been retrieved for a particular workspace.
          return {
            ...workspace,
            users,
            datasets,
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
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
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
    const [workspace, users, datasets] = await Promise.all([
      this.getWorkspace(workspaceId),
      this.getWorkspaceUsers(workspaceId),
      this.getWorkspaceDatasets(workspaceId),
    ]);

    // We combine after all has been retrieved.
    return {
      ...workspace,
      users,
      datasets,
    };
  }
  /**
   * Creates a new workspace with the specified name.
   * @param workspaceName The name of the new workspace.
   * @returns The newly created workspace.
   */
  public async createWorkspace(workspaceName: string): Promise<Workspace> {
    const url = `${this.getApiUrl()}/${encodeURIComponent(workspaceName)}`;
    const response = await fetch(url, { method: "POST" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to create workspace."
    );

    return parseWorkspace(await this.readJSON<WorkspaceDTO>(response));
  }
  public async createCompleteWorkspace(
    workspace: Workspace
  ): Promise<Workspace> {
    const newWorkspace = await this.createWorkspace(workspace.name);

    // TODO: Add support for posting datasets.
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
    const url = `${this.getWorkspaceUrl(workspaceId)}?archived=true`;
    const response = await fetch(url, { method: "PATCH" });

    this.ensureSuccess(
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
    const url = `${this.getWorkspaceUrl(workspaceId)}?archived=false`;
    const response = await fetch(url, { method: "PATCH" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to unarchive workspace."
    );

    return parseWorkspace(await this.readJSON<WorkspaceDTO>(response));
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
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
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
    const response = await fetch(url, {
      method: "PATCH",
      body: this.writeJSON(users),
    });

    this.ensureSuccess(
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
    const url = `${this.getWorkspaceUrl(workspaceId)}/users?${users
      .map((user) => `users=${user.id}`)
      .join("&")}`;
    const response = await fetch(url, { method: "DELETE" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to remove users from workspace."
    );
  }
  // #endregion

  // #region Workspace Datasets
  /**
   * Retrieves the datasets that are stored in a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @returns The list of datasets in the workspace.
   */
  public async getWorkspaceDatasets(
    workspaceId: string
  ): Promise<WorkspaceDataset[]> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/data`;
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace datasets."
    );

    return (await this.readJSON<WorkspaceDatasetDTO[]>(response)).map(
      parseWorkspaceDataset
    );
  }
  /**
   * Retrieves a specific dataset that is stored in a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param datasetId The unique identifier of the dataset.
   * @returns The specified dataset in the workspace.
   */
  public async getWorkspaceDataset(
    workspaceId: string,
    datasetId: string
  ): Promise<WorkspaceDataset> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/data/${encodeURIComponent(
      datasetId
    )}`;
    const response = await fetch(url, { method: "GET" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace dataset."
    );

    return parseWorkspaceDataset(
      await this.readJSON<WorkspaceDatasetDTO>(response)
    );
  }
  /**
   * Adds a dataset to a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param source The dataset source.
   * @param resource The dataset resource.
   * @returns The newly added dataset in the workspace.
   */
  public async addWorkspaceDataset(
    workspaceId: string,
    source: string,
    resource: string
  ): Promise<WorkspaceDataset> {
    const dataUrlPart = `${encodeURIComponent(source)}/${encodeURIComponent(
      resource
    )}`;
    const url = `${this.getWorkspaceUrl(workspaceId)}/data/${dataUrlPart}`;
    const response = await fetch(url, { method: "POST" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to add dataset to workspace."
    );

    return parseWorkspaceDataset(
      await this.readJSON<WorkspaceDatasetDTO>(response)
    );
  }
  /**
   * Removes a dataset from a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param datasetId The unique identifier of the dataset.
   */
  public async removeWorkspaceDataset(
    workspaceId: string,
    datasetId: string
  ): Promise<void> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/data/${encodeURIComponent(
      datasetId
    )}`;
    const response = await fetch(url, { method: "DELETE" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to remove dataset from workspace."
    );
  }
  /**
   * Updates a dataset in a specific workspace with a name and/or applied workflow.
   * @param workspaceId The unique identifier of the workspace.
   * @param dataset The dataset to update in the workspace. Must contain an identifier, and the fields to update.
   * @returns The updated dataset in the workspace.
   */
  public async updateWorkspaceDataset(
    workspaceId: string,
    dataset: WorkspaceDataset
  ): Promise<WorkspaceDataset> {
    const params = {
      name: dataset.name,
      workflow: dataset.workflowId,
    };
    const url = `${this.getWorkspaceUrl(workspaceId)}/data/${encodeURIComponent(
      dataset.id
    )}?${Object.entries(params)
      .filter(([key, value]) => value !== undefined)
      .map(([key, value]) => `${key}=${value}`)
      .join("&")}`;
    const response = await fetch(url, { method: "PATCH" });

    this.ensureSuccess(
      response,
      "Error occurred while trying to update workspace dataset."
    );

    return parseWorkspaceDataset(
      await this.readJSON<WorkspaceDatasetDTO>(response)
    );
  }
  // #endregion
}

export default WorkspaceAPI;
