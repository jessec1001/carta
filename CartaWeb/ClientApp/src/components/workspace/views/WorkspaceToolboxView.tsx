import { IconAddButton } from "components/buttons";
import { WorkflowIcon } from "components/icons";
import { SearchboxInput } from "components/input";
import { VerticalScroll } from "components/scroll";
import { Column, Row } from "components/structure";
import { Tab, TabContainer } from "components/tabs";
import ViewContext from "components/views/ViewContext";
import { WorkspaceContext } from "context";
import { useAPI } from "hooks";
import { WorkspaceWorkflow } from "library/api";
import { ObjectFilter } from "library/search";
import React, { useContext, useEffect, useState } from "react";
import WorkflowCreateView from "./WorkflowCreateView";

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
        flexShrink: 0,
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
  const [query, setQuery] = useState<string>("");

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
  const handleAdd = () => {
    const parentView = actions.getParentView(viewId);
    if (parentView) {
      actions.addChildElement(parentView.currentId, <WorkflowCreateView />);
    }
  };

  const workflowFilter = new ObjectFilter(query, {});

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
            <Row>
              <Column>
                <SearchboxInput value={query} onChange={setQuery} clearable />
              </Column>
              <IconAddButton onClick={handleAdd} />
            </Row>
            <div
              style={{
                marginTop: "1rem",
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
                  {workflowFilter
                    .filter(workflows)
                    .map((workflow) => renderWorkflow(workflow))}
                </ul>
              )}
            </div>
          </div>
        </VerticalScroll>
      </Tab>
    </TabContainer>
  );
};

export default WorkspaceToolboxView;
