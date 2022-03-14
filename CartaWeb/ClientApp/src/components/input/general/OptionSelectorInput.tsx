import React, {
  ComponentProps,
  FunctionComponent,
  ReactElement,
  useCallback,
  useEffect,
  useState,
} from "react";
import classNames from "classnames";
import OptionInput, { OptionInputProps } from "./OptionInput";

/** The props used for the {@link OptionSelectorInput} component. */
interface OptionSelectorInputProps extends ComponentProps<"div"> {
  /** Whether the option selector menu is toggled to be visible. */
  toggled?: boolean;
  // TODO: Allow for multiple selection.
  // multiple?: boolean;

  /** The event handler for when the value selected by the user has changed. */
  onSelect?: (value: any) => void;
}

/** A component that allows for any of multiple {@link OptionInput} components to have their value selected. */
const OptionSelectorInput: FunctionComponent<OptionSelectorInputProps> = ({
  toggled,
  onSelect,
  className,
  children,
  ...props
}) => {
  // We use an index to determine which option in the array is currently hovered over.
  const [index, setIndex] = useState(0);

  // Get the children of this element and split into a pair of partitions.
  // 1. If an element is not an `OptionInput`, it goes in the header of this element.
  // 2. if an element is an `OptionInput`, it gets transformed and displayed in a list.
  const childrenArray = React.Children.toArray(children);
  const headerArray = childrenArray.filter(
    (child) => !(React.isValidElement(child) && child.type === OptionInput)
  );
  const optionArray = childrenArray.filter(
    (child) => React.isValidElement(child) && child.type === OptionInput
  ) as ReactElement<OptionInputProps>[];

  // We use an event handler here to notify parent components of a selection change.
  const handleSelect = useCallback(
    (value: any) => {
      onSelect && onSelect(value);
    },
    [onSelect]
  );

  // We attach an event listener to check if a keyboard key was pressed.
  useEffect(() => {
    const handleKeyPress = (event: KeyboardEvent) => {
      // If a key was pressed while the option selector menu was toggled, handle it as possible navigation.
      if (toggled) {
        if (event.code === "ArrowUp")
          setIndex((index) => Math.max(0, index - 1));
        if (event.code === "ArrowDown")
          setIndex((index) => Math.min(optionArray.length - 1, index + 1));
        if (event.code === "Tab" && optionArray.length > 0) {
          handleSelect(optionArray[index].props.value);
        }
      }
    };

    // Make sure that the index is clamped inside an appropriate range.
    if (index < 0 || index >= optionArray.length)
      setIndex(Math.max(0, Math.min(optionArray.length - 1, index)));

    // Setup and teardown.
    window.addEventListener("keydown", handleKeyPress);
    return () => window.removeEventListener("keydown", handleKeyPress);
  }, [toggled, index, handleSelect, optionArray]);

  return (
    <div
      className={classNames("input-option-selector", { toggled }, className)}
      {...props}
    >
      {headerArray}
      {toggled && (
        // The set of options is only displayed if toggled is set.
        <ul className="input-option-list" role="presentation">
          {optionArray.map((child, childIndex) => (
            // Each of the options is rendered with appropriate event listeners to make the selector fully functional.
            // Notice the use of the mouse down event here because it fires before focus/blur events needed on combobox.
            <li
              key={childIndex}
              onMouseDown={() => handleSelect(child.props.value)}
              onMouseOver={() => setIndex(childIndex)}
              className={classNames("input-option", {
                selected: childIndex === index,
              })}
            >
              {child}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default OptionSelectorInput;
export type { OptionSelectorInputProps };
