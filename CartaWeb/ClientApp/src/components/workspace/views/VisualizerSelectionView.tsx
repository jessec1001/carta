import {
  Accordian,
  AccordianContent,
  AccordianHeader,
  AccordianToggle,
} from "components/accordian";
import { ValueIcon, VertexIcon } from "components/icons";
import { SearchboxInput } from "components/input";
import { VerticalScroll } from "components/scroll";
import { Column, Row } from "components/structure";
import { EmptySymbol, NullSymbol } from "components/symbols";
import { Tab } from "components/tabs";
import { ViewContext } from "components/views";
import { WorkspaceContext } from "context";
import { DataNode, GraphData, Property } from "library/api";
import { ObjectFilter } from "library/search";
import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from "react";

const renderValues = (values: any[]) => {
  return (
    <ul role="presentation">
      {values.map((obs, index) => {
        const valueType = computeTypeName(obs);
        const valueColor = computeTypeColor(valueType);
        return (
          <li key={index} style={{ display: "flex" }}>
            <span
              style={{
                display: "inline-flex",
                alignItems: "center",
                alignSelf: "flex-start",
                padding: "0.1em 0em",
                flexShrink: 0,
              }}
              title={valueType}
            >
              <ValueIcon color={valueColor} />
            </span>
            <span>{renderValue(obs)}</span>
            {/* <span className="observation-type text-muted">
            {computeTypeName(obs)}
          </span> */}
          </li>
        );
      })}
    </ul>
  );
};

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

const computeTypeName = (obs: any) => {
  if (obs === null) {
    return "null";
  } else if (Array.isArray(obs)) {
    return "array";
  } else if (typeof obs === "object") {
    return "map";
  } else {
    return typeof obs;
  }
};

const renderValue = (obs: any) => {
  if (obs === null) {
    return <NullSymbol />;
  } else if (Array.isArray(obs)) {
    if (obs.length === 0) return <EmptySymbol />;
    else
      return (
        <table>
          <tr>
            {obs.map((value, index) => (
              <td key={index}>{renderValue(value)}</td>
            ))}
          </tr>
        </table>
      );
  } else if (typeof obs === "object") {
    if (Object.keys(obs).length === 0) return <EmptySymbol />;
    else
      return (
        <table>
          {Object.entries(obs).map(([key, value]) => (
            <tr key={key}>
              <td>{key}</td>
              <td>{renderValue(value)}</td>
            </tr>
          ))}
        </table>
      );
  } else if (typeof obs === "string" && obs === "") {
    return <EmptySymbol />;
  } else {
    return obs.toString();
  }
};

const renderPropertyTree = (properties: Property[] | undefined) => {
  if (!properties) return null;
  return (
    <ul
      role="presentation"
      style={{
        marginLeft: "0.2em",
        // marginLeft: "0.7em",
        paddingLeft: "0.8em",
        borderLeft: "1px solid var(--color-stroke-faint)",
        color: "var(--color-stroke-lowlight)",
      }}
    >
      {properties.map((property) => {
        const hasSubproperties =
          property.properties !== undefined && property.properties.length > 0;
        return (
          <li key={property.id}>
            <Accordian initialToggled={!hasSubproperties}>
              <AccordianHeader>
                {property.id}
                {hasSubproperties && <AccordianToggle caret />}
              </AccordianHeader>
              <AccordianContent>
                {hasSubproperties && renderPropertyTree(property.properties)}
                {renderValues(property.values)}
              </AccordianContent>
            </Accordian>
          </li>
        );
      })}
    </ul>
  );
};

const VisualizerSelectionView: FunctionComponent = () => {
  const { viewId, activeId, actions } = useContext(ViewContext);
  const activeView = activeId === null ? null : actions.getView(activeId);

  const { datasets } = useContext(WorkspaceContext);
  const datasetId: string | undefined = activeView?.tags["dataset"];
  // TODO: Better way to find dataset.
  const dataset =
    datasets.value?.find((dataset) => dataset.id === datasetId) ?? null;
  // TODO: Better way to get dataset name.
  const datasetName =
    dataset && (dataset.name ?? `(${dataset.source}/${dataset.resource})`);

  const [query, setQuery] = useState<string>("");
  const [selection, setSelection] = useState<DataNode[]>([]);

  const [mode, setMode] = useState<"vertex" | "property">("vertex");

  const graph: GraphData | undefined = activeView?.tags["graph"];
  useEffect(() => {
    const handleSelection = () => {
      if (graph) {
        setSelection(graph.nodes.get(graph.selection));
      }
    };

    if (graph) {
      setSelection(graph.nodes.get(graph.selection));
    } else {
      setSelection([]);
    }
    if (graph) {
      graph.on("selectionChanged", handleSelection);
      graph.on("dataChanged", handleSelection);
      return () => {
        graph.off("selectionChanged", handleSelection);
        graph.off("dataChanged", handleSelection);
      };
    }
  }, [graph]);

  const handleClose = () => {
    actions.removeElement(viewId);
  };

  let filter: ObjectFilter;
  switch (mode) {
    case "vertex":
      filter = new ObjectFilter(query, { defaultProperty: "label" });
      break;
    case "property":
      filter = new ObjectFilter(query, { defaultProperty: "id" });
      break;
  }

  return (
    <Tab
      title={
        <React.Fragment>
          <VertexIcon padded selected />
          {(graph && datasetName) ?? (
            <span style={{ color: "var(--color-stroke-faint)" }}>(None)</span>
          )}
          &nbsp;
          <span
            style={{
              color: "var(--color-stroke-faint)",
              fontSize: "var(--font-small)",
            }}
          >
            [Selection]
          </span>
        </React.Fragment>
      }
      closeable
      onClose={handleClose}
    >
      <VerticalScroll>
        <div
          style={{
            padding: "1rem",
          }}
        >
          <Row>
            <Column>
              <SearchboxInput value={query} onChange={setQuery} clearable />
            </Column>
          </Row>
          <ul
            role="presentation"
            style={{
              paddingTop: "1rem",
            }}
          >
            {filter.filter(selection).map((vertex) => {
              return (
                <li key={vertex.id}>
                  <Accordian initialToggled={false}>
                    <AccordianHeader>
                      <span
                        className="normal-text"
                        title={vertex.label}
                        style={{
                          marginLeft: "-0.5em",
                        }}
                      >
                        {/* TODO: Allow these vertex icons to be clicked on to focus on the vertex in the graph. */}
                        <VertexIcon
                          padded
                          selected
                          color={vertex.color as string}
                        />{" "}
                        {vertex.label}
                      </span>
                      <AccordianToggle caret />
                    </AccordianHeader>
                    <AccordianContent>
                      {renderPropertyTree(vertex.properties)}
                    </AccordianContent>
                  </Accordian>
                </li>
              );
            })}
          </ul>
        </div>
      </VerticalScroll>
    </Tab>
  );
};

export default VisualizerSelectionView;
