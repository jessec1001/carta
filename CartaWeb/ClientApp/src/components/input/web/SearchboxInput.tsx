import React, { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import { SearchIcon } from "components/icons";
import { TextFieldInput } from "components/input";
import classNames from "classnames";

import "./inputs-web.css";

interface JoinProps {
  direction: "horizontal" | "vertical";
}

const Join: FunctionComponent<JoinProps> = ({ direction, children }) => {
  const childCount = React.Children.count(children);
  return (
    <span
      style={{
        display: "flex",
      }}
    >
      {React.Children.map(children, (child, index) => (
        <span
          key={index}
          className={classNames({
            "join-left": direction === "horizontal" && index > 0,
            "join-right": direction === "horizontal" && index + 1 < childCount,
            "join-top": direction === "vertical" && index > 0,
            "join-bototm": direction === "vertical" && index + 1 < childCount,
          })}
        >
          {child}
        </span>
      ))}
    </span>
  );
};

/** The props used for the {@link SearchboxInput} component. */
interface SearchboxInputProps {
  clearable?: boolean;
  searchable?: boolean;

  value?: string;

  onChange?: (value: string) => void;
  onSearch?: (value: string) => void;
}

/** A searchbox component that can be cleared and searched by the click of special inserted buttons. */
const SearchboxInput: FunctionComponent<SearchboxInputProps> = ({
  clearable,
  searchable,
  value,
  onChange,
  onSearch,
  children,
}) => {
  // We allow this component to be optionally controlled.
  const [actualValue, setValue] = useControllableState("", value, onChange);

  return (
    <span
      style={{
        display: "flex",
      }}
    >
      <span
        className={
          searchable ? "input-augmented join-right" : "input-augmented"
        }
        style={{
          position: "relative",
          flexGrow: 1,
        }}
      >
        <TextFieldInput
          value={actualValue}
          placeholder="Search"
          onChange={(value) => setValue(value)}
        />
        {clearable && (
          <span
            className="input-augment"
            style={{
              position: "absolute",
              right: "0%",
              top: "50%",
              transform: "translate(0, -50%)",
              cursor: "pointer",
              padding: "0rem 0.5rem",
            }}
            onClick={() => setValue("")}
          >
            ×
          </span>
        )}
      </span>
      <span
        style={{
          flexGrow: 0,
        }}
      >
        {searchable && (
          <button
            className="join-left"
            onClick={() => onSearch && onSearch(actualValue)}
            style={{
              width: "2em",
              height: "100%",
              textAlign: "center",
              border: "none",
              margin: "0",
              padding: "0",
              backgroundColor: "var(--color-primary)",
              borderRadius: "var(--border-radius)",
              borderTopLeftRadius: 0,
              borderBottomLeftRadius: 0,
            }}
          >
            <SearchIcon />
          </button>
        )}
      </span>
    </span>
  );
};

export default SearchboxInput;
export type { SearchboxInputProps };
