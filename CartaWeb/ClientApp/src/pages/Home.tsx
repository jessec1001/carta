import React, { FunctionComponent } from "react";
import { Section, Title } from "components/structure";
import { UserIsNotAuthenticated } from "components/utility";
import { Layout } from "components/layout";

/** The page users will see when first visiting the website. */
const HomePage: FunctionComponent = () => {
  return (
    <Layout navigation footer>
      {/* <AnimatedJumbotron> */}
      <div
        style={{
          padding: "4rem 20%",
          fontSize: "1.4rem",
          backgroundColor: "#ddd",
          textAlign: "center",
        }}
      >
        <Title>Welcome to Carta!</Title>
        <p>
          Carta is a web-based API and application that provides graph-based
          tools for accessing, exploring, and transforming existing datasets and
          models.
        </p>
      </div>
      {/* </AnimatedJumbotron> */}
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
                onClick={() => console.log("Click!")}
              >
                <a>sign in</a>
              </span>{" "}
              or
              <span
                onClick={() => console.log("Click!")}
                style={{
                  color: "var(--color-primary)",
                  cursor: "pointer",
                }}
              >
                {" "}
                <a>sign up</a>
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
