import { FunctionComponent } from "react";
import {
  Dropdown,
  DropdownArea,
  DropdownItem,
  DropdownToggler,
} from "components/dropdown";
import { Text } from "components/text";
import { useViews } from "components/views";
import { useWorkspace } from "./WorkspaceContext";
import { WorkspaceSettingsView } from "pages/workspace/views";
import VisualizerSelectionView from "pages/workspace/views/VisualizerSelectionView";
import SettingsIcon from "components/icons/SettingsIcon";

const WorkspaceToolbar: FunctionComponent = () => {
  const { rootId, actions } = useViews();
  const { workspace } = useWorkspace();

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        // padding: "0.5rem",
        backgroundColor: "var(--color-fill-element)",
        boxShadow: "var(--shadow)",
      }}
    >
      {workspace && (
        <div
          style={{
            display: "flex",
            flexDirection: "row",
            alignItems: "center",
            justifyContent: "space-between",
            padding: "0.5rem 0.5rem 0rem 0.5rem",
          }}
        >
          <Text size="medium">{workspace.name}</Text>
          <Text size="normal">
            <span
              style={{
                cursor: "pointer",
              }}
              onClick={() => {
                actions.addElementToContainer(
                  rootId,
                  <WorkspaceSettingsView />
                );
              }}
            >
              <SettingsIcon />
            </span>
          </Text>
        </div>
      )}
      <div
        style={{
          display: "flex",
          flexDirection: "row",
        }}
      >
        <Dropdown side="bottom-right">
          <DropdownToggler>View</DropdownToggler>
          <DropdownArea>
            <div className="toolbar-group-label">Workflows</div>
            <div className="toolbar-group-label">Visualizers</div>
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
      </div>
    </div>
  );
};

export default WorkspaceToolbar;
