// TODO: Add the following
/**
 * 1. Display the workflow (and allow for changing) applied to the dataset.
 */

import { FormGroup } from "components/form";
import { DatasetIcon } from "components/icons";
import WorkflowInput from "components/input/resource/WorkflowInput";
import { VerticalScroll } from "components/scroll";
import { Tab } from "components/tabs";
import { LoadingText } from "components/text";
import { ViewContext } from "components/views";
import { WorkspaceContext } from "context";
import { useAPI } from "hooks";
import { WorkspaceWorkflow } from "library/api";
import React, { useContext, useEffect, useState } from "react";

/** A view that displays the properties of an active dataset in another view. */
const DatasetPropertiesView = () => {
  const { workspaceAPI } = useAPI();
  const { workspace } = useContext(WorkspaceContext);
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

  const [workflows, setWorkflows] = useState<WorkspaceWorkflow[]>([]);
  const [workflow, setWorkflow] = useState<WorkspaceWorkflow | null>(null);
  const [loaded, setLoaded] = useState<boolean>(false);
  useEffect(() => {
    if (dataset && workspace) {
      (async () => {
        if (!dataset.workflow) {
          setWorkflow(null);
        } else {
          const workflow = await workspaceAPI.getWorkspaceWorkflow(
            workspace.id,
            dataset.workflow
          );
          setWorkflow(workflow);
        }

        const workflows = await workspaceAPI.getWorkspaceWorkflows(
          workspace.id
        );
        setWorkflows(workflows);
        setLoaded(true);
      })();
    }
  }, [workspaceAPI, dataset, workspace]);

  const handleClose = () => {
    actions.removeElement(viewId);
  };
  const handleSelectWorkflow = (workflow: WorkspaceWorkflow | null) => {
    if (workspace && dataset) {
      if (workflow === null) {
        datasets.CRUD.update({
          ...dataset,
          workflow: undefined,
          workflowVersion: undefined,
        });
      } else {
        datasets.CRUD.update({
          ...dataset,
          workflow: workflow.id,
          workflowVersion: workflow.versionInformation.number,
        });
      }
    }
    setWorkflow(workflow);
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
            <p style={{ display: "flex" }}>
              Source
              <span
                style={{
                  flexGrow: 1,
                  textAlign: "right",
                  color: "var(--color-stroke-lowlight)",
                }}
              >
                {dataset.source}
              </span>
            </p>
            <p style={{ display: "flex" }}>
              Resource
              <span
                style={{
                  flexGrow: 1,
                  textAlign: "right",
                  color: "var(--color-stroke-lowlight)",
                }}
              >
                {dataset.resource}
              </span>
            </p>
            {/* TODO: Add workflow selector. */}
            {loaded && (
              <FormGroup title="Workflow" density="flow">
                <WorkflowInput
                  value={workflow}
                  onChange={handleSelectWorkflow}
                  workflows={workflows}
                />
              </FormGroup>
            )}
            {!loaded && (
              <p style={{ display: "flex" }}>
                Workflow
                <span
                  style={{
                    flexGrow: 1,
                    textAlign: "right",
                    color: "var(--color-stroke-lowlight)",
                  }}
                >
                  <LoadingText />
                </span>
              </p>
            )}
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
