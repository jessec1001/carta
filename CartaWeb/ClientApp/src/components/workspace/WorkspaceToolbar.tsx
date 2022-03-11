import { FunctionComponent } from "react";
import { Text } from "components/text";
import { useWorkspace } from "./WorkspaceContext";

const WorkspaceToolbar: FunctionComponent = () => {
  const { workspace } = useWorkspace();

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "row",
        // padding: "0.5rem",
        backgroundColor: "var(--color-fill-element)",
        boxShadow: "var(--shadow)",
      }}
    >
      <div
        style={{
          display: "flex",
          flexDirection: "row",
        }}
      >
        {/* <Dropdown side="bottom-right">
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
        </Dropdown> */}
      </div>
      {workspace && (
        <div
          style={{
            display: "flex",
            flexGrow: 1,
            flexDirection: "row-reverse",
            // justifyContent: "space-between",
            padding: "0.5rem 0.5rem 0rem 0.5rem",
          }}
        >
          <Text size="normal" justify="right">
            {workspace.name}
          </Text>
          {/* TODO: Implement settings menu. */}
          {/* <Text size="normal">
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
          </Text> */}
        </div>
      )}
    </div>
  );
};

export default WorkspaceToolbar;
