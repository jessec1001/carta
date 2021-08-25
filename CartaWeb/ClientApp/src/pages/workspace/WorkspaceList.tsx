import { PageLayout, Wrapper } from "components/layout";
import { Title } from "components/text";
import { FunctionComponent } from "react";

/** The page that lists all workspaces in a more concise way and allows for archiving or unarchiving. */
const WorkspaceListPage: FunctionComponent = () => {
  return (
    <PageLayout header footer>
      <Wrapper>
        <Title>Workspaces</Title>
      </Wrapper>
    </PageLayout>
  );
};

export default WorkspaceListPage;
