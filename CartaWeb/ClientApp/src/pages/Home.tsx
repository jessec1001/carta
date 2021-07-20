import { FunctionComponent, useEffect, useRef, useState } from "react";
import { Paragraph, Section, Title } from "components/structure";
import {
  UserIsAuthenticated,
  UserIsNotAuthenticated,
  UserSignIn,
} from "components/utility";
import { Layout, Wrapper } from "components/layout";
import { AnimatedJumbotron } from "components/jumbotron";
import { Link } from "components/common";
import { Workspace, WorkspaceAPI } from "library/api";
import { TextFieldInput } from "components/input";

/** The page users will see when first visiting the website. */
const HomePage: FunctionComponent = () => {
  // We need a reference to the workspace API to execute calls.
  const workspaceApiRef = useRef(new WorkspaceAPI());
  const workspaceApi = workspaceApiRef.current;

  const [workspaces, setWorkspaces] = useState<Workspace[] | null>(null);

  useEffect(() => {
    (async () => {
      setWorkspaces(await workspaceApi.getCompleteWorkspaces());
    })();
  }, [workspaceApi]);

  return (
    <Layout header footer>
      {/* Jumbotron goes with nice animation. */}
      <AnimatedJumbotron>
        <Title>Welcome to Carta!</Title>
        <Paragraph>
          Carta is a web-based API and application that provides graph-based
          tools for accessing, exploring, and transforming existing datasets and
          models.
        </Paragraph>
      </AnimatedJumbotron>

      <Wrapper>
        <section>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
            }}
          >
            <span className="normal-text">
              <h2>Workspaces</h2>
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
            </span>
            <span
              style={{
                flexBasis: "12rem",
              }}
            >
              <TextFieldInput placeholder="Search" />
            </span>
          </div>
          <UserIsNotAuthenticated>
            <Paragraph>
              You must{" "}
              <Link to="#">
                <UserSignIn>sign in</UserSignIn>
              </Link>{" "}
              or{" "}
              <Link to="#">
                <UserSignIn>sign up</UserSignIn>
              </Link>{" "}
              to use this functionality.
            </Paragraph>
          </UserIsNotAuthenticated>
          <UserIsAuthenticated>
            {workspaces && (
              <ul
                style={{
                  marginTop: "1rem",
                  display: "grid",
                  gridTemplateColumns: "repeat(4, 1fr)",
                  columnGap: "0.5rem",
                  listStyle: "none",
                }}
              >
                {workspaces.map((workspace) => (
                  <li
                    style={{
                      display: "block",
                      padding: "1rem",
                      width: "100%",
                      minHeight: "8rem",
                      backgroundColor: "var(--color-fill-element)",
                      boxShadow: "var(--shadow-offset)",
                      borderRadius: "var(--border-radius)",
                    }}
                  >
                    <h3
                      style={{
                        fontSize: "1.2rem",
                      }}
                    >
                      <Link to={`/workspace/${workspace.id}`}>
                        {workspace.name}
                      </Link>
                    </h3>
                  </li>
                ))}
              </ul>
            )}
          </UserIsAuthenticated>
        </section>
      </Wrapper>
    </Layout>
  );
};

export default HomePage;
