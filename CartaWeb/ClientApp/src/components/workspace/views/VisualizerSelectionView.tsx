import {
  FunctionComponent,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { WorkspaceContext } from "context";
import {
  DataNode,
  defaultWorkspaceDatasetName,
  GraphData,
  Property,
} from "library/api";
import { ObjectFilter } from "library/search";
import { Accordian } from "components/accordian";
import { PropertyIcon, ValueIcon, VertexIcon } from "components/icons";
import { SearchboxInput } from "components/input";
import { Column, Row } from "components/structure";
import { EmptySymbol, NullSymbol } from "components/symbols";
import { useViews, Views } from "components/views";
import { Text } from "components/text";

import "./VisualizerSelectionView.css";

/** A property that is attached to a particular vertex. */
interface AttachedProperty extends Property {
  /** The values of the property along with the vertex that owns this property. */
  values: [DataNode, any][];

  /** The subproperties of the property. */
  properties?: AttachedProperty[];
}

/**
 * Renders a list of values for a property of any type.
 * @param values The list of values.
 * @returns An element that renders the list of values.
 */
const renderValues = (values: any[]) => {
  return (
    <ul role="presentation">
      {/* Loop through each of the values in the array. */}
      {values.map((obs, index) => {
        // Get the type and color of the value.
        const valueType = computeTypeName(obs);
        const valueColor = computeTypeColor(valueType);
        return (
          <li key={index}>
            <Text align="middle" {...({ title: valueType } as any)}>
              <ValueIcon color={valueColor} />
              {renderValue(obs)}
            </Text>
          </li>
        );
      })}
    </ul>
  );
};
const renderAttachedValues = (values: [DataNode, any][]) => {
  return (
    <ul role="presentation">
      {/* Loop through each of the values in the array. */}
      {values.map((obs, index) => {
        // Get the type and color of the value.
        const [vertex, value] = obs;
        const valueType = computeTypeName(value);
        return (
          <li key={index}>
            <Text align="middle">
              <VertexIcon
                padded
                selected
                color={vertex.color as string}
                {...{ title: vertex.label }}
              />
              <span title={valueType}>{renderValue(value)}</span>
            </Text>
          </li>
        );
      })}
    </ul>
  );
};

/**
 * Computes the color of a particular type of value.
 * @param typename The name of the type.
 * @returns The color assigned to the type.
 */
const computeTypeColor = (typename: string): string => {
  switch (typename) {
    case "number":
      return "#0000dd";
    case "boolean":
      return "#00bb00";
    case "string":
      return "#aa4422";
    case "null":
      return "#ff0000";
    case "array":
      return "#00dddd";
    case "map":
      return "#dd00dd";
    default:
      return "#888888";
  }
};

/**
 * Computes the name of a particular type of value.
 * @param value The value to compute the type of.
 * @returns The name of the type.
 */
const computeTypeName = (value: any) => {
  if (value === null) {
    return "null";
  } else if (Array.isArray(value)) {
    return "array";
  } else if (typeof value === "object") {
    return "map";
  } else {
    return typeof value;
  }
};

const consolidateProperties = (
  properties: AttachedProperty[] | undefined
): AttachedProperty[] => {
  // We store properties in a map so that there is one property (with possibly multiple values) per key.
  const consolidated: Record<string, AttachedProperty> = {};

  // Loop through each of the properties.
  if (properties) {
    for (const property of properties) {
      const id = property.id;

      if (id in consolidated) {
        // If the property already exists, merge the values.
        consolidated[id].values = consolidated[id].values.concat(
          property.values
        );

        // We also need to merge the subproperties.
        // This is done recursively.
        if (property.properties) {
          consolidated[id].properties = consolidateProperties(
            consolidated[id].properties?.concat(property.properties)
          );
        }
      } else {
        // If the property does not exist, add it to the consolidated list.
        consolidated[id] = property;
      }
    }
  }

  // We do not need the keys for the consolidated properties anymore.
  return Object.values(consolidated);
};
/**
 * Formats the properties of a vertex so that the vertex is attached to each property.
 * @param vertex The vertex to format the properties of.
 * @returns The formatted properties.
 */
const formatProperties = (vertex: DataNode) => {
  const properties: AttachedProperty[] = [];

  // Loop through each of the properties of the vertex.
  // This is recursive and will stop once there are no more properties.
  if (vertex.properties) {
    for (const property of vertex.properties) {
      // Get the subproperties in attached form.
      const subproperties = formatProperties({
        ...vertex,
        properties: property.properties,
      });

      // Append the property to the list.
      properties.push({
        id: property.id,
        values: property.values.map((value) => [vertex, value]),
        properties: subproperties,
      });
    }
  }

  return properties;
};

/**
 * Renders a particular value.
 * @param value The value to render.
 * @returns An element that renders the value.
 */
const renderValue = (value: any) => {
  if (value === null) {
    // Render null.
    return <NullSymbol />;
  } else if (Array.isArray(value)) {
    // Render an empty array.
    if (value.length === 0) return <EmptySymbol />;
    // Render the array as a list of values.
    else
      return (
        <table>
          <tr>
            {value.map((value, index) => (
              <td key={index}>{renderValue(value)}</td>
            ))}
          </tr>
        </table>
      );
  } else if (typeof value === "object") {
    // Render an empty object.
    if (Object.keys(value).length === 0) return <EmptySymbol />;
    else
      return (
        // Render the object as a list of key-value pairs.
        <table>
          {Object.entries(value).map(([key, value]) => (
            <tr key={key}>
              <td>{key}</td>
              <td>{renderValue(value)}</td>
            </tr>
          ))}
        </table>
      );
  } else if (typeof value === "string" && value === "") {
    // Render an empty string.
    return <EmptySymbol />;
  } else {
    // Render the value as its string representation if no further structure is found.
    return value.toString();
  }
};

const renderVertexTree = (properties: Property[] | undefined) => {
  if (!properties) return null;
  // TODO: Cleanup.
  return (
    <ul
      role="presentation"
      style={{
        marginLeft: "0.7em",
        paddingLeft: "0.5em",
        borderLeft: "1px solid var(--color-stroke-hairline)",
        color: "var(--color-stroke-lowlight)",
      }}
    >
      {properties.map((property) => {
        const hasSubproperties =
          property.properties !== undefined && property.properties.length > 0;
        return (
          <li key={property.id}>
            <Accordian initialToggled={!hasSubproperties}>
              <Accordian.Header>
                <Text align="middle">
                  <PropertyIcon padded />
                  {property.id}
                </Text>
                {hasSubproperties && <Accordian.Toggle caret />}
              </Accordian.Header>
              <Accordian.Content>
                {hasSubproperties && renderVertexTree(property.properties)}
                {renderValues(property.values)}
              </Accordian.Content>
            </Accordian>
          </li>
        );
      })}
    </ul>
  );
};
const renderPropertyTree = (properties: AttachedProperty[] | undefined) => {
  // TODO: Cleanup.
  if (!properties) return null;
  return (
    <ul
      role="presentation"
      style={{
        marginLeft: "0.7em",
        paddingLeft: "0.5em",
        borderLeft: "1px solid var(--color-stroke-hairline)",
        color: "var(--color-stroke-lowlight)",
      }}
    >
      {properties.map((property) => {
        const hasSubproperties =
          property.properties !== undefined && property.properties.length > 0;
        return (
          <li key={property.id}>
            <Accordian initialToggled={!hasSubproperties}>
              <Accordian.Header>
                <Text align="middle">
                  <PropertyIcon padded />
                  {property.id}
                </Text>
                {hasSubproperties && <Accordian.Toggle caret />}
              </Accordian.Header>
              <Accordian.Content>
                {hasSubproperties && renderPropertyTree(property.properties)}
                {renderAttachedValues(property.values)}
              </Accordian.Content>
            </Accordian>
          </li>
        );
      })}
    </ul>
  );
};

/**
 * Renders the visualizer selection as a list of vertices.
 * @param vertices The selected vertices.
 * @param filter A filter to apply to the vertices.
 * @param query The query to filter vertices by.
 * @returns An element that renders the list of selected vertices.
 */
const renderVertexList = (vertices: DataNode[], filter: ObjectFilter) => {
  // Get the filtered vertices.
  const filteredVertices = filter.filter(vertices);

  // Render the property trees of each vertex.
  return (
    <ul className="VisualizerSelectionView-List" role="presentation">
      {filteredVertices.map((vertex) => {
        return (
          <li key={vertex.id} className="VisualizerSelectionView-ListItem">
            <Accordian initialToggled={false}>
              <Accordian.Header>
                <Text
                  {...({ title: `ID: ${vertex.id}` } as any)}
                  align="middle"
                >
                  {/* TODO: Allow these vertex icons to be clicked on to focus on the vertex in the graph. */}
                  <VertexIcon padded selected color={vertex.color as string} />
                  &nbsp;
                  {vertex.label}
                </Text>
                <Accordian.Toggle caret />
              </Accordian.Header>
              <Accordian.Content>
                {renderVertexTree(vertex.properties)}
              </Accordian.Content>
            </Accordian>
          </li>
        );
      })}
    </ul>
  );
};
/**
 * Renders the visualizer selection as a list of properties.
 * @param vertices The selected vertices.
 * @param filter A filter to apply to the properties.
 * @param query The query to filter properties by.
 * @returns An element that renders the list of selected properties.
 */
const renderPropertyList = (vertices: DataNode[], filter: ObjectFilter) => {
  // Get the considated properties of the vertices.
  let properties: AttachedProperty[] = [];
  for (const vertex of vertices) {
    properties = properties.concat(formatProperties(vertex));
  }
  properties = consolidateProperties(properties);
  console.log(properties);

  // TODO: Apply the filter to the properties of the vertices.

  // Render the property tree.
  return renderPropertyTree(properties);
};

/** A component that renders the active selection of a visualizer. */
const VisualizerSelectionView: FunctionComponent = () => {
  // Retrieve the dataset information.
  const { actions } = useViews();
  const { datasets } = useContext(WorkspaceContext);
  const datasetId: string | undefined = actions.getActiveTag("dataset");
  const dataset =
    datasets.value?.find((dataset) => dataset.id === datasetId) ?? null;
  const datasetName =
    dataset && (dataset.name ?? defaultWorkspaceDatasetName(dataset));

  // Retrieve the graph information.
  // Additionally, setup event handlers to update the view when the graph selection changes.
  const graph: GraphData | undefined = actions.getActiveTag("graph");
  useEffect(() => {
    // The selection event handler.
    const handleSelection = () => {
      if (graph) setSelection(graph.nodes.get(graph.selection));
    };

    // Set the initial selection.
    if (graph) {
      setSelection(graph.nodes.get(graph.selection));
    } else {
      setSelection([]);
    }

    // If the graph changes, update the selection.
    if (graph) {
      graph.on("selectionChanged", handleSelection);
      graph.on("dataChanged", handleSelection);
      return () => {
        graph.off("selectionChanged", handleSelection);
        graph.off("dataChanged", handleSelection);
      };
    }
  }, [graph]);

  // Setup the query and selection state.
  const [query, setQuery] = useState<string>("");
  const [selection, setSelection] = useState<DataNode[]>([]);

  // The mode dictates whether to sort properties via property or vertex.
  const [mode, setMode] = useState<"vertex" | "property">("vertex");

  // Create a filter for the searchbox that matches vertices or properties depending on the mode.
  let filter: ObjectFilter;
  switch (mode) {
    case "vertex":
      filter = new ObjectFilter(query, { defaultProperty: "label" });
      break;
    case "property":
      // TODO: Implement filtering by value.
      filter = new ObjectFilter(query, { defaultProperty: "id" });
      break;
  }

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <VertexIcon padded selected />
        {datasetName ?? <Text color="warning">No Dataset</Text>}&nbsp;
        <Text color="muted" size="small">
          [Selection]
        </Text>
      </Text>
    );
  }, [datasetName]);

  return (
    <Views.Container title={title} closeable direction="vertical" padded>
      {/* Render a header with a searchbox. */}
      <Row>
        <Column>
          <SearchboxInput value={query} onChange={setQuery} clearable />
        </Column>
        &nbsp;
        <Text size="medium" align="middle">
          <span
            className="VisualizerSelectionView-Mode"
            onClick={() => {
              // Swap between vertex and property mode.
              setMode((mode) => (mode === "vertex" ? "property" : "vertex"));
            }}
          >
            {mode === "vertex" ? <VertexIcon selected /> : <PropertyIcon />}
          </span>
        </Text>
      </Row>

      {/* Depeding on the mode, render the selection. */}
      {mode === "vertex" && renderVertexList(selection, filter)}
      {mode === "property" && renderPropertyList(selection, filter)}
    </Views.Container>
  );
};

export default VisualizerSelectionView;
