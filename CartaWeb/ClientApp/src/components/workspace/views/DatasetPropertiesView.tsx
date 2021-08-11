// TODO: Add the following
/**
 * 1. Display the workflow (and allow for changing) applied to the dataset.
 */

import { DatasetIcon } from "components/icons";
import { VerticalScroll } from "components/scroll";
import { Tab } from "components/tabs";
import { ViewContext } from "components/views";
import { WorkspaceContext } from "context";
import React, { useContext } from "react";

/** A view that displays the properties of an active dataset in another view. */
const DatasetPropertiesView = () => {
  const { viewId, activeId, actions } = useContext(ViewContext);
  const activeView = activeId === null ? null : actions.getView(activeId);

  // Retrieve the active dataset and its relevant information.
  const { datasets } = useContext(WorkspaceContext);
  const datasetId = activeView?.tags["dataset"];
  // TODO: Better way to find dataset.
  const dataset =
    datasets.value?.find((dataset) => dataset.id === datasetId) ?? null;
  // TODO: Better way to get dataset name.
  const datasetName =
    dataset && (dataset.name ?? `(${dataset.source}/${dataset.resource})`);

  const handleClose = () => {
    actions.removeElement(viewId);
  };

  return (
    <Tab
      title={
        <React.Fragment>
          <DatasetIcon padded />
          {/* Display the name of the currently selected dataset. */}
          {/* If there is no dataset that is currently selected, we render "(None)" in faint text. */}
          {datasetName ?? (
            <span style={{ color: "var(--color-stroke-faint)" }}>(None)</span>
          )}
          &nbsp;{" "}
          <span
            style={{
              color: "var(--color-stroke-faint)",
              fontSize: "var(--font-small)",
            }}
          >
            [Properties]
          </span>
        </React.Fragment>
      }
      closeable
      onClose={handleClose}
    >
      <VerticalScroll>
        {dataset && (
          <div
            style={{
              padding: "1rem",
            }}
          >
            <p>Source: {dataset.source}</p>
            <p>Resource: {dataset.resource}</p>
            {/* TODO: Add workflow selector. */}
          </div>
        )}
      </VerticalScroll>
    </Tab>
  );

  // TODO: SAMPLE CODE
  /**
   *   const [dataset, error]: [WorkspaceDataset | null, Error | null] = datasets.find(datasetId);
   *   return <... />;
   */
};

export default DatasetPropertiesView;
