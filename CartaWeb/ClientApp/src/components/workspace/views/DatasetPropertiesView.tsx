// TODO: Add the following
/**
 * 1. Display the source and resource of the dataset.
 * 2. Display the workflow (and allow for changing) applied to the dataset.
 */

import { FormGroup } from "components/form";
import { DatasetIcon } from "components/icons";
import { VerticalScroll } from "components/scroll";
import { Tab } from "components/tabs";
import { ViewContext } from "components/views";
import { WorkspaceContext } from "context";
import React, { useContext } from "react";

/** A view that displays the properties of an active dataset in another view. */
const DatasetPropertiesView = () => {
  const { activeId, actions } = useContext(ViewContext);
  const activeView = activeId === null ? null : actions.getView(activeId);

  const { datasets } = useContext(WorkspaceContext);
  const datasetId = activeView?.tags["dataset"];
  const dataset =
    datasets.value?.find((dataset) => dataset.id === datasetId) ?? null;
  const datasetName =
    dataset && (dataset.name ?? `(${dataset.source}/${dataset.resource})`);

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
          &nbsp; [Properties]
        </React.Fragment>
      }
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
