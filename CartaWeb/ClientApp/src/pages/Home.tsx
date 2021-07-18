import { FunctionComponent } from "react";
import { Section, Title } from "components/structure";
import { UserIsNotAuthenticated, UserSignIn } from "components/utility";
import { Layout } from "components/layout";
import { AnimatedJumbotron } from "components/jumbotron";

/** The page users will see when first visiting the website. */
const HomePage: FunctionComponent = () => {
  return (
    <Layout navigation footer>
      {/* Jumbotron goes with nice animation. */}
      <AnimatedJumbotron>
        <Title>Welcome to Carta!</Title>
        <p>
          Carta is a web-based API and application that provides graph-based
          tools for accessing, exploring, and transforming existing datasets and
          models.
        </p>
      </AnimatedJumbotron>

      <div
        style={{
          padding: "2rem 4rem",
        }}
      >
        <Section title="Workspaces">
          <UserIsNotAuthenticated>
            <p>
              You must{" "}
              <span
                style={{
                  color: "var(--color-primary)",
                  cursor: "pointer",
                }}
              >
                <UserSignIn>sign in</UserSignIn>
              </span>{" "}
              or
              <span
                style={{
                  color: "var(--color-primary)",
                  cursor: "pointer",
                }}
              >
                {" "}
                <UserSignIn>sign up</UserSignIn>
              </span>{" "}
              to use this functionality.
            </p>
          </UserIsNotAuthenticated>
        </Section>
      </div>
    </Layout>
  );
};

export default HomePage;
