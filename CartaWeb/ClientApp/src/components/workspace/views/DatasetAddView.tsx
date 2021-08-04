import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from "react";
import { useAPI } from "hooks";
import { ViewContext } from "context";
import { WorkspaceDataset } from "library/api";
import { BlockButton } from "components/buttons";
import { Link } from "components/common";
import { ApiException } from "library/exceptions";
import { FormGroup } from "components/form";
import { DatabaseIcon } from "components/icons";
import { CheckboxInput, TextFieldInput } from "components/input";
import { VerticalScroll } from "components/scroll";
import { Tab } from "components/tabs";
import { ErrorText, Heading, Paragraph } from "components/text";
import DatasetGraphView from "./DatasetGraphView";

import "./view.css";

/**
 * Renders a loading status.
 * @returns  An element to render describing a loading status.
 */
const renderLoading = () => {
  return <span>Loading</span>;
};
/**
 * Renders an error that has been raised while loading resources.
 * - Handles special cases such as lack of HyperThought authentication.
 * @param error The error that has been raised.
 * @returns An element to render describing the error.
 */
const renderError = (error: ApiException) => {
  // Authentication related error.
  if (error.status === 401 || error.status === 403) {
    if (error.data) {
      // We render specific text if HyperThought authentication failed.
      if (
        typeof error.data.source === "string" &&
        error.data.source.toLowerCase() === "HyperThought".toLowerCase()
      )
        return (
          <Paragraph>
            You need to be authenticated with HyperThought&trade; to load these
            datasets. To access this data, add your HyperThought&trade; API key
            to the integration section on your{" "}
            <Link
              to="/profile#profile-integration-hyperthought"
              target="_blank"
            >
              profile
            </Link>
            .
          </Paragraph>
        );
    }
  }

  // Default error handling text.
  return (
    <ErrorText>
      Error occurred ({error.status}): {error.message}
    </ErrorText>
  );
};

/**
 * Renders a particular data resource item that can be selected when clicked.
 * @param source The data source.
 * @param resource The data resource.
 * @param selected Whether the data resource is selected.
 * @returns An element to render describing the resource.
 */
const renderResource = (
  source: string,
  resource: string,
  selected: boolean
) => {
  return (
    <label style={{ display: "flex" }}>
      <CheckboxInput value={selected} />{" "}
      <span style={{ padding: "0rem 0.5rem" }}>{resource}</span>
    </label>
  );
};

/** A component that renders a view to add datasets to a workspace. */
const DatasetAddView: FunctionComponent = ({ children }) => {
  // We need the data API to make calls to get the data resources.
  const { dataAPI } = useAPI();

  // These resources are stored in a dictionary:
  // Keys - Data source
  // Values - Data resources per source (or null if loading)
  const [groupedResources, setGroupedResources] = useState<
    Record<string, string[] | ApiException | null>
  >({});
  // This effect loads the data resources asynchronously.
  useEffect(() => {
    // We use these methods to load in the data resources.
    const loadDataResources = async (source: string) => {
      try {
        const resources = await dataAPI.getResources(source);
        setGroupedResources((groupedResources) => ({
          ...groupedResources,
          [source]: resources,
        }));
      } catch (error) {
        if (error instanceof ApiException)
          setGroupedResources((groupedResources) => ({
            ...groupedResources,
            [source]: error,
          }));
        else throw error;
      }
    };
    const loadDataSources = async () => {
      const sources = await dataAPI.getSources();
      sources.forEach((source) => {
        // Set the loading state if we do not have resources cached already.
        setGroupedResources((groupedResources) =>
          !Array.isArray(groupedResources[source])
            ? { ...groupedResources, [source]: null }
            : groupedResources
        );

        // Procede to load the resources corresponding to this source.
        loadDataResources(source);
      });
    };

    loadDataSources();
  }, [dataAPI]);

  // We store a reference to the selected source-resource pair.
  // For now, only a single dataset should be able to be selected at a time.
  const [selected, setSelected] = useState<{
    source: string;
    resource: string;
  } | null>(null);
  const [name, setName] = useState<string>("");
  // When a dataset item is clicked, its selected state is toggled.
  // Since we currently only allow a single dataset, this will also move the selection around.
  const handleSelect = (source: string, resource: string) => {
    if (selected?.source === source && selected.resource === resource)
      setSelected(null);
    else setSelected({ source, resource });
  };

  const { container, view, actions } = useContext(ViewContext);
  // TODO: Add a new dataset to the workspace, remove the current element from the container, and open the added dataset in a new container item on add.
  const handleAdd = () => {
    (async () => {
      // TODO: Add the new dataset to the workspace.
      const newDataset: WorkspaceDataset = null as any;

      // Remove the current element from the container.
      if (view) actions?.remove(view.id);

      // Open the added dataset in a new view.
      if (container) actions?.add(DatasetGraphView, container.id);
    })();
  };
  const handleCancel = () => {
    // Remove the current element from the container on cancel.
    if (view) actions?.remove(view.id);
  };

  return (
    // Render the view itself within a tab so it can be easily added to container views.
    <Tab
      title={
        <React.Fragment>
          <DatabaseIcon /> Add Dataset
        </React.Fragment>
      }
      status="none"
      closeable
      onClose={handleCancel}
    >
      <VerticalScroll>
        <div className="view">
          {/* Render some information on how to use this view. */}
          <Paragraph>
            Select a dataset to add to the workspace by clicking on the checkbox
            beside its name. You can optionally provide a more descriptive name
            for the dataset that will be displayed within the workspace.
          </Paragraph>

          {/* Depending on the state of each group of resources, execute a corresponding rendering function. */}
          {Object.entries(groupedResources).map(([source, resources]) => {
            let contents;
            if (resources === null) {
              contents = renderLoading();
            } else if (resources instanceof ApiException) {
              contents = renderError(resources);
            } else {
              contents =
                resources.length > 0 ? (
                  <span className="paragraph">
                    <ul role="presentation">
                      {resources.map((resource) => (
                        <li
                          key={resource}
                          onClick={() => handleSelect(source, resource)}
                        >
                          {renderResource(
                            source,
                            resource,
                            source === selected?.source &&
                              resource === selected.resource
                          )}
                        </li>
                      ))}
                    </ul>
                  </span>
                ) : (
                  <Paragraph>No data resources available.</Paragraph>
                );
            }

            return (
              <div key={source}>
                {/* TODO: Make into accordian component. */}
                <Heading>
                  <DatabaseIcon /> {source}
                </Heading>
                <div>{contents}</div>
              </div>
            );
          })}

          {/* Render an input for the optional display name of the selected dataset. */}
          <FormGroup title="Name" density="dense">
            <TextFieldInput
              placeholder={
                selected
                  ? `(${selected.source}/${selected.resource})`
                  : undefined
              }
              disabled={selected === null}
              value={name}
              onChange={setName}
            />
          </FormGroup>

          {/* Render a set of buttons to perform or cancel the add dataset operation. */}
          <div className="form-spaced-group">
            <BlockButton
              color="primary"
              type="submit"
              onClick={handleAdd}
              disabled={selected === null}
            >
              Add
            </BlockButton>
            <BlockButton color="secondary" type="button" onClick={handleCancel}>
              Cancel
            </BlockButton>
          </div>
        </div>
      </VerticalScroll>
    </Tab>
  );
};

export default DatasetAddView;
