import { FunctionComponent, useEffect, useMemo, useState } from "react";
import { ObjectFilter } from "library/search";
import { Accordian } from "components/accordian";
import { PropertyIcon, VertexIcon } from "components/icons";
import { SearchboxInput } from "components/input";
import { Column, Row } from "components/structure";
import { EmptySymbol, NullSymbol } from "components/symbols";
import { useViews, Views } from "components/views";
import { Text } from "components/text";
import {
  IDataVertex,
  IVisualizeProperty,
} from "components/visualizations/GraphVisualizer";

import "./VisualizerSelectionView.css";

/** A property that is attached to a particular vertex. */
interface AttachedProperty {
  /** The values of the property along with the vertex that owns this property. */
  value: [IDataVertex, any][];

  /** The subproperties of the property. */
  properties?: Record<string, AttachedProperty>;
}

/**
 * Renders a value for a property of any type.
 * @param values The value.
 * @returns An element that renders the value.
 */
const renderAttachedValues = (values: [IDataVertex, any][]) => {
  return (
    <ul role="presentation" className="VisualizerSelectionView-IndentList">
      {/* Loop through each of the values in the array. */}
      {values.map(([vertex, value], index) => {
        // Get the type and color of the value.
        const valueType = computeTypeName(value);
        return (
          <li key={index}>
            <Text
              align="middle"
              {...{ title: `Name: ${vertex.label}; ID: ${vertex.id}` }}
            >
              <VertexIcon
                padded
                selected={vertex.selected}
                expanded={vertex.expanded}
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

/**
 * Consolidates a list of properties into a single property hierarchy with all of the same-named properties at equal
 * levels condensed into a single list of values and subproperties.
 * @param properties The list of properties to consolidate.
 * @returns The consolidated properties.
 */
const consolidateProperties = (
  properties: Record<string, AttachedProperty[]> | undefined,
  filter?: ObjectFilter
): Record<string, AttachedProperty> => {
  // We store properties in a map so that there is one property (with possibly multiple values) per key.
  const consolidated: Record<string, AttachedProperty> = {};

  // Loop through each of the properties.
  if (properties) {
    for (const [propName, propValues] of Object.entries(properties)) {
      // Check if we should include this property based on the filter.
      if (
        filter &&
        filter.filter([
          {
            id: propName,
            values: propValues.flatMap((ap) => ap.value.map((v) => v[1])),
          },
        ]).length === 0
      ) {
        continue;
      }

      // If the property does not exist yet, add it to the consolidated properties.
      if (!consolidated[propName]) {
        consolidated[propName] = {
          value: [],
        };
      }
      const consolidatedProp = consolidated[propName];

      // Recursively consolidate the properties.
      const subproperties: Record<string, AttachedProperty[]> = {};
      for (const propValue of propValues) {
        // Merge the values.
        consolidatedProp.value.push(...propValue.value);

        if (propValue.properties) {
          for (const [subpropName, subpropValue] of Object.entries(
            propValue.properties
          )) {
            if (!subproperties[subpropName]) {
              subproperties[subpropName] = [];
            }
            subproperties[subpropName].push(subpropValue);
          }
        }
      }
      // Merge the subproperties.
      consolidatedProp.properties = consolidateProperties(subproperties);
    }
  }
  return consolidated;
};
/**
 * Formats the properties of a vertex so that the vertex is attached to each property.
 * @param vertex The vertex to format the properties of.
 * @returns The formatted properties.
 */
const formatProperties = (vertex: IDataVertex) => {
  const properties: Record<string, AttachedProperty> = {};

  // Loop through each of the properties of the vertex.
  // This is recursive and will stop once there are no more properties.
  if (vertex.properties) {
    for (const [propName, propValue] of Object.entries(vertex.properties)) {
      // Get the subproperties in attached form.
      const subproperties = formatProperties({
        ...vertex,
        properties: propValue.properties,
      });

      // Append the property to the list.
      properties[propName] = {
        value: [[vertex, propValue.value]],
        properties: subproperties,
      };
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

/**
 * Renders the tree of properties attached to a single vertex.
 * @param properties The properties attached to the vertex.
 * @returns The rendered properties.
 */
const renderVertexTree = (
  properties: Record<string, IVisualizeProperty> | undefined
) => {
  // Render the properties if they exist.
  if (!properties) return null;
  return (
    <ul role="presentation" className="VisualizerSelectionView-IndentList">
      {Object.entries(properties).map(([propName, propValue]) => {
        // We check if the property has subproperties and render them in an expandable accordian.
        const hasSubproperties =
          propValue.properties !== undefined &&
          Object.keys(propValue.properties).length > 0;
        return (
          <li key={propName}>
            <Accordian initialToggled={!hasSubproperties}>
              <Accordian.Header>
                {/* Render the title of the property along with the property icon. */}
                <Text align="middle">
                  <PropertyIcon padded />
                  {propName}
                </Text>
                {hasSubproperties && <Accordian.Toggle caret />}
              </Accordian.Header>
              <Accordian.Content>
                {/* Render the values first followed by subproperties second. */}
                {renderValue(propValue.value)}
                {hasSubproperties && renderVertexTree(propValue.properties)}
              </Accordian.Content>
            </Accordian>
          </li>
        );
      })}
    </ul>
  );
};
/**
 * Renders the tree of properties under a parent property (if it exists).
 * @param properties The subproperties of a property (in attached format).
 * @returns The rendered properties.
 */
const renderPropertyTree = (
  properties: Record<string, AttachedProperty> | undefined
) => {
  // Render the properties if they exist.
  if (!properties) return null;
  return (
    <ul role="presentation" className="VisualizerSelectionView-IndentList">
      {Object.entries(properties).map(([propName, propValue]) => {
        // We check if the property has subproperties or values and render them in an expandable accordian.
        const hasSubproperties =
          propValue.properties !== undefined &&
          Object.entries(propValue.properties).length > 0;
        const hasValues = propValue.value.length > 0;
        return (
          <li key={propName}>
            <Accordian initialToggled={!(hasSubproperties || hasValues)}>
              <Accordian.Header>
                {/* Render the title of the property along with the property icon. */}
                <Text align="middle">
                  <PropertyIcon padded />
                  {propName}
                </Text>
                {(hasSubproperties || hasValues) && <Accordian.Toggle caret />}
              </Accordian.Header>
              <Accordian.Content>
                {/* Render the values first followed by subproperties second. */}
                {renderAttachedValues(propValue.value)}
                {(hasSubproperties || hasValues) &&
                  renderPropertyTree(propValue.properties)}
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
 * @returns An element that renders the list of selected vertices.
 */
const renderVertexList = (vertices: IDataVertex[], filter: ObjectFilter) => {
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
                  <VertexIcon
                    padded
                    selected={vertex.selected}
                    expanded={vertex.expanded}
                  />
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
 * @returns An element that renders the list of selected properties.
 */
const renderPropertyList = (vertices: IDataVertex[], filter: ObjectFilter) => {
  // Get the considated properties of the vertices.
  // Notice that we also apply the filter so we can search for properties.
  const properties: Record<string, AttachedProperty[]> = {};
  for (const vertex of vertices) {
    if (!vertex.properties) continue;
    for (const [propName, propValue] of Object.entries(
      formatProperties(vertex)
    )) {
      if (properties[propName] === undefined) {
        properties[propName] = [];
      }
      properties[propName].push(propValue);
    }
  }
  const propertyTree = consolidateProperties(properties, filter);

  // Render the property tree.
  return renderPropertyTree(propertyTree);
};

/** A component that renders the active selection of a visualizer. */
const VisualizerSelectionView: FunctionComponent = () => {
  // Retrieve the dataset information.
  const { actions } = useViews();
  const visName: string | undefined = actions.getActiveTag("visualization");

  // Retrieve the graph information.
  // Additionally, setup event handlers to update the view when the graph selection changes.
  const vertices: IDataVertex[] | undefined = actions.getActiveTag("selected");
  useEffect(() => {
    // Update the selection if necessary.
    setSelection((selection) => {
      selection = new Map(selection);
      for (const vertex of vertices ?? []) {
        // If the vertex is not selected, select it.
        if (vertex.selected && !selection.has(vertex.id))
          selection.set(vertex.id, vertex);
        if (!vertex.selected && selection.has(vertex.id))
          selection.delete(vertex.id);
      }
      return selection;
    });
  }, [vertices]);

  // Setup the query and selection state.
  const [query, setQuery] = useState<string>("");
  const [selection, setSelection] = useState<Map<string, IDataVertex>>(
    new Map()
  );

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
        {visName ?? <Text color="warning">No Selection</Text>}&nbsp;
        <Text color="muted" size="small">
          [Selection]
        </Text>
      </Text>
    );
  }, [visName]);

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
      {mode === "vertex" &&
        renderVertexList(Array.from(selection.values()), filter)}
      {mode === "property" &&
        renderPropertyList(Array.from(selection.values()), filter)}
    </Views.Container>
  );
};

export default VisualizerSelectionView;
