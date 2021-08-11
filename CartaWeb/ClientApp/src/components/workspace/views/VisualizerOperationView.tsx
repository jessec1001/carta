import { SchemaForm } from "components/form/schema";
import { WorkflowIcon } from "components/icons";
import { VerticalScroll } from "components/scroll";
import { Tab } from "components/tabs";
import { ErrorText, LoadingText } from "components/text";
import { ViewContext } from "components/views";
import { GraphData } from "library/api";
import MetaApi, { MetaTypeEntry } from "library/api/meta";
import { JsonSchema } from "library/schema";
import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from "react";

interface VisualizerOperationViewProps {
  name: string;
  operation: string;
  type: "actor" | "selector";
}

const VisualizerOperationView: FunctionComponent<VisualizerOperationViewProps> =
  ({ name, operation, type }) => {
    const { viewId, activeId, actions } = useContext(ViewContext);
    const activeView = activeId === null ? null : actions.getView(activeId);

    const graph: GraphData | undefined = activeView?.tags["graph"];

    const [error, setError] = useState<Error | null>(null);

    const handleClose = () => {
      actions.removeElement(viewId);
    };
    const handleSubmit = () => {
      console.log(graph);
      if (graph) {
        console.log(graph.workflow);
        if (!graph.workflow._id) {
          setError(
            new Error("Dataset does not have a workflow applied to it.")
          );
        } else {
          if (type === "actor") {
            graph.workflow.applyAction({ ...value, type: operation });
          }
          if (type === "selector") {
            graph.workflow.applySelector({ ...value, type: operation });
          }
        }
      }
    };

    const [value, setValue] = useState<any | undefined>(undefined);
    const [schema, setSchema] = useState<JsonSchema | undefined>(undefined);
    const [loaded, setLoaded] = useState(false);
    useEffect(() => {
      (async () => {
        if (type === "actor") {
          const defaultValue = await MetaApi.getActorDefaultAsync({
            actor: operation,
          });
          const schema = await MetaApi.getActorSchemaAsync({
            actor: operation,
          });

          delete defaultValue.type;

          setValue(defaultValue);
          setSchema(schema);
        }
        if (type === "selector") {
          const defaultValue = await MetaApi.getSelectorDefaultAsync({
            selector: operation,
          });
          const schema = await MetaApi.getSelectorSchemaAsync({
            selector: operation,
          });

          delete defaultValue.type;

          setValue(defaultValue);
          setSchema(schema);
        }

        setLoaded(true);
      })();
    }, [operation, type]);

    return (
      <Tab
        title={
          <React.Fragment>
            <WorkflowIcon padded /> {name}
          </React.Fragment>
        }
        closeable
        onClose={handleClose}
      >
        <VerticalScroll>
          <div style={{ padding: "1rem" }}>
            {!loaded && <LoadingText />}
            {graph && schema && value && (
              <SchemaForm
                schema={schema}
                value={value}
                onChange={setValue}
                onSubmit={handleSubmit}
              />
            )}
            <ErrorText>{error?.message}</ErrorText>
          </div>
        </VerticalScroll>
      </Tab>
    );
  };

export default VisualizerOperationView;
