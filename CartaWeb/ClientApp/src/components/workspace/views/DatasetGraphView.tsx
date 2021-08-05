import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { GraphIcon } from "components/icons";
import { Tab } from "components/tabs";
import { WorkspaceContext } from "context";
import { GraphData, WorkspaceDataset } from "library/api";
import { GraphVisualizer } from "components/visualizations";
import { GraphWorkflow } from "library/api/workflow";

/** The props used for the {@link DatasetGraphView} component. */
interface DatasetGraphViewProps {
  /** The unique identifier of the dataset to graph. */
  id: string;
}

/** A component that renders a graph of a particular dataset. */
const DatasetGraphView: FunctionComponent<DatasetGraphViewProps> = ({ id }) => {
  const { datasets } = useContext(WorkspaceContext);

  // TODO: Simplify retrieving a dataset especially if it already exists.
  const [dataset, setDataset] = useState<WorkspaceDataset | null>(null);

  // TODO: Replace usage of old graph data structure class with new data structure.
  const graphDataRef = useRef<GraphData | null>(null);

  useEffect(() => {
    if (dataset === null && datasets.value !== null) {
      const dataset = datasets.value.find((dataset) => dataset.id === id);
      if (dataset) {
        setDataset(dataset);
        graphDataRef.current = new GraphData(
          dataset.source,
          dataset.resource,
          new GraphWorkflow(undefined)
        );
      }
    }
  }, [id, dataset, datasets]);

  // TODO: Simplify getting default display name for datasets.
  const datasetName =
    dataset === null
      ? "Loading"
      : dataset.name ?? `(${dataset.source}/${dataset.resource})`;
  const modified = false;

  console.log(graphDataRef.current);
  return (
    <Tab
      title={
        <React.Fragment>
          <GraphIcon padded /> {datasetName}
        </React.Fragment>
      }
      closeable
      status={modified ? "modified" : "unmodified"}
    >
      {graphDataRef.current && <GraphVisualizer graph={graphDataRef.current} />}
    </Tab>
  );
};

export default DatasetGraphView;
