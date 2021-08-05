import React, { FunctionComponent, useContext } from "react";
import { ViewContext, WorkspaceContext } from "context";
import { IconAddButton } from "components/buttons";
import { DatabaseIcon, DatasetIcon } from "components/icons";
import { SearchboxInput } from "components/input";
import { VerticalScroll } from "components/scroll";
import { Column, Row } from "components/structure";
import { Tab } from "components/tabs";
import { Heading } from "components/text";
import DatasetAddView from "./DatasetAddView";

/** A component that renders a list of datasets that can be searched and sorted. */
const DatasetListView: FunctionComponent = () => {
  const { container, actions } = useContext(ViewContext);
  const { datasets } = useContext(WorkspaceContext);

  const handleAdd = () => {
    if (container) actions?.add(() => <DatasetAddView />, container.id);
  };

  return (
    <Tab
      title={
        <React.Fragment>
          <DatabaseIcon /> Datasets
        </React.Fragment>
      }
    >
      <VerticalScroll>
        <div className="view">
          <Heading>
            <Row>
              <Column>
                <SearchboxInput clearable />
              </Column>
              <IconAddButton onClick={handleAdd} />
            </Row>
          </Heading>
          {!datasets.value && <span>Loading</span>}
          {datasets.value && (
            <ul
              role="presentation"
              style={{
                margin: "0.5rem 0rem",
              }}
            >
              {datasets.value.map((dataset) => (
                <li key={dataset.id}>
                  <span
                    className="normal-text"
                    style={{
                      color: "var(--color-stroke-lowlight)",
                    }}
                  >
                    <DatasetIcon />
                    {dataset.name ?? `(${dataset.source}/${dataset.resource})`}
                  </span>
                </li>
              ))}
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
