import {
  FunctionComponent,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { useHistory } from "react-router";
import { UserContext } from "context";
import { Workspace, WorkspaceAPI } from "library/api";
import { Column, Row } from "components/structure";
import { Heading } from "components/text";
import { IconAddButton } from "components/buttons";
import { Searchbox } from "components/input";
import { UserIsAuthenticated, UserNeedsAuthentication } from "components/user";

/** The props used for the {@link WorkspaceCarousel} component. */
interface WorkspaceCarouselProps {
  /**
   * An optionally specifiable list of workspaces that can be used for the carousel.
   * If not specified, all workspaces available to the user will be used.
   */
  workspaces?: Workspace[];
}

const WorkspaceCarousel: FunctionComponent<WorkspaceCarouselProps> = ({
  workspaces,
}) => {
  // We need a reference to the workspace API to execute calls.
  const workspaceApiRef = useRef(new WorkspaceAPI());
  const workspaceApi = workspaceApiRef.current;

  // We cannot retrieve workspaces if we are not authenticated.
  const { authenticated } = useContext(UserContext);

  const [actualWorkspaces, setWorkspaces] = useState<Workspace[] | null>(null);

  useEffect(() => {
    if (authenticated) {
      (async () => {
        setWorkspaces(await workspaceApi.getCompleteWorkspaces());
      })();
    }
  }, [authenticated, workspaceApi]);

  // The component is only renderable if the user is authenticated or a list of workspaces were specified.
  const needsAuthentication = !authenticated && workspaces === undefined;

  // We need to use this history object to move to a different URL.
  // Specifically, when the add workspace button is clicked, we navigate to the new workspace page.
  const history = useHistory();
  const handleNewWorkspace = () => {
    history.push({
      pathname: "/workspace/new",
    });
  };

  return (
    <section>
      {/*
        Display a header that indicates that the carousel is for workspaces.
        We do not display the search or add buttons unless we are authenticated. 
      */}
      <Row>
        <Column>
          <Heading>
            Workspaces
            <UserIsAuthenticated>
              <IconAddButton onClick={handleNewWorkspace} />
            </UserIsAuthenticated>
          </Heading>
        </Column>
        <Column>
          <Searchbox clearable />
        </Column>
      </Row>

      {/* <div
        style={{
          display: "flex",
          justifyContent: "space-between",
        }}
      >
        <span className="normal-text">
          <h2>Workspaces</h2>
          {authenticated && (
            <button
              style={{
                margin: "0rem 0.5rem",
                width: "1.5rem",
                height: "1.5rem",
                borderRadius: "1.5rem",
                border: "none",
                backgroundColor: "var(--color-fill-element)",
                boxShadow: "var(--shadow-offset)",
                fontWeight: 500,
                fontSize: "1.2rem",
                cursor: "pointer",
              }}
            >
              +
            </button>
          )}
        </span> */}
      {/* {authenticated && ( */}
      {/* // <span */}
      {/* //   style={{ */}
      {/* //     flexBasis: "12rem", */}
      {/* //   }} */}
      {/* // > */}
      {/* //   <TextFieldInput placeholder="Search" /> */}
      {/* // </span> */}
      {/* // )} */}
      {/* // </div> */}

      {/* Display a message indicating that the user should sign in. */}
      {needsAuthentication && <UserNeedsAuthentication />}

      {/* Display a carousel of workspace cards. */}
      {/* {actualWorkspaces && (
        <Carousel>
          {actualWorkspaces.map((workspace) => (
            <WorkspaceCard key={workspace.id} workspace={workspace} />
          ))}
        </Carousel>

        // <ul
        //   style={{
        //     marginTop: "1rem",
        //     display: "grid",
        //     gridTemplateColumns: "repeat(4, 1fr)",
        //     columnGap: "1rem",
        //     listStyle: "none",
        //   }}
        // >
        //   {actualWorkspaces.map((workspace) => (
        //     <li
        //       key={workspace.id}
        //       style={{
        //         display: "block",
        //         padding: "1rem",
        //         width: "100%",
        //         minHeight: "8rem",
        //         backgroundColor: "var(--color-fill-element)",
        //         boxShadow: "var(--shadow-offset)",
        //         borderRadius: "var(--border-radius)",
        //       }}
        //     >
        //       <h3
        //         style={{
        //           fontSize: "1.2rem",
        //         }}
        //       >
        //         <Link to={`/workspace?id=${workspace.id}`}>
        //           {workspace.name}
        //         </Link>
        //       </h3>
        //     </li>
        //   ))}
        // </ul>
      )} */}
    </section>
  );
};

export default WorkspaceCarousel;
