import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import classNames from "classnames";
import { WorkspaceContext } from "context";
import { defaultWorkspaceDatasetName, WorkspaceDataset } from "library/api";
import { ObjectFilter } from "library/search";
import { Text, Loading } from "components/text";
import { IconButtonAdd } from "components/buttons";
import { DatabaseIcon, DatasetIcon } from "components/icons";
import { SearchboxInput, TextFieldInput } from "components/input";
import { Column, Row } from "components/structure";
import { useViews, Views } from "components/views";
import DatasetAddView from "./DatasetAddView";
import DatasetGraphView from "./DatasetGraphView";

import "./DatasetListView.css";

/**
 * Renders a dataset item that can be selected and renamed..
 * @param dataset The dataset to render.
 * @param setSelected A function to set the selected dataset.
 * @param setName A function to set the name of the dataset.
 * @param selected Whether this dataset is currently selected.
 * @param rename Whether this dataset is currently being renamed and what it is being renamed to.
 * @returns An element that represents a dataset item.
 */
const renderDataset = (
  dataset: WorkspaceDataset,
  setSelected: (event: React.MouseEvent) => void,
  setName: (value: string) => void,
  selected: boolean,
  rename?: string
) => {
  // Compute the default display name for the dataset.
  const displayName = dataset.name ?? defaultWorkspaceDatasetName(dataset);

  return (
    <li
      className={classNames("DatasetListView-DatasetItem", { selected })}
      key={dataset.id}
      onClick={setSelected}
    >
      <Text align="middle">
        <DatasetIcon padded />
        {rename !== undefined && selected ? (
          <TextFieldInput
            className={"DatasetListView-DatasetItem-Input"}
            value={rename}
            onChange={setName}
            placeholder={displayName}
          />
        ) : (
          displayName
        )}
      </Text>
    </li>
  );
};

