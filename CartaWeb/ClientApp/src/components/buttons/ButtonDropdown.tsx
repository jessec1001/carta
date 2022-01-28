import { ComponentProps, FC, useState } from "react";
import { AppColors } from "app";
import { Button, ButtonGroup } from "components/buttons";
import { CaretIcon } from "components/icons";
import { OptionInput, OptionSelectorInput } from "components/input";
import { Text } from "components/text";

/** The props used for the {@link ButtonDropdown} comonent. */
interface ButtonDropdownProps extends ComponentProps<typeof ButtonGroup> {
  /** The color of the button. Defaults to primary. */
  color?: AppColors;
  /** Whether the button should be rendered as an outline. Defaults to fale. */
  outline?: boolean;

  /** The available options and their corresponding display values. */
  options: Record<string, string>;
  /** The default option for the button dropdown. */
  auto: string;

  onPick?: (value: string) => void;
}

/** A dropdown button component that renders an optional dropdown selector. */
const ButtonDropdown: FC<ButtonDropdownProps> = ({
  color = "primary",
  outline = false,
  options,
  auto,
  onPick = () => {},
  children,
  ...props
}) => {
  const [toggled, setToggled] = useState(false);

  return (
    <ButtonGroup connected {...props}>
      <Button
        type="button"
        color={color}
        outline={outline}
        onClick={() => onPick(auto)}
      >
        {children}
      </Button>
      <Button
        type="button"
        color={color}
        outline={outline}
        onClick={() => setToggled((toggled) => !toggled)}
      >
        <Text size="small" align="middle">
          <CaretIcon direction="down" />
        </Text>
      </Button>
      <OptionSelectorInput
        toggled={toggled}
        onSelect={(value: string) => onPick(value)}
      >
        {Object.entries(options).map(([value, display]) => (
          <OptionInput key={value} value={value}>
            {display}
          </OptionInput>
        ))}
      </OptionSelectorInput>
    </ButtonGroup>
  );
};

export default ButtonDropdown;
export type { ButtonDropdownProps };
