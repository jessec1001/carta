import { FunctionComponent, HTMLProps, useEffect, useState } from "react";
import { useAPI, useDelayCallback, useSequentialRequest } from "hooks";
import { Modify } from "types";
import { OperationType } from "library/api";
import { SearchboxInput } from "components/input";
import { Menu, MenuOption } from "components/menu";
import { VerticalScroll } from "components/scroll";
import { Loading, Text } from "components/text";
import "./OperationMenu.css";
import ReactMarkdown from "react-markdown";
import { useRef } from "react";

/**
 * The props used for the {@link OperationMenu} component.
 */
interface OperationMenuProps {
  /** The relative position of the menu. */
  position: {
    /** The x-coordinate of the position. */
    x: number;
    /** The y-coordinate of the position. */
    y: number;
  };

  /** The event handler that is called when an option is selected. If nothing is selected, `null` is passed in. */
  onSelect?: (option: { type: string; subtype: string | null } | null) => void;
}

// TODO: Final cleanup.
/**
 * A component that renders operations available to the user in a selectable menu. Displays the tags of operations as
 * well as their descriptions.
 */
const OperationMenu: FunctionComponent<OperationMenuProps> = ({
  position,
  onSelect,
}) => {
  const { operationsAPI } = useAPI();

  // We use a query to filter the operations that are relevant to a search.
  const [query, setQuery] = useState("");

  // We collect the operation types that is reflective of the query.
  const [types, setTypes] = useState<OperationType[] | null>(null);

  // Out of all of the types that are available, we also keep track of a selected operation.
  const [selected, setSelected] = useState<number | null>(null);
  const selectedRef = useRef<HTMLDivElement>(null);

  // We allow for the selection to be changed via the arrow keys.
  // Notice that these keyboard events should only be valid if the menu is open.
  useEffect(() => {
    // Make sure that the selected item is scrolled into view.
    if (selectedRef.current) {
      selectedRef.current.scrollIntoView({ block: "nearest" });
    }

    // Handle keyboard shortcuts.
    const handleKey = (event: KeyboardEvent) => {
      if (types === null || types.length === 0) return;
      switch (event.code) {
        case "ArrowUp":
          setSelected((selected) => {
            if (selected === null) return selected;
            else return (selected - 1 + types.length) % types.length;
          });

          // There is some default functionality to avoid.
          event.preventDefault();
          break;
        case "ArrowDown":
          setSelected((selected) => {
            if (selected === null) return selected;
            else return (selected + 1) % types.length;
          });

          // There is some default functionality to avoid.
          event.preventDefault();
          break;
        case "Enter":
          if (selected !== null && onSelect) {
            onSelect({
              type: types[selected].type,
              subtype: types[selected].subtype,
            });
          }
          break;
        case "Escape":
          if (onSelect) onSelect(null);
          break;
      }
    };

    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [onSelect, selected, types]);

  // Whenever the query is updated, we launch a request to update the list of operations.
  // When we make this request, other requests should be canceled by the hook and shortcut the latest request.
  const requester = useSequentialRequest(setTypes, true);
  const requesterDelay = useDelayCallback(requester, 250);
  useEffect(() => {
    // Get the various parts of the query.
    // Name parts are in plaintext.
    // Tag parts are prefixed by an octothorpe ('#').
    const parts = query.trim().split(/\s+/);
    const nameParts = parts.filter((part) => part.charAt(0) !== "#");
    const tagParts = parts.filter((part) => part.charAt(0) === "#");

    // Compute the name and tags for the query.
    const name = nameParts.join(" ");
    const tags = tagParts.map((part) => part.substring(1));

    // Request the queried types.
    requesterDelay(() =>
      operationsAPI.getOperationTypes(
        name.length > 0 ? name : undefined,
        tags.length > 0 ? tags : undefined
      )
    );
  }, [requesterDelay, query, operationsAPI]);

  // We also make sure to reset the selection every time that the query is updated if we cannot find the same type.
  useEffect(() => {
    setSelected(types && types.length > 0 ? 0 : null);
  }, [types]);

  return (
    <Menu position={position} onSelect={onSelect} className="OperationMenu">
      {/*
        Render a search bar that remains fixed at the top of this menu.
        This menu is automatically focused when the menu is opened.
      */}
      <SearchboxInput value={query} onChange={setQuery} autoFocus clearable />

      {/* Render a scrollable section with the operations queried. */}
      <VerticalScroll>
        {types === null && <Loading>Loading operations</Loading>}
        {types && types.length === 0 && (
          <Text color="muted">No operations matching query</Text>
        )}
        {types &&
          types.length > 0 &&
          types.map((type, index) => {
            // TODO: Change to `Menu.Option`.
            // TODO: Refactor
            return (
              <MenuOption
                key={`${type.type}:${type.subtype}`}
                value={{ type: type.type, subtype: type.subtype }}
                selected={index === selected}
              >
                <div
                  onMouseOver={() => setSelected(index)}
                  ref={index === selected ? selectedRef : undefined}
                >
                  <OperationMenuItem
                    type={type}
                    onClickTag={(tag) => setQuery(`#${tag}`)}
                  />
                </div>
              </MenuOption>
            );
          })}
      </VerticalScroll>

      {types &&
        types.length > 0 &&
        types.map((type, index) => {
          if (index === selected) {
            return (
              <div
                key={index}
                style={{
                  padding: "1rem",
                  position: "absolute",
                  width: "24em",
                  left: "100%",
                  top: "1.5rem",
                  backgroundColor: "var(--color-fill-element)",
                  boxShadow: "var(--shadow)",
                  borderTopRightRadius: "var(--border-radius)",
                  borderBottomRightRadius: "var(--border-radius)",
                }}
              >
                <Text size="normal">{type.display}</Text>
                <Text size="normal" color="muted">
                  <ReactMarkdown linkTarget="_blank">
                    {type.description ?? ""}
                  </ReactMarkdown>
                </Text>
              </div>
            );
          } else return null;
        })}
    </Menu>
  );
};

/**
 * The props used for the {@link OperationMenuItem} component.
 */
interface OperationMenuItemProps {
  /** The type of operation to list. */
  type: OperationType;

  /** Whether the operation description should be rendered. */
  describe?: boolean;

  /** The event handler that is called when an operation tag is clicked. */
  onClickTag?: (tag: string) => void;
}

/**
 * A component that renders an operation type with tags and relevant information.
 */
const OperationMenuItem: FunctionComponent<
  Modify<HTMLProps<HTMLDivElement>, OperationMenuItemProps>
> = ({ type, onClickTag, ...props }) => {
  if (type.type === "workflow" && !type.subtype) return null;
  return (
    <>
      <Text>
        <span style={{ display: "flex", justifyContent: "space-between" }}>
          <span>{type.display}</span>
          {type.type === "workflow" && (
            <a
              href={`${window.location.origin}/${type.subtype}`}
              style={{
                color: "var(--color-primary)",
              }}
              onClick={(event) => {
                window.location.assign(
                  `${window.location.origin}/${type.subtype}`
                );
                event.preventDefault();
                event.stopPropagation();
              }}
            >
              Edit
            </a>
          )}
        </span>
      </Text>
      <Text color="muted" size="small">
        {/* TODO: Make a tag component for non-link clickables. */}
        {type.tags.map((tag) => (
          <span
            key={tag}
            onClick={(event) => {
              onClickTag && onClickTag(tag);
              event.stopPropagation();
            }}
            className="Tag"
          >
            #{tag}{" "}
          </span>
        ))}
      </Text>
    </>
  );
};

export default OperationMenu;
export type { OperationMenuProps };
