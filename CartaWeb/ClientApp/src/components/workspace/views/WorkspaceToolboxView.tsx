import { WorkflowIcon } from "components/icons";
import { VerticalScroll } from "components/scroll";
import { Tab, TabContainer } from "components/tabs";
import ViewContext from "components/views/ViewContext";
import { WorkspaceContext } from "context";
import { useAPI } from "hooks";
import { WorkspaceWorkflow } from "library/api";
import React, { useContext, useEffect, useState } from "react";

const renderWorkflow = (workflow: WorkspaceWorkflow) => {
  return (
    <div
      className="workflow-card"
      style={{
        fontSize: "0.8em",
        display: "flex",
        flexDirection: "column",
        width: "8rem",
        height: "8rem",
        backgroundColor: "var(--color-fill-element)",
        borderRadius: "var(--border-radius)",
        boxShadow: "var(--shadow)",
        padding: "1rem",
        overflow: "hidden",
      }}
    >
      <span
        style={{
          fontSize: "2em",
          width: "100%",
          textAlign: "center",
          display: "block",
        }}
      >
        <WorkflowIcon />
      </span>
      <span
        style={{
          flexGrow: 1,
          display: "block",
          textAlign: "center",
          overflow: "hidden",
          textOverflow: "ellipsis",
          // lineClamp: 2,
        }}
      >
        {workflow.name}
      </span>
    </div>
  );
};

const WorkspaceToolboxView = () => {
  const { workspaceAPI } = useAPI();
  const { viewId, actions } = useContext(ViewContext);
  const { workspace } = useContext(WorkspaceContext);

  const [workflows, setWorkflows] = useState<WorkspaceWorkflow[] | null>(null);

  useEffect(() => {
    if (workspace !== null) {
      (async () => {
        const workflows = await workspaceAPI.getWorkspaceWorkflows(
          workspace.id
        );
        setWorkflows(workflows);
      })();
    }
  }, [workspace, workspaceAPI]);

  const handleClose = () => {
    actions.removeChildElement(viewId);
  };

  console.log(workflows);
  return (
    <TabContainer>
      <Tab
        title={
          <React.Fragment>
            <WorkflowIcon padded /> Toolbox
          </React.Fragment>
        }
        onClose={handleClose}
        closeable
      >
        <VerticalScroll>
          <div
            style={{
              padding: "1rem",
            }}
          >
            {!workflows && <span>Loading</span>}
            {workflows && (
              <ul
                role="presentation"
                style={{
                  display: "flex",
                  flexDirection: "row",
                  flexWrap: "wrap",
                  gap: "1rem",
                }}
              >
                {workflows.map((workflow) => renderWorkflow(workflow))}
              </ul>
            )}
          </div>
        </VerticalScroll>
      </Tab>
    </TabContainer>
  );
};

export default WorkspaceToolboxView;
