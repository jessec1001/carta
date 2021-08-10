import {
  Dropdown,
  DropdownArea,
  DropdownItem,
  DropdownToggler,
} from "components/dropdown";
import ViewContext from "components/views/ViewContext";
import { FunctionComponent, useContext } from "react";
import { DatasetAddView, DatasetListView } from ".";
import { WorkspaceToolboxView } from "./views";
import DatasetPropertiesView from "./views/DatasetPropertiesView";

const WorkspaceToolbar: FunctionComponent = () => {
  const { viewId, actions } = useContext(ViewContext);

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
              actions.addChildElement(viewId, <DatasetListView />);
            }}
          >
            Dataset List
          </DropdownItem>
          <DropdownItem
            onClick={() => {
              actions.addChildElement(viewId, <DatasetAddView />);
            }}
          >
            Dataset Import{" "}
          </DropdownItem>
          <DropdownItem
            onClick={() => {
              actions.addChildElement(viewId, <DatasetPropertiesView />);
            }}
          >
            Dataset Properties
          </DropdownItem>
          <hr />
          <DropdownItem
            onClick={() => {
              actions.addChildElement(viewId, <WorkspaceToolboxView />);
            }}
          >
            Toolbox
          </DropdownItem>
        </DropdownArea>
      </Dropdown>
    </div>
  );
};

export default WorkspaceToolbar;
