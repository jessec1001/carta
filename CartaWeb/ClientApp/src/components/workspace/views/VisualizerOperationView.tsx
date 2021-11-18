import { SchemaForm } from "components/form/schema";
import { VerticalScroll } from "components/scroll";
import { Text, Loading } from "components/text";
import { useViews } from "components/views";
import { GraphData } from "library/api";
import MetaApi from "library/api/meta";
import { JsonSchema } from "library/schema";
import { FunctionComponent, useEffect, useState } from "react";

interface VisualizerOperationViewProps {
  name: string;
  operation: string;
  type: "actor" | "selector";
}

const VisualizerOperationView: FunctionComponent<VisualizerOperationViewProps> =
  ({ name, operation, type }) => {
    const { viewId, actions } = useViews();

    const graph: GraphData | undefined = actions.getActiveTag("graph");

    const [error, setError] = useState<Error | null>(null);

    const handleClose = () => {
      actions.removeView(viewId);
    };
    const handleSubmit = () => {
      if (graph) {
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
      // <Tabs.Tab
      //   id={0}
      //   title={
      //     <React.Fragment>
      //       <WorkflowIcon padded /> {name}
      //     </React.Fragment>
      //   }
      //   closeable
      //   onClose={handleClose}
      // >
      <VerticalScroll>
        <div style={{ padding: "1rem" }}>
          {!loaded && <Loading />}
          {graph && schema && value && (
            <SchemaForm
              schema={schema}
              value={value}
              onChange={setValue}
              onSubmit={handleSubmit}
            />
          )}
          <Text color="error">{error?.message}</Text>
        </div>
      </VerticalScroll>
      // </Tabs.Tab>
    );
  };

export default VisualizerOperationView;
