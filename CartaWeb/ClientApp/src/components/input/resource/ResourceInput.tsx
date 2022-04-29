import { Loading } from "components/text";
import { useAPI, useControllableState } from "hooks";
import { ObjectFilter } from "library/search";
import { ComponentProps, FC, useEffect, useState } from "react";
import { OptionInput } from "../general";
import ComboboxInput from "../general/ComboboxInput";

/** The props used for the {@link ResourceInput} component. */
interface ResourceInputProps
  extends Omit<ComponentProps<"div">, "value" | "onChange"> {
  /** A filter that is applied to resource objects to find valid ones. */
  filter?: string;
  /** The value that this option component takes on. */
  value?: string;
  /** The event handler for when the choice of resource has changed. */
  onChange?: (value: string) => void;
}
/**
 * A component that inputs a resource using a searcheable combobox input.
 */
const ResourceInput: FC<ResourceInputProps> = ({
  filter,
  value,
  onChange,
  ...props
}) => {
  // We need to allow this component to be optionally controllable because we are not using a native UI element.
  const [actualValue, setValue] = useControllableState("", value, onChange);

  // We need access to the data API to handle requests.
  const { dataAPI } = useAPI();

  // We need to get the list of resources.
  const [resources, setResources] = useState<
    { source: string; resource: string }[] | null
  >(null);
  useEffect(() => {
    const fetchResources = async () => {
      // Get all of the sources
      const sources = await dataAPI.getSources();

      // Get all of the resources for each source.
      const resourcesBySource = await Promise.all(
        sources.map(async (source) => {
          const resources = await dataAPI.getResources(source);
          return resources.map((resource) => ({ source, resource }));
        })
      );

      // Set the resources.
      setResources(resourcesBySource.flat());
    };
    fetchResources();
  }, [dataAPI]);

  // We construct a filter for the resources.
  const resourceFilter = new ObjectFilter(filter ?? "", {
    defaultProperty: "resource",
  });

  return (
    <ComboboxInput
      {...props}
      text={actualValue}
      value={actualValue}
      onTextChanged={setValue}
      onValueChanged={setValue}
    >
      {/* If the resources have not loaded yet, display a loading symbol. */}
      {!resources && (
        <OptionInput unselectable>
          <Loading>Loading resources</Loading>
        </OptionInput>
      )}

      {/* If there are no resources found in the search, display as such. */}
      {resources && resources.length === 0 && (
        <OptionInput unselectable>No resources found</OptionInput>
      )}

      {resources &&
        resourceFilter.filter(resources).map((resource) => {
          const value: string = `${resource.resource}`;

          return (
            <OptionInput
              key={resource.resource}
              value={value}
              alias={resource.resource}
            >
              {resource.source} {resource.resource}
            </OptionInput>
          );
        })}
    </ComboboxInput>
  );
};

export default ResourceInput;
export type { ResourceInputProps };
