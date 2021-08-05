import { FunctionComponent } from "react";
import { SeparatedText, Title } from "components/text";
import { PageLayout, Wrapper } from "components/layout";
import { AnimatedJumbotron } from "components/jumbotron";
import { WorkspaceCarousel } from "components/workspace";

/** The page users will see when first visiting the website. */
const HomePage: FunctionComponent = () => {
  return (
    <PageLayout header footer>
      {/* Jumbotron goes here with nice animation. */}
      <AnimatedJumbotron>
        <Title>Welcome to Carta!</Title>
        <SeparatedText>
          Carta is a web-based API and application that provides graph-based
          tools for accessing, exploring, and transforming existing datasets and
          models.
        </SeparatedText>
      </AnimatedJumbotron>

      {/* Carousel of workspaces for easy access. */}
      <Wrapper>
        <WorkspaceCarousel />
      </Wrapper>
    </PageLayout>
  );
};

export default HomePage;
