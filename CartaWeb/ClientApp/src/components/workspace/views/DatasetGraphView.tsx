import { FunctionComponent, useContext, useEffect, useState } from "react";
import { WorkspaceContext } from "context";
import { GraphData } from "library/api";
import { GraphVisualizer } from "components/visualizations";
import { GraphWorkflow } from "library/api/workflow";
import ViewContext from "components/views/ViewContext";

/** The props used for the {@link DatasetGraphView} component. */
interface DatasetGraphViewProps {
  /** The unique identifier of the dataset to graph. */
  id: string;
}

/** A component that renders a graph of a particular dataset. */
const DatasetGraphView: FunctionComponent<DatasetGraphViewProps> = ({ id }) => {
  // Retrieve the specified dataset and its relevant information.
  const { datasets } = useContext(WorkspaceContext);
  const datasetId = id;
  // TODO: Better way to find dataset.
  const dataset =
    datasets.value?.find((dataset) => dataset.id === datasetId) ?? null;
  // TODO: Better way to get dataset name.
  const datasetName =
    dataset && (dataset.name ?? `(${dataset.source}/${dataset.resource})`);

  // Construct a reference to the graph data structure when the dataset has loaded correctly.
  // TODO: Replace usage of old graph data structure class with new data structure.
  const [graph, setGraph] = useState<GraphData | null>(null);
  const { source, resource, workflow } = dataset ?? {};
  useEffect(() => {
    // We only create the graph data when the source/resource exist and are different than previously.
    if (!source || !resource) return;
    setGraph((graph) => {
      return new GraphData(source, resource, new GraphWorkflow(workflow));
    });
  }, [source, resource, workflow]);

  // TODO: Use modified value.
  // TODO: Use error value.
  const modified = false;
  const error = false;
  const status = error ? "error" : modified ? "modified" : "unmodified";

  // We use the view context to create or remove views from the view container.
  const { viewId, actions } = useContext(ViewContext);
  const handleClose = () => {
    // Destroy this view.
    actions.removeElement(viewId);
  };
  useEffect(() => {
    actions.setTag(viewId, "dataset", datasetId);
  }, [datasetId, viewId, actions]);
  useEffect(() => {
    if (graph) actions.setTag(viewId, "graph", graph);
    else actions.unsetTag(viewId, "graph");
  }, [graph, viewId, actions]);

  return (
    // Render the view itself within a tab so it can be easily added to container views.
    // <Tabs.Tab
    //   id={0}
    //   title={
    //     <React.Fragment>
    //       <GraphIcon padded />
    //       {datasetName ?? <Loading />}
    //       &nbsp;
    //       <span
    //         style={{
    //           color: "var(--color-stroke-muted)",
    //           fontSize: "var(--font-small)",
    //         }}
    //       >
    //         [Visualizer]
    //       </span>
    //     </React.Fragment>
    //   }
    //   status={status}
    //   closeable
    //   onClose={handleClose}
    // >
    <div
      style={{
        width: "100%",
        height: "100%",
      }}
      onClick={() => {
        actions.setActiveView(viewId);
      }}
    >
      {/* Only render the graph if we have created the data structure. */}
      {graph && <GraphVisualizer graph={graph} />}
    </div>
    // </Tabs.Tab>
  );
};

export default DatasetGraphView;
