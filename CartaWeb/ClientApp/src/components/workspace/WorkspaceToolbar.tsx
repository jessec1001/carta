import {
  Dropdown,
  DropdownArea,
  DropdownItem,
  DropdownToggler,
} from "components/dropdown";
import { LoadingIcon } from "components/icons";
import ViewContext from "components/views/ViewContext";
import { GraphData } from "library/api";
import MetaApi, { MetaTypeEntry } from "library/api/meta";
import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from "react";
import { DatasetAddView, DatasetListView } from ".";
import { WorkspaceToolboxView } from "./views";
import DatasetPropertiesView from "./views/DatasetPropertiesView";
import VisualizerOperationView from "./views/VisualizerOperationView";
import VisualizerSelectionView from "./views/VisualizerSelectionView";
import WorkflowCreateView from "./views/WorkflowCreateView";

// TODO: Load actors and selectors along with the workspace (in workspace wrapper/context).
// TODO: Use <kbd /> tag to specify keyboard shortcuts.
const renderGroupedMenu = (
  groups: Record<string, Record<string, MetaTypeEntry>>,
  type: "actor" | "selector",
  creator: (element: React.ReactElement) => void
) => {
  return Object.entries(groups).map(([group, entries]) => {
    return (
      <React.Fragment key={group}>
        {group !== "" && (
          <div className="toolbar-group-label" key={group}>
            {group}
          </div>
        )}
        {Object.entries(entries).map(([key, entry]) => {
          if (!entry.hidden)
            return (
              <DropdownItem
                key={key}
                onClick={() =>
                  creator(
                    <VisualizerOperationView
                      name={entry.name ?? ""}
                      operation={key}
                      type={type}
                    />
                  )
                }
                // onClick={() => callback(key, false, entry.name)}
              >
                {entry.name}
              </DropdownItem>
            );
          else return null;
        })}
      </React.Fragment>
    );
  });
};

const WorkspaceToolbar: FunctionComponent = () => {
  const { rootId, activeId, actions } = useContext(ViewContext);
  const activeView = activeId === null ? null : actions.getView(activeId);

  const [actors, setActors] = useState<
    Record<string, Record<string, MetaTypeEntry>> | undefined
  >(undefined);
  const [selectors, setSelectors] = useState<
    Record<string, Record<string, MetaTypeEntry>> | undefined
  >(undefined);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      // Fetch the actors and selectors existance data from the server.
      const actors = await MetaApi.getActorsAsync();
      const selectors = await MetaApi.getSelectorsAsync();

      // We will be storing the actors and selectors: first by group, second by key.
      const actorsByGroup: Record<string, Record<string, MetaTypeEntry>> = {};
      const selectorsByGroup: Record<
        string,
        Record<string, MetaTypeEntry>
      > = {};

      // Convert the actors structure to be sorted by group.
      Object.entries(actors).forEach(([key, entry]) => {
        // Check that the group has been created.
        const group = entry.group ?? "";
        if (!(group in actorsByGroup)) actorsByGroup[group] = {};

        // Add the entry to the group.
        actorsByGroup[group][key] = entry;
      });
      // Convert the selectors structure to be sorted by group.
      Object.entries(selectors).forEach(([key, entry]) => {
        // Check that the group has been created.
        const group = entry.group ?? "";
        if (!(group in selectorsByGroup)) selectorsByGroup[group] = {};

        // Add the entry to the group.
        selectorsByGroup[group][key] = entry;
      });

      setActors(actorsByGroup);
      setSelectors(selectorsByGroup);

      setLoading(false);
    })();
  }, []);

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
          <div className="toolbar-group-label">Datasets</div>
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
          <div className="toolbar-group-label">Workflows</div>
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
      {graph && (
        <React.Fragment>
          <Dropdown side="bottom-right">
            <DropdownToggler>
              Selection {loading && <LoadingIcon padded animated />}
            </DropdownToggler>
            {selectors && (
              <DropdownArea>
                {renderGroupedMenu(selectors, "selector", (element) =>
                  actions.addElementToContainer(rootId, element)
                )}
              </DropdownArea>
            )}
          </Dropdown>
          <Dropdown side="bottom-right">
            <DropdownToggler>
              Action {loading && <LoadingIcon padded animated />}
            </DropdownToggler>
            {!loading && <DropdownArea></DropdownArea>}
            {actors && (
              <DropdownArea>
                {renderGroupedMenu(actors, "actor", (element) =>
                  actions.addElementToContainer(rootId, element)
                )}
              </DropdownArea>
            )}
          </Dropdown>
        </React.Fragment>
      )}
    </div>
  );
};

export default WorkspaceToolbar;