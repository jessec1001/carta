import {
  FunctionComponent,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { WorkspaceContext } from "context";
import { GraphData, GraphWorkflow } from "library/api";
import { GraphVisualizer } from "components/visualizations";
import { useViews, Views } from "components/views";
import { GraphIcon } from "components/icons";
import { Loading, Text } from "components/text";

/** The props used for the {@link DatasetGraphView} component. */
interface DatasetGraphViewProps {
  /** The unique identifier of the dataset to graph. */
  id: string;
}

/** A component that renders a graph of a particular dataset. */
const DatasetGraphView: FunctionComponent<DatasetGraphViewProps> = ({ id }) => {
  // Retrieve the dataset information.
  const datasetId = id;
  const dataset: any = null;
  const datasetName = dataset && (dataset.name ?? "UNKNOWN");

  // Construct a reference to the graph data structure when the dataset has loaded correctly.
  const [graph, setGraph] = useState<GraphData | null>(null);
  const { source, resource, workflow } = dataset ?? {};
  useEffect(() => {
    // We only create the graph data when the source/resource exist and are different than previously.
    if (!source || !resource) return;
    setGraph(() => {
      return new GraphData(source, resource, new GraphWorkflow(workflow));
    });
  }, [source, resource, workflow]);

  // TODO: Use modified value.
  // TODO: Use error value.
  // Calculate the current status of this view.
  const modified = false;
  const error = false;

  let status: "error" | "modified" | "unmodified" = "unmodified";
  if (error) status = "error";
  else if (modified) status = "modified";

  // We use the view context to create or remove views from the view container.
  const { viewId, actions } = useViews();
  useEffect(() => {
    actions.setTag(viewId, "dataset", datasetId);
  }, [datasetId, viewId, actions]);
  useEffect(() => {
    if (graph) actions.setTag(viewId, "graph", graph);
    else actions.unsetTag(viewId, "graph");
  }, [graph, viewId, actions]);

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <GraphIcon padded /> {datasetName ?? <Loading />}&nbsp;
        <Text color="muted" size="small">
          [Visualizer]
        </Text>
      </Text>
    );
  }, [datasetName]);

  // TODO: Implement data stored about the active graph vertices.
  return (
    <Views.Container
      title={title}
      closeable
      direction="fill"
      status={status}
      onClick={() => actions.addHistory(viewId)}
    >
      {/* Only render the graph if we have created the data structure. */}
      {graph && <GraphVisualizer graph={graph} />}
    </Views.Container>
  );
};

export default DatasetGraphView;
