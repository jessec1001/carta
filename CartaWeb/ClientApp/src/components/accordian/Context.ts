import { createContext, useCallback, useContext } from "react";

/**
 * The type of value used for the {@link AccordianContext}.
 * This is meant to provide the minimal functionality to use an accordian.
 * Additional features are defined in {@link IAccordianActions}.
 */
interface IAccordianContext {
  /** Whether the accordian is currently toggled. */
  toggled: boolean;
  /** Changes the toggled state to the specified value. */
  setToggled: (value: boolean) => void;
}
/**
 * Defines actions that can be performed on the {@link Accordian} component.
 * Extends the functionality of the {@link IAccordianContext} interface.
 */
interface IAccordianActions {
  /** Sets the toggled state of the accordian. */
  set: (value: boolean) => void;
  /** Toggles the accordian to the on state. */
  on: () => void;
  /** Toggles the accordian to the off state. */
  off: () => void;
  /** Toggles the accordian to the opposite state. */
  toggle: () => void;
}
/**
 * Exposes the state of, and the actions performable on an {@link Accordian}.
 */
interface IAccordian {
  /** Whether the accordian is currently toggled. */
  toggled: boolean;
  /** Actions that can be performed on the accordian. */
  actions: IAccordianActions;
}

/** The context used to expose information about the {@link Accordian} component. */
const AccordianContext = createContext<IAccordianContext | undefined>(
  undefined
);

/**
 * Returns an object that allows for determining the state of an accordian along with actions that allow changing the
 * state of this accordian.
 * @returns The state along with state-mutating actions.
 */
const useAccordian = (): IAccordian => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(AccordianContext);
  if (!context) {
    throw new Error(
      "Accordian context must be used within an accordian component."
    );
  }

  // We grab the original state from the context.
  const { toggled, setToggled } = context;

  // From the simple functionality that the context provides, we can define additional actions.
  const set = setToggled;
  const on = useCallback(() => setToggled(true), [setToggled]);
  const off = useCallback(() => setToggled(false), [setToggled]);
  const toggle = useCallback(() => setToggled(!toggled), [toggled, setToggled]);

  // We return this modified context.
  return {
    toggled,
    actions: {
      set,
      on,
      off,
      toggle,
    },
  };
};

export default AccordianContext;
export { useAccordian };
export type { IAccordianContext, IAccordianActions, IAccordian };
