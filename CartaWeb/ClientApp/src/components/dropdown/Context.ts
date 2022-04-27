import { createContext, useContext } from "react";

/**
 * The type of value used for the {@link DropdownContext}.
 * This is meant to provide the minimal functionality to use a dropdown.
 * Additional features are defined in {@link IDropdownActions}.
 */
interface IDropdownContext {
  /** Whether the dropdown is currently toggled. */
  toggled: boolean;
  /** Changes the toggled state to the specified value. */
  setToggled: (value: boolean) => void;
}
/**
 * Defines actions that can be performed on the {@link Dropdown} component.
 * Extends the functionality of the {@link IDropdownContext} interface.
 */
interface IDropdownActions {
  /** Sets the toggled state of the dropdown. */
  set: (value: boolean) => void;
  /** Toggles the dropdown to the on state. */
  on: () => void;
  /** Toggles the dropdown to the off state. */
  off: () => void;
  /** Toggles the dropdown to the opposite state. */
  toggle: () => void;
}
/**
 * Exposes the state of, and the actions performable on a {@link Dropdown}.
 */
interface IDropdown {
  /** Whether the dropdown is currently toggled. */
  toggled: boolean;
  /** Actions that can be performed on the dropdown. */
  actions: IDropdownActions;
}

/** The context used to expose information about the {@link Dropdown} component. */
const DropdownContext = createContext<IDropdownContext | undefined>(undefined);

/**
 * Returns an object that allows for determining the state of a dropdown along with actions that allow changing the
 * state of this dropdown.
 * @returns The state along with state-mutating actions.
 */
const useDropdown = (): IDropdown => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(DropdownContext);
  if (!context) {
    throw new Error(
      "Dropdown context must be used within a dropdown component."
    );
  }

  // We grab the original state from the context.
  const { toggled, setToggled } = context;

  return {
    toggled,
    actions: {
      set: setToggled,
      on: () => setToggled(true),
      off: () => setToggled(false),
      toggle: () => setToggled(!toggled),
    },
  };
};

export default DropdownContext;
export { useDropdown };
export type { IDropdownContext, IDropdownActions, IDropdown };
