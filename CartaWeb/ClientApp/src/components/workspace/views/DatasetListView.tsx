import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { ViewContext, WorkspaceContext } from "context";
import { WorkspaceDataset } from "library/api";
import { ObjectFilter } from "library/search";
import { IconAddButton } from "components/buttons";
import { DatabaseIcon, DatasetIcon } from "components/icons";
import { SearchboxInput, TextFieldInput } from "components/input";
import { VerticalScroll } from "components/scroll";
import { Column, Row } from "components/structure";
import { Tab } from "components/tabs";
import { Heading } from "components/text";
import DatasetAddView from "./DatasetAddView";
import DatasetGraphView from "./DatasetGraphView";

/** A component that renders a list of datasets that can be searched and sorted. */
const DatasetListView: FunctionComponent = () => {
  // We use these contexts to handle opening and closing views and managing data.
  const { container, view, actions } = useContext(ViewContext);
  const { datasets } = useContext(WorkspaceContext);

  // TODO: Open the dataset properties view when a dataset is selected and close it otherwise.
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
  const handleAdd = useCallback(() => {
    if (container) actions?.add(() => <DatasetAddView />, container.id);
  }, [actions, container]);
  const handleOpen = useCallback(
    (datasetId: string) => {
      if (container)
        actions?.add(() => <DatasetGraphView id={datasetId} />, container.id);
    },
    [actions, container]
  );
  const handleClose = useCallback(() => {
    if (view) actions?.remove(view.id);
  }, [actions, view]);

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

  // This handles the logic of selecting a particular dataset item.
  const handleSelect = useCallback(
    (dataset: WorkspaceDataset, event: React.MouseEvent) => {
      // This corresponds to a single click.
      if (event.detail === 1) {
        if (selected === dataset.id) {
          // If the current selected element was clicked, start renaming.
          const name =
            dataset.name ?? `(${dataset.source}/${dataset.resource})`;
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
      if (event.detail === 2) {
        handleOpen(dataset.id);
      }
    },
    [handleOpen, handleRename, selected, renaming]
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
      window.addEventListener("click", handlePotentialOutsideClick);
      return () =>
        window.removeEventListener("click", handlePotentialOutsideClick);
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
    };

    // Setup and teardown.
    window.addEventListener("keydown", handlePotentialKey);
    return () => window.removeEventListener("keydown", handlePotentialKey);
  }, [handleRename, renaming]);

  return (
    <Tab
      title={
        <React.Fragment>
          <DatabaseIcon padded /> Datasets
        </React.Fragment>
      }
      onClose={handleClose}
      closeable
    >
      <VerticalScroll>
        <style>
          {`
          .dataset-item:hover {
            background-color: var(--color-primary-hover);
          }
          .dataset-item.selected {
            background-color: var(--color-primary-select);
          }
          `}
        </style>
        <div
          className="view"
          style={{
            padding: "0rem",
          }}
        >
          <div
            style={{
              padding: "1rem",
            }}
          >
            <Heading>
              <Row>
                <Column>
                  <SearchboxInput onChange={setQuery} clearable />
                </Column>
                <IconAddButton onClick={handleAdd} />
              </Row>
            </Heading>
          </div>
          {!datasets.value && <span>Loading</span>}
          {datasets.value && (
            <ul role="presentation" ref={datasetListRef}>
              {datasetFilter.filter(datasets.value).map((dataset) => {
                const displayName =
                  dataset.name ?? `(${dataset.source}/${dataset.resource})`;
                const datasetSelected = selected === dataset.id;

                return (
                  <li
                    className={`dataset-item ${
                      datasetSelected ? "selected" : ""
                    }`}
                    key={dataset.id}
                    style={{
                      padding: "0rem 1rem",
                      cursor: "pointer",
                    }}
                    onClick={(event) => handleSelect(dataset, event)}
                  >
                    <span
                      className="normal-text"
                      style={{
                        display: "flex",
                      }}
                    >
                      <span
                        style={{
                          flexShrink: 0,
                        }}
                      >
                        <DatasetIcon padded />
                      </span>
                      {renaming && datasetSelected ? (
                        <TextFieldInput
                          value={name}
                          onChange={setName}
                          placeholder={displayName}
                        />
                      ) : (
                        displayName
                      )}
                    </span>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      </VerticalScroll>
    </Tab>
  );
};

// TODO: Toggle for resource/source hierarchy:
// <Database Icon> <Accordian> Source
//   <Database Icon> <Accordian> Resource
//     <Dataset Icon> Dataset Name
//     <Dataset Icon> Dataset Name
//   <Database Icon> <Accordian> Resource
//     <Dataset Icon> Dataset Name

// TODO: INTERACTIONS
/** Search
 * const filter = new ObjectFilter(searchText);
 * const filteredObjects = filter.filter(datasets);
 */

/** Focus
 * handleSingleClick = () => setFocusIndex(index)
 */

/** Rename
 * handleSingleClick = (event) => {
 *   if (event.detail === 1) {
 *     setRenamining(true);
 *   }
 * }
 *
 * handleRename = (event) => {
 * }
 */

export default DatasetListView;