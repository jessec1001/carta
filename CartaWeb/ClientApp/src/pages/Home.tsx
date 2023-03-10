import { FunctionComponent, useCallback, useContext, useState } from "react";
import { useHistory } from "react-router";
import { useAPI, useNestedAsync, useStoredState } from "hooks";
import { UserContext } from "components/user";
import { Workspace } from "library/api";
import { ObjectFilter } from "library/search";
import { IconButtonAdd } from "components/buttons";
import { SearchboxInput } from "components/input";
import { AnimatedJumbotron } from "components/jumbotron";
import { PageLayout, Wrapper } from "components/layout";
import { Column, Row } from "components/structure";
import { Loading, Text, Paragraph, Title } from "components/text";
import { UserNeedsAuthentication } from "components/user";
import { WorkspaceCarousel } from "components/workspace";
import { seconds } from "library/utility";

/** A page-specific component to render the workspaces section when the user is unauthenticated. */
const WorkspacesUnauthenticated: FunctionComponent = () => {
  return (
    <section>
      {/* Render a simplified heading section for the workspaces area. */}
      <Row>
        <Column>
          <Text size="large">Workspaces</Text>
        </Column>
        <Column />
      </Row>

      {/* Notify the user that they need to be authenticated. */}
      <UserNeedsAuthentication />
    </section>
  );
};

/** A page-specific component to render the workspaces section when the user is authenticated. */
const WorkspacesAuthenticated: FunctionComponent = () => {
  // TODO: Replace calls to getCompletedWorkspaces with nested calls.
  // TODO: Review how we might load all workspaces, then all their user and operation ID's, then all of their user informations and operation informations more efficiently.
  // We use the workspace API to load in the workspaces.
  const { workspaceAPI } = useAPI();
  const loadWorkspaces = useCallback(async () => {
    return await workspaceAPI.getCompleteWorkspaces(false);
  }, [workspaceAPI]);
  const [workspaces] = useNestedAsync<typeof loadWorkspaces, Workspace[]>(
    loadWorkspaces,
    true,
    seconds(15)
  );

  // We use a query specified in a search bar to filter through the workspaces.
  const [query, setQuery] = useState("");
  const workspaceFilter = new ObjectFilter(query, {
    defaultProperty: "name",
    mappedProperties: new Map([
      [["user"], ["users", "userInformation"]],
      [["operation"], ["operations"]],
    ]),
  });

  // We sort workspaces by the date they were last accessed at (per this user).
  const [workspaceHistory] = useStoredState<Record<string, number>>(
    {},
    "workspaceHistory"
  );
  const workspaceSorter = (workspace1: Workspace, workspace2: Workspace) => {
    const access1 = workspaceHistory[workspace1.id];
    const access2 = workspaceHistory[workspace2.id];
    if (access1 === undefined && access2 === undefined) return 0;
    if (access1 === undefined) return +1;
    if (access2 === undefined) return -1;
    return access2 - access1;
  };

  // Process the workspaces through the query filter and the access sorter.
  let renderWorkspaces = workspaces;
  if (!(renderWorkspaces === undefined || renderWorkspaces instanceof Error)) {
    renderWorkspaces = workspaceFilter.filter(renderWorkspaces);
    renderWorkspaces = renderWorkspaces.sort(workspaceSorter);
  }

  // We need to hook into the browser history to move to the new workspace page when the
  // corresponding button is clicked.
  const history = useHistory();
  const navigateNewWorkspace = () => {
    history.push({
      pathname: "/workspace/new",
    });
  };

  return (
    <section>
      {/* Render a common heading section for the workspaces area. */}
      <Row>
        <Column>
          <Title level={2} size="large" align="middle">
            Workspaces &nbsp;
            <Text size="reset">
              <IconButtonAdd onClick={navigateNewWorkspace} />
            </Text>
          </Title>
        </Column>
        <Column>
          <SearchboxInput
            value={query}
            onChange={setQuery}
            placeholder="Search workspaces"
            clearable
          />
        </Column>
      </Row>

      {/* TODO: Add a loading property to the workspaces carousel to indicate that workspaces are still loading. */}
      {/* If workspaces were retrieved, render them in a carousel. */}
      {renderWorkspaces &&
        !(renderWorkspaces instanceof Error) &&
        (renderWorkspaces.length > 0 ? (
          <WorkspaceCarousel workspaces={renderWorkspaces} />
        ) : (
          <Text color="muted">
            No workspaces yet. Click the '+' button to create one!
          </Text>
        ))}

      {/* If an error occurred, render it in error colored text. */}
      {workspaces instanceof Error && (
        <Text color="error">Error occurred: {workspaces.message}</Text>
      )}

      {/* If the data is still loading, render a loading symbol. */}
      {!workspaces && <Loading />}
    </section>
  );
};

/** The page users will see when first visiting the website. */
const HomePage: FunctionComponent = () => {
  // Depending on whether we are authenticated or not, we render a different subcomponent representing this state.
  const { authenticated } = useContext(UserContext);

  return (
    <PageLayout header footer>
      {/* Jumbotron goes here with nice animation. */}
      <AnimatedJumbotron>
        <Title>Welcome to Carta!</Title>
        <Paragraph>
          Carta is a web-based API and application that provides graph-based
          tools for accessing, exploring, and transforming existing datasets and
          models.
        </Paragraph>
      </AnimatedJumbotron>

      {/* Carousel of workspaces for easy access. */}
      <Wrapper>
        {authenticated ? (
          <WorkspacesAuthenticated />
        ) : (
          <WorkspacesUnauthenticated />
        )}
      </Wrapper>
    </PageLayout>
  );
};

export default HomePage;
