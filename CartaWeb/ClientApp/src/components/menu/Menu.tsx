import classNames from "classnames";
import {
  FunctionComponent,
  HTMLProps,
  createContext,
  useCallback,
  useEffect,
  useRef,
  useContext,
} from "react";
import { Modify } from "types";
import "./Menu.css";

// TODO: Clean up.
interface IMenuContext {
  select: (option: any) => void;
}

const MenuContext = createContext<IMenuContext | undefined>(undefined);

interface MenuProps {
  position: { x: number; y: number };

  onSelect?: (option: any | null) => void;
}

const useMenu = (): IMenuContext => {
  const context = useContext(MenuContext);
  if (context === undefined) throw new Error("Must be used within a menu.");
  return context;
};

const Menu: FunctionComponent<Modify<HTMLProps<HTMLDivElement>, MenuProps>> = ({
  position,
  onSelect,
  children,
  className,
  style,
  ...props
}) => {
  const handleSelect = useCallback(
    (option: any) => {
      onSelect && onSelect(option);
    },
    [onSelect]
  );

  const elementRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const handleMouseDown = (event: MouseEvent) => {
      if (
        elementRef.current &&
        !elementRef.current.contains(event.target as Node)
      ) {
        handleSelect(null);
      }
    };

    window.addEventListener("mousedown", handleMouseDown);
    return () => window.addEventListener("mousedown", handleMouseDown);
  }, [handleSelect]);

  return (
    <MenuContext.Provider value={{ select: handleSelect }}>
      <div
        className={classNames(className, "Menu")}
        style={{
          ...style,
          left: position.x,
          top: position.y,
        }}
        ref={elementRef}
        {...props}
      >
        {children}
      </div>
    </MenuContext.Provider>
  );
};

export default Menu;
export { useMenu };
