import classNames from "classnames";
import { FunctionComponent } from "react";
import { useMenu } from "./Menu";
import "./MenuOption.css";

// TODO: Cleanup.
interface MenuOptionProps {
  value: any;
  selected?: boolean;
}

const MenuOption: FunctionComponent<MenuOptionProps> = ({
  value,
  selected,
  children,
}) => {
  const menuContext = useMenu();

  return (
    <div
      className={classNames("MenuOption", { selected })}
      onClick={() => menuContext.select(value)}
    >
      {children}
    </div>
  );
};

export default MenuOption;
export type { MenuOptionProps };
