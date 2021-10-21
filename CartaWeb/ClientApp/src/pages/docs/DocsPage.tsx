import { FunctionComponent } from "react";
import { GraphingDocs } from "./GraphingDocs";
import { DataFormatsDocs } from "./DataFormatsDocs";
import { ApiDocs } from "./ApiDocs";
import { Text, Title } from "components/text";
import { useParams } from "react-router-dom";
import { Mainbar, Sidebar, SidebarLayout } from "components/ui/layout";
import { Link } from "components/common";

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
      contents = <Text>Please select a documentation topic.</Text>;
      break;
  }

  // Return the correct contents based on the topic.
  return (
    <div>
      <SidebarLayout side="left">
        <Sidebar>
          <Link to={`/docs/${Topic.Graphing}`}>Graphing</Link>
          <Link to={`/docs/${Topic.DataFormat}`}>Data Format</Link>
          <Link to={`/docs/${Topic.Api}`}>API</Link>
        </Sidebar>
        <Mainbar>
          <Title>Documentation</Title>
          {contents}
        </Mainbar>
      </SidebarLayout>
    </div>
  );
};

export default DocsPage;
