import { FunctionComponent, useEffect, useState } from "react";
import { useAPI } from "hooks";
import { ApiException } from "library/exceptions";
import { DatabaseIcon } from "components/icons";
import { ErrorText, Heading, Paragraph } from "components/text";
import { CheckboxInput, TextFieldInput } from "components/input";
import { BlockButton } from "components/buttons";
import { FormGroup } from "components/form";

const DatasetAddView: FunctionComponent = ({ children }) => {
  const { dataAPI } = useAPI();

  // These resources are stored in a dictionary:
  // Keys - Data source
  // Values - Data resources per source (or null if loading)
  const [groupedResources, setGroupedResources] = useState<
    Record<string, string[] | ApiException | null>
  >({});

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
          groupedResources[source] === undefined
            ? { ...groupedResources, [source]: null }
            : groupedResources
        );

        // Procede to load the resources corresponding to this source.
        loadDataResources(source);
      });
    };

    loadDataSources();
  }, [dataAPI]);

  return (
    <div
      style={{
        flexGrow: 1,
        width: "100%",
        height: "100%",
        // backgroundColor: "var(--color-fill-element)",
      }}
    >
      <Paragraph>
        Select a dataset to add to the workspace by clicking on the checkbox
        beside its name. You can optionally provide a more descriptive name for
        the dataset that will be displayed within the workspace.
      </Paragraph>
      {Object.entries(groupedResources).map(([source, resources]) => {
        return (
          <div>
            <Heading>
              <DatabaseIcon /> {source}
            </Heading>
            {resources === null && "Loading"}
            {resources instanceof ApiException && (
              <span style={{ display: "block", padding: "0.5em" }}>
                <ErrorText>
                  Error occurred ({resources.status}): {resources.message}
                </ErrorText>
              </span>
            )}
            {Array.isArray(resources) &&
              (resources.length > 0 ? (
                <ul
                  style={{
                    padding: "0.5em",
                  }}
                >
                  {resources.map((resource) => (
                    <li
                      style={{
                        display: "flex",
                        alignItems: "flex-start",
                      }}
                    >
                      <span style={{ flexShrink: 0, marginRight: "0.5em" }}>
                        <CheckboxInput />
                      </span>
                      <span>{resource}</span>
                    </li>
                  ))}
                </ul>
              ) : (
                <span style={{ display: "inline-block", padding: "0.5em" }}>
                  No data resources available.
                </span>
              ))}
          </div>
        );
      })}
      <FormGroup title="Name" density="dense">
        <TextFieldInput placeholder="(source/resource)" />
      </FormGroup>
      <div className="form-spaced-group">
        <BlockButton color="primary" type="submit">
          Add
        </BlockButton>
        <BlockButton color="secondary" type="button">
          Cancel
        </BlockButton>
      </div>
    </div>
  );
};

export default DatasetAddView;
