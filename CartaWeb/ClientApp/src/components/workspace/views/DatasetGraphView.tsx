import React, { FunctionComponent } from "react";
import { GraphIcon } from "components/icons";
import { Tab } from "components/tabs";

interface DatasetGraphViewProps {
  id: string;
}
const DatasetGraphView: FunctionComponent<DatasetGraphViewProps> = ({ id }) => {
  const datasetName = "(source/resource)";
  const modified = false;

  return (
    <Tab
      title={
        <React.Fragment>
          <GraphIcon /> {datasetName}
        </React.Fragment>
      }
      closeable
      status={modified ? "modified" : "unmodified"}
    ></Tab>
  );
};

export default DatasetGraphView;
