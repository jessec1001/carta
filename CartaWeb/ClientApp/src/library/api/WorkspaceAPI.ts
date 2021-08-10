import queryString from "query-string";
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
  parseWorkspaceChange,
  WorkspaceChangeDTO,
  WorkspaceChange,
  WorkspaceChangeType,
  WorkspaceWorkflowDTO,
  parseWorkspaceWorkflow,
  WorkspaceWorkflow,
} from "./workspace";

/** Contains methods for accessing the Carta Workspace API module. */
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
    const url = queryString.stringifyUrl({
      url: this.getApiUrl(),
      query: { archived: archived },
    });
    const response = await fetch(url, this.defaultFetcher());

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
    const response = await fetch(url, this.defaultFetcher());

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
    const response = await fetch(url, this.defaultFetcher("POST"));

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

    // TODO: Add support for posting datasets.
    // TODO: Add support for posting workflows.
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
    const response = await fetch(url, this.defaultFetcher("PATCH"));

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
    const response = await fetch(url, this.defaultFetcher("PATCH"));

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
      url: this.getWorkspaceUrl(workspaceId),
      query: {
        type: type,
        dateFrom: dateFrom && dateFrom.toISOString(),
        dateTo: dateTo && dateTo.toISOString(),
      },
    });
    const response = await fetch(url, this.defaultFetcher());

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
    const response = await fetch(url, this.defaultFetcher());

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
    const response = await fetch(url, this.defaultFetcher("PATCH", users));

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
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
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
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
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
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
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
    const response = await fetch(url, this.defaultFetcher("POST"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to add dataset to workspace."
    );

    return parseWorkspaceDataset(
      await this.readJSON<WorkspaceDatasetDTO>(response)
    );
  }
  /**
   * Adds a dataset with the specified name and workflow to a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param dataset The dataset to add.
   * @returns The newly added dataset in the workspace.
   */
  public async addCompleteWorkspaceDataset(
    workspaceId: string,
    dataset: WorkspaceDataset
  ): Promise<WorkspaceDataset> {
    let newDataset: WorkspaceDataset;
    newDataset = await this.addWorkspaceDataset(
      workspaceId,
      dataset.source,
      dataset.resource
    );
    newDataset = await this.updateWorkspaceDataset(workspaceId, {
      ...newDataset,
      name: dataset.name,
      workflow: dataset.workflow,
    });
    return newDataset;
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
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
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
      workflow: dataset.workflow,
      workflowVersion: dataset.workflowVersion,
    };
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/data/${encodeURIComponent(
        dataset.id
      )}`,
      query: params,
    });
    const response = await fetch(url, this.defaultFetcher("PATCH"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to update workspace dataset."
    );

    return parseWorkspaceDataset(
      await this.readJSON<WorkspaceDatasetDTO>(response)
    );
  }
  // #endregion

  // #region Workspace Workflows
  /**
   * Retrieves the workflows that are stored in a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param archived Whether we should search through archived or unarchived workspaces.
   * @returns A list of workflows in the workspace.
   */
  public async getWorkspaceWorkflows(
    workspaceId: string,
    archived?: boolean
  ): Promise<WorkspaceWorkflow[]> {
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/workflows`,
      query: { archived: archived },
    });
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace workflows."
    );

    return (await this.readJSON<WorkspaceWorkflowDTO[]>(response)).map(
      parseWorkspaceWorkflow
    );
  }
  /**
   * Retrieves a specific workflow that is stored in a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param workflowId The unique identifier of the workflow.
   * @returns The specified workflow in the workspace.
   */
  public async getWorkspaceWorkflow(
    workspaceId: string,
    workflowId: string
  ): Promise<WorkspaceWorkflow> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/workflows/${workflowId}`;
    const response = await fetch(url, this.defaultFetcher());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch workspace workflow."
    );

    return parseWorkspaceWorkflow(
      await this.readJSON<WorkspaceWorkflowDTO>(response)
    );
  }
  /**
   * Adds a workflow to a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param workflowId The unique identifier of the workflow.
   * @returns The newly added workflow in the workspace.
   */
  public async addWorkspaceWorkflow(
    workspaceId: string,
    workflowId: string
  ): Promise<WorkspaceWorkflow> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/workflows/${workflowId}`;
    const response = await fetch(url, this.defaultFetcher("POST"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to add workflow to workspace."
    );

    return parseWorkspaceWorkflow(
      await this.readJSON<WorkspaceWorkflowDTO>(response)
    );
  }
  /**
   * Archives a specific workflow that is stored in a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param workflowId The unique identifier of the workflow.
   * @returns The archived workflow in the workspace.
   */
  public async archiveWorkspaceWorkflow(
    workspaceId: string,
    workflowId: string
  ): Promise<WorkspaceWorkflow> {
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/workflows/${workflowId}`,
      query: { archived: true },
    });
    const response = await fetch(url, this.defaultFetcher("PATCH"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to archive workflow in workspace."
    );

    return parseWorkspaceWorkflow(
      await this.readJSON<WorkspaceWorkflowDTO>(response)
    );
  }
  /**
   * Unarchives a specific workflow that is stored in a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param workflowId The unique identifier of the workflow.
   * @returns The unarchived workflow in the workspace.
   */
  public async unarchiveWorkspaceWorkflow(
    workspaceId: string,
    workflowId: string
  ): Promise<WorkspaceWorkflow> {
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/workflows/${workflowId}`,
      query: { archived: true },
    });
    const response = await fetch(url, this.defaultFetcher("PATCH"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to unarchive workflow in workspace."
    );

    return parseWorkspaceWorkflow(
      await this.readJSON<WorkspaceWorkflowDTO>(response)
    );
  }
  /**
   * Removes a workflow from a specific workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param workflowId The unique identifier of the workflow.
   */
  public async removeWorkspaceWorkflow(
    workspaceId: string,
    workflowId: string
  ): Promise<void> {
    const url = `${this.getWorkspaceUrl(workspaceId)}/workflows/${workflowId}`;
    const response = await fetch(url, this.defaultFetcher("DELETE"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to delete workflow from workspace."
    );
  }
  /**
   * Changes the version number of a workflow in a workspace.
   * @param workspaceId The unique identifier of the workspace.
   * @param workflowId The unique identifier of the workflow.
   * @param workflowVersion The version number to change the workflow to.
   * @returns The reverted workflow in the workspace.
   */
  public async revertWorkspaceWorkflowVersion(
    workspaceId: string,
    workflowId: string,
    workflowVersion: number
  ): Promise<WorkspaceWorkflow> {
    const url = queryString.stringifyUrl({
      url: `${this.getWorkspaceUrl(workspaceId)}/workflows/${workflowId}`,
      query: { workflowVersion },
    });
    const response = await fetch(url, this.defaultFetcher("PATCH"));

    await this.ensureSuccess(
      response,
      "Error occurred while trying to change workflow version in workspace."
    );

    return parseWorkspaceWorkflow(
      await this.readJSON<WorkspaceWorkflowDTO>(response)
    );
  }
  // #endregion
}

export default WorkspaceAPI;
