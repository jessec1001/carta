import { ComponentProps, FunctionComponent } from "react";
import { useControllableState } from "hooks";
import { Modify } from "types";
import { SearchIcon } from "components/icons";
import {
  InputAugment,
  InputAugmentContainer,
  TextFieldInput,
  IBaseInputProps,
} from "components/input";
import { JoinContainer, JoinInputButton } from "components/join";

/** The props used for the {@link SearchboxInput} component. */
interface SearchboxInputProps extends IBaseInputProps<string> {
  /** Whether the searchbox is clearable via an augment. */
  clearable?: boolean;
  /** Whether the searchbox is searcheable via a search button. */
  searchable?: boolean;

  /** The event handler for when the search button has been clicked. */
  onSearch?: (value: string) => void;
}

/** A searchbox component that can be cleared and searched by the click of special inserted buttons. */
const SearchboxInput: FunctionComponent<
  Modify<ComponentProps<typeof TextFieldInput>, SearchboxInputProps>
> = ({ clearable, searchable, value, onChange, onSearch, ...props }) => {
  // We allow this component to be optionally controlled.
  const [actualValue, setValue] = useControllableState("", value, onChange);

  return (
    <JoinContainer direction="horizontal" grow="grow-first">
      {/* Render the optional clear button as an augment. */}
      <InputAugmentContainer side="right">
        <TextFieldInput
          value={actualValue}
          placeholder="Search"
          onChange={(value) => setValue(value)}
          {...props}
        />
        {clearable && (
          <InputAugment interactive onClick={() => setValue("")}>
            ×
          </InputAugment>
        )}
      </InputAugmentContainer>

      {/* Render the optional search button as a join. */}
      {searchable && (
        <JoinInputButton onClick={() => onSearch && onSearch(actualValue)}>
          <SearchIcon />
        </JoinInputButton>
      )}
    </JoinContainer>
  );
};

export default SearchboxInput;
export type { SearchboxInputProps };
