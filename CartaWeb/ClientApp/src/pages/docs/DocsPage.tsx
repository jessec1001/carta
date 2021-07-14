import { FunctionComponent } from "react";
import { Container } from "reactstrap";
import { GraphingDocs } from "./GraphingDocs";
import { DataFormatsDocs } from "./DataFormatsDocs";
import { ApiDocs } from "./ApiDocs";
import { IndentList, Title } from "components/structure";
import { useParams } from "react-router-dom";
import { Mainbar, Sidebar, SidebarLayout } from "components/ui/layout";
import { Link } from "components/ui/common/link";

/** The type of topic parameter used for the {@link DocsPage}. */
enum Topic {
  Graphing = "graphing",
  DataFormat = "data-format",
  Api = "api",
}

/** The page containing documentation which is split into multiple topics. */
const DocsPage: FunctionComponent = ({ children }) => {
  // Get the topic for the documentation.
  const { topic } = useParams<{ topic: Topic }>();

  // Determine the contents of the documentation page from its topic parameter.
  let contents;
  switch (topic) {
    case Topic.Graphing:
      contents = <GraphingDocs />;
      break;
    case Topic.DataFormat:
      contents = <DataFormatsDocs />;
      break;
    case Topic.Api:
      contents = <ApiDocs />;
      break;
    default:
      contents = <p>Please select a documentation topic.</p>;
      break;
  }

  // Return the correct contents based on the topic.
  return (
    <Container>
      <SidebarLayout side="left">
        <Sidebar>
          <IndentList>
            <Link href={`/docs/${Topic.Graphing}`}>Graphing</Link>
            <Link href={`/docs/${Topic.DataFormat}`}>Data Format</Link>
            <Link href={`/docs/${Topic.Api}`}>API</Link>
          </IndentList>
        </Sidebar>
        <Mainbar>
          <Title>Documentation</Title>
          {contents}
        </Mainbar>
      </SidebarLayout>
    </Container>
  );
};

export default DocsPage;
