import React from "react";
import { GraphIcon } from "components/icons";
import { Tab } from "components/tabs";

const DatasetGraphView = () => {
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
