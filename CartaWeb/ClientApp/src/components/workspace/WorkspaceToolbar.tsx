import {
  Dropdown,
  DropdownArea,
  DropdownItem,
  DropdownToggler,
} from "components/dropdown";
import ViewContext from "components/views/ViewContext";
import { GraphData } from "library/api";
import React, { FunctionComponent, useContext } from "react";
import { DatasetAddView, DatasetListView } from ".";
import { WorkspaceToolboxView } from "./views";
import DatasetPropertiesView from "./views/DatasetPropertiesView";
import VisualizerSelectionView from "./views/VisualizerSelectionView";
import WorkflowCreateView from "./views/WorkflowCreateView";

const WorkspaceToolbar: FunctionComponent = () => {
  const { rootId, activeId, actions } = useContext(ViewContext);
  const activeView = activeId === null ? null : actions.getView(activeId);

  const graph: GraphData | undefined = activeView?.tags["graph"];

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "row",
        padding: "0.5rem",
        backgroundColor: "var(--color-fill-element)",
        boxShadow: "var(--shadow)",
      }}
    >
      <Dropdown side="bottom-right">
        <DropdownToggler>View</DropdownToggler>
        <DropdownArea>
          <DropdownItem
            onClick={() => {
              actions.addElementToContainer(rootId, <DatasetListView />);
            }}
          >
            Dataset List
          </DropdownItem>
          <DropdownItem
            onClick={() => {
              actions.addElementToContainer(rootId, <DatasetAddView />);
            }}
          >
            Dataset Import{" "}
          </DropdownItem>
          <DropdownItem
            onClick={() => {
              actions.addElementToContainer(rootId, <DatasetPropertiesView />);
            }}
          >
            Dataset Properties
          </DropdownItem>
          <hr />
          <DropdownItem
            onClick={() => {
              actions.addElementToContainer(rootId, <WorkflowCreateView />);
            }}
          >
            Workflow Create
          </DropdownItem>
          <DropdownItem
            onClick={() => {
              actions.addElementToContainer(rootId, <WorkspaceToolboxView />);
            }}
          >
            Workflow Toolbox
          </DropdownItem>
          <hr />
          <DropdownItem
            onClick={() => {
              actions.addElementToContainer(
                rootId,
                <VisualizerSelectionView />
              );
            }}
          >
            Visualizer Selection
          </DropdownItem>
        </DropdownArea>
      </Dropdown>
      {graph && (
        <React.Fragment>
          <Dropdown side="bottom-right">
            <DropdownToggler>Selection</DropdownToggler>
          </Dropdown>
          <Dropdown side="bottom-right">
            <DropdownToggler>Action</DropdownToggler>
          </Dropdown>
        </React.Fragment>
      )}
    </div>
  );
};

export default WorkspaceToolbar;
