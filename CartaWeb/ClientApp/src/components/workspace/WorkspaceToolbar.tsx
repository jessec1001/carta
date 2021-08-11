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
  const { rootId, actions } = useContext(ViewContext);

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
              actions.addElementToContainer(rootId, <WorkspaceToolboxView />);
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
