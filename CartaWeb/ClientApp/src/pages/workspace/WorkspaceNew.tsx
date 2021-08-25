import { FunctionComponent, useRef, useState } from "react";
import { useHistory } from "react-router";
import { UserJsonSchema, Workspace, WorkspaceAPI } from "library/api";
import {
  JsonObjectSchema,
  JsonStringSchema,
  JsonArraySchema,
} from "library/schema";
import { ApiException } from "library/exceptions";
import { PageLayout, Wrapper } from "components/layout";
import { Text, Title } from "components/text";
import {
  UserIsAuthenticated,
  UserIsNotAuthenticated,
  UserNeedsAuthentication,
} from "components/user";
import { SchemaForm } from "components/form/schema";

/** The schema used for the {@link WorkspaceNewPage} page. */
const workspaceCreateSchema: JsonObjectSchema = {
  type: "object",
  description: `
A workspace is a place to store multiple related datasets and transformations together in an easy-to-access format.
It allows for collaboration on workflows between multiple users and provides a natural and clean interface for
exploring and analyzing data.
`,
  properties: {
    name: {
      type: "string",
      title: "Name",
      description: `
This is the name that will be displayed when navigating to, or searching for workspaces.
`,
      minLength: 1,
    } as JsonStringSchema,
    users: {
      type: "array",
      title: "Users",
      description: `
You can share a workspace with numerous other users. You can add these users here or you can modify these settings
later.
`,
      items: UserJsonSchema,
    } as JsonArraySchema,
  } as Record<keyof Workspace, any>,
};

/** The page that allows users to create a new workspace. */
const WorkspaceNewPage: FunctionComponent = () => {
  // We need access to the workspace API to handle requests.
  const workspaceAPIRef = useRef(new WorkspaceAPI());
  const workspaceAPI = workspaceAPIRef.current;

  // We use the history navigate when the form is submitted or canceled.
  const history = useHistory();

  // We use this error text to indicate if a server-client communication error occurred.
  const [error, setError] = useState<ApiException | null>(null);

  const handleSubmit = (value: Workspace) => {
    // When the workspace creation form is submitted, we need to make a request to create the resource on the server.
    (async () => {
      let workspace: Workspace;
      try {
        workspace = await workspaceAPI.createCompleteWorkspace(value);
      } catch (error) {
        if (error instanceof ApiException) setError(error);
        else throw error;
        return;
      }

      // Afterwards, if successful, we redirect to the newly created workspace.
      history.push(`/workspace?id=${workspace.id}`);
    })();
  };
  const handleCancel = () => {
    // Go back to the page which brought us here.
    (history as any).goBack();
  };

  return (
    <PageLayout header footer>
      <Wrapper>
        <Title>Create Workspace</Title>

        {/* Only render the workspace creation form if the user is authenticated. */}
        <UserIsAuthenticated>
          <SchemaForm
            schema={workspaceCreateSchema}
            cancelable
            submitText="Create"
            cancelText="Cancel"
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </UserIsAuthenticated>

        {/* Render a message indicating that the user needs to authenticate to use this page. */}
        <UserIsNotAuthenticated>
          <UserNeedsAuthentication />
        </UserIsNotAuthenticated>

        {/* If there is an error, render text that indicates what went wrong. */}
        {error && (
          <Text color="error">
            {error.message} (Status code: {error.status})
          </Text>
        )}
      </Wrapper>
    </PageLayout>
  );
};

export default WorkspaceNewPage;
