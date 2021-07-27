import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import { SearchIcon } from "components/icons";
import {
  InputAugment,
  InputAugmentContainer,
  TextFieldInput,
} from "components/input";
import { JoinContainer, JoinInputButton } from "components/join";

/** The props used for the {@link SearchboxInput} component. */
interface SearchboxInputProps {
  /** Whether the searchbox is clearable via an augment. */
  clearable?: boolean;
  /** Whether the searchbox is searcheable via a search button. */
  searchable?: boolean;

  /** The search text currently inside the searchbox. */
  value?: string;

  /** The event handler for when the text entered by the user has changed. */
  onChange?: (value: string) => void;
  /** The event handler for when the search button has been clicked. */
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
    <JoinContainer direction="horizontal" grow="grow-first">
      {/* Render the optional clear button as an augment. */}
      <InputAugmentContainer side="right">
        <TextFieldInput
          value={actualValue}
          placeholder="Search"
          onChange={(value) => setValue(value)}
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