/** A component that renders a list of datasets that can be searched and sorted. */
const DatasetListView: FunctionComponent = () => {
  // We use these contexts to handle opening and closing views and managing data.
  const { viewId, rootId, actions } = useViews();
  const { datasets } = useContext(WorkspaceContext);
  const elementRef = useRef<HTMLDivElement>(null);

  // We use a state variable to indicate which item of the dataset list is currently selected.
  // We use a state variable to indicate whether the currently selected dataset item is being renamed and the new name.
  // By selecting a dataset, we indicate that we are preparing to rename the dataset.
  const [selected, setSelected] = useState<string | null>(null);
  const [renaming, setRenaming] = useState<boolean>(false);
  const [name, setName] = useState<string>("");

  // To aid in the selection and renaming process, we need some references to some of the raw HTML elements.
  // Namely, we need a reference to the list of all elements and to the currently selected element.
  const datasetListRef = useRef<HTMLUListElement>(null);

  // We setup a query that can be changed by the filter box.
  // This filter uses the more complex object filter which allows for complex searches to be made.
  const [query, setQuery] = useState<string>("");
  const datasetFilter = new ObjectFilter(query, {});

  // We create these event handlers to handle when views should be modified.
  // Clicking the add button should open the add dataset view.
  // Clicking the close tab button should close the list datasets view.
  // Executing a dataset item should open the graph dataset view to that dataset.
  const handleAddDataset = useCallback(() => {
    actions.addElementToContainer(rootId, <DatasetAddView />);
  }, [actions, rootId]);
  const handleOpenDataset = useCallback(
    (datasetId: string) => {
      actions.addElementToContainer(
        rootId,
        <DatasetGraphView id={datasetId} />
      );
    },
    [actions, rootId]
  );

  /**
   * Dataset items should be able to be renamed.
   *
   * This occurs when the user single clicks on an already selected dataset item.
   * This should replace the typical dataset list item with a text input.
   *
   * To finalize the renaming:
   * - Clicking elsewhere (not on the selected dataset) stops the renaming and submits it.
   * - Pressing 'ENTER' stops the renaming and submits it.
   * - Pressing 'ESCAPE' stops the renaming and cancels it.
   */

  /**
   * Dataset items should be able to be opened.
   *
   * This occurs when the user double clicks on a dataset item.
   * This should simply open up the dataset in a new view with a graph visualizer.
   */

  // This handles the logic of actually submitting a renaming update.
  const handleRename = useCallback(() => {
    // Try to find the selected dataset within the datasets collection.
    const dataset = datasets.value?.find((dataset) => dataset.id === selected);
    if (dataset) {
      // Perform the actual update.
      datasets.CRUD.update({
        ...dataset,
        name: name,
      });
    }
  }, [datasets, name, selected]);
  // This handles the logic of deleting a dataset.
  const handleDelete = useCallback(() => {
    const dataset = datasets.value?.find((dataset) => dataset.id === selected);
    if (dataset) {
      datasets.CRUD.remove(dataset);
      setSelected(null);
    }
  }, [datasets, selected]);

  // This handles the logic of selecting a particular dataset item.
  const handleSelect = useCallback(
    (dataset: WorkspaceDataset, event: React.MouseEvent) => {
      // This corresponds to a single click.
      if (event.detail === 1) {
        if (selected === dataset.id) {
          // If the current selected element was clicked, start renaming.
          // TODO: Default dataset names?
          const name = dataset.name ?? defaultWorkspaceDatasetName(dataset);
          if (!renaming) {
            setRenaming(true);
            setName(name);
          }
        } else {
          // If there was a selection being renamed, submit renaming.
          if (renaming) {
            setRenaming(false);
            handleRename();
          }
        }
        setSelected(dataset.id);
      }

      // This corresponds to a double click.
      if (event.detail === 2 && !renaming) {
        handleOpenDataset(dataset.id);
      }
    },
    [handleOpenDataset, handleRename, selected, renaming]
  );

  // This removes the selection when a click is made outside of the dataset list.
  useEffect(() => {
    if (datasetListRef.current) {
      const handlePotentialOutsideClick = (event: MouseEvent) => {
        if (!datasetListRef.current?.contains(event.target as Element)) {
          if (renaming) {
            // This submits renaming.
            setRenaming(false);
            handleRename();
          }
          setSelected(null);
        }
      };

      // Setup and teardown.
      if (elementRef.current) {
        const element = elementRef.current;
        element.addEventListener("click", handlePotentialOutsideClick);
        return () => {
          element.removeEventListener("click", handlePotentialOutsideClick);
        };
      }
    }
  }, [handleRename, renaming]);

  // This allows the user to use keyboard shortcuts to perform or cancel an operation.
  useEffect(() => {
    const handlePotentialKey = (event: KeyboardEvent) => {
      if (event.code === "Enter") {
        // We submit the renaming if currently happening.
        if (renaming) {
          setRenaming(false);
          handleRename();
        }
      }
      if (event.code === "Escape") {
        // We simply cancel the rename request here.
        setRenaming(false);
      }
      if (event.code === "Delete") {
        // We delete the focussed element if not renaming.
        if (!renaming) {
          handleDelete();
        }
      }
    };

    // Setup and teardown.
    window.addEventListener("keydown", handlePotentialKey);
    return () => window.removeEventListener("keydown", handlePotentialKey);
  }, [handleRename, handleDelete, renaming]);

  // Whenever the selected dataset changes, we update the view tags.
  useEffect(() => {
    if (selected) actions.setTag(viewId, "dataset", selected);
    else actions.unsetTag(viewId, "dataset");
  }, [selected, viewId, actions]);

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <DatabaseIcon padded /> Datasets
      </Text>
    );
  }, []);

  return (
    <Views.Container
      title={title}
      closeable
      direction="vertical"
      ref={elementRef}
      onClick={() => actions.addHistory(viewId)}
    >
      {/* Display a searchbox for filtering the datasets. */}
      <Row>
        <Column>
          <SearchboxInput onChange={setQuery} clearable />
        </Column>
        <IconButtonAdd onClick={handleAddDataset} />
      </Row>

      {/* Display the dataset list. */}
      {/* If loading, display loading text. */}
      {!datasets.value && <Loading />}

      {/* Otherwise, display the list of datasets. */}
      {datasets.value && (
        <ul role="presentation" ref={datasetListRef}>
          {datasetFilter
            .filter(datasets.value)
            .map((dataset) =>
              renderDataset(
                dataset,
                (event) => handleSelect(dataset, event),
                setName,
                selected === dataset.id,
                renaming ? name : undefined
              )
            )}
        </ul>
      )}
    </Views.Container>
  );
};

export default DatasetListView;
