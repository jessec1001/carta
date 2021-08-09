import { SplitArea } from "components/containers";
import { PageLayout } from "components/layout";
import { FunctionComponent } from "react";

const TestPage: FunctionComponent = () => {
  return (
    <PageLayout header footer>
      <SplitArea direction="vertical">
        <div>Area Alpha</div>
        <div>Area Beta</div>
        <SplitArea direction="horizontal">
          <div>Area A</div>
          <div>Area B</div>
          <div>Area C</div>
        </SplitArea>
      </SplitArea>
    </PageLayout>
  );
};

export default TestPage;
