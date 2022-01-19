import { FunctionComponent, useEffect, useMemo, useState } from "react";
import { useAPI, useMounted } from "hooks";
import { Accordian } from "components/accordian";
import { Button, ButtonGroup } from "components/buttons";
import { Link } from "components/common";
import { ApiException } from "library/exceptions";
import { FormGroup } from "components/form";
import { DatabaseIcon } from "components/icons";
import {
  CheckboxInput,
  SearchboxInput,
  TextFieldInput,
} from "components/input";
import { Text, Loading } from "components/text";
import { Column, Row } from "components/structure";
import { useViews, Views } from "components/views";

import "./DatasetAddView.css";

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
          <Text color="error">
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
          </Text>
        );
    }
  }

  // Default error handling text.
  return (
    <Text color="error">
      Error occurred ({error.status}): {error.message}
    </Text>
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
    <label className="DatasetAddView-Resource">
      <CheckboxInput
        className="DatasetAddView-Resource-Checkbox"
        value={selected}
      />
      {resource}
    </label>
  );
};

/** A component that renders a view to add datasets to a workspace. */
const DatasetAddView: FunctionComponent = () => {
  // We need the data API to make calls to get the data resources.
  const { dataAPI } = useAPI();

  // We store a reference to the selected source-resource pair.
  // For now, only a single dataset should be able to be selected at a time.
  const [selected, setSelected] = useState<{
    source: string;
    resource: string;
  } | null>(null);
  const [name, setName] = useState<string>("");

  // We use a searchbox query to filter the names of resources.
  const [query, setQuery] = useState<string>("");

  // These resources are stored in a dictionary:
  // Keys - Data source
  // Values - Data resources per source (or null if loading, or error if applicable)
  const mountedRef = useMounted();
  const [groupedResources, setGroupedResources] = useState<
    Record<string, string[] | ApiException | null>
  >({});

  // This effect loads the data resources asynchronously.
  useEffect(() => {
    // We use these methods to load in the data resources.
    const loadDataResources = async (source: string) => {
      try {
        const resources = await dataAPI.getResources(source);
        if (!mountedRef.current) return;
        setGroupedResources((groupedResources) => ({
          ...groupedResources,
          [source]: resources,
        }));
      } catch (error) {
        if (error instanceof ApiException) {
          if (!mountedRef.current) return;
          setGroupedResources((groupedResources) => ({
            ...groupedResources,
            [source]: error as ApiException,
          }));
        } else throw error;
      }
    };
    const loadDataSources = async () => {
      const sources = await dataAPI.getSources();
      sources.forEach((source) => {
        // Set the loading state if we do not have resources cached already.
        if (!mountedRef.current) return;
        setGroupedResources((groupedResources) =>
          !Array.isArray(groupedResources[source])
            ? { ...groupedResources, [source]: null }
            : groupedResources
        );

        // Procede to load the resources corresponding to this source.
        loadDataResources(source);
      });
    };

    // Start asynchronous data loading.
    loadDataSources();
  }, [mountedRef, dataAPI]);

  // We use the view context to create or remove views from the view container.
  const { viewId, rootId, actions } = useViews();
  const handleSelect = (source: string, resource: string) => {
    // When a dataset item is clicked, its selected state is toggled.
    // Since we currently only allow a single dataset, this will also move the selection around.
    if (selected?.source === source && selected.resource === resource)
      setSelected(null);
    else setSelected({ source, resource });
  };
  const handleAdd = () => {};
  const handleCancel = () => {
    actions.removeView(viewId);
  };

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <DatabaseIcon padded /> Add Dataset
      </Text>
    );
  }, []);

  return (
    <Views.Container title={title} closeable padded direction="vertical">
      {/* Render some information on how to use this view. */}
      <Text>
        Select a dataset to add to the workspace by clicking on the checkbox
        beside its name. You can optionally provide a more descriptive name for
        the dataset that will be displayed within the workspace.
      </Text>

      {/* Display a searchbox for filtering the datasets. */}
      <Row>
        <Column>
          <FormGroup density="flow">
            <SearchboxInput value={query} onChange={setQuery} clearable />
          </FormGroup>
        </Column>
      </Row>

      {/* Depending on the state of each group of resources, execute a corresponding rendering function. */}
      {Object.entries(groupedResources).map(([source, resources]) => {
        let contents;
        if (resources === null) {
          contents = <Loading />;
        } else if (resources instanceof ApiException) {
          contents = renderError(resources);
        } else {
          // Filter on the query in the search bar.
          resources = resources.filter((resource) =>
            resource.toLowerCase().includes(query.toLowerCase())
          );
          contents =
            resources.length > 0 ? (
              <Text>
                <ul role="presentation">
                  {resources.map((resource) => (
                    <li
                      key={resource}
                      onClick={() => handleSelect(source, resource)}
                    >
                      {renderResource(
                        source,
                        resource,
                        selected !== null &&
                          source === selected.source &&
                          resource === selected.resource
                      )}
                    </li>
                  ))}
                </ul>
              </Text>
            ) : (
              <Text>No data resources found.</Text>
            );
        }

        // Each source is rendered inside of an accordian that can be collapsed.
        return (
          <div key={source}>
            <Accordian>
              <Accordian.Header>
                <Text size="medium" align="middle">
                  <DatabaseIcon /> {source}
                </Text>
                <Accordian.Toggle caret />
              </Accordian.Header>
              <Accordian.Content>{contents}</Accordian.Content>
            </Accordian>
          </div>
        );
      })}

      {/* Render an input for the optional display name of the selected dataset. */}
      <FormGroup title="Name" density="flow">
        <TextFieldInput
          placeholder={
            selected ? `(${selected.source}/${selected.resource})` : undefined
          }
          disabled={selected === null}
          value={name}
          onChange={setName}
        />
      </FormGroup>

      {/* Render a set of buttons to perform or cancel the add dataset operation. */}
      <ButtonGroup>
        <Button
          color="primary"
          type="submit"
          onClick={handleAdd}
          disabled={selected === null}
        >
          Add
        </Button>
        <Button color="secondary" type="button" onClick={handleCancel}>
          Cancel
        </Button>
      </ButtonGroup>
    </Views.Container>
  );
};

export default DatasetAddView;
