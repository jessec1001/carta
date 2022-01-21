import classNames from "classnames";
import { ComponentProps, FC, useState } from "react";
import SplitsContext from "./Context";
import Panel from "./Panel";

/** The props used for the {@link Splits} component. */
interface SplitsProps extends ComponentProps<"div"> {
  /** The direction of the splits. */
  direction: "horizontal" | "vertical";
}

/**
 * Defines the composition of the compound {@link Splits} component.
 * @borrows Panel as Panel
 */
interface SplitsComposition {
  Panel: typeof Panel;
}

/**
 * A component that renders a split environment by the use of panels. Each panel has an associated identifier. These
 * panels are laid out in a specified direction and given a specified ratio of the space.
 * @example
 * ```jsx
 * <!-- Notice that one of the split panels has an unspecified ratio of space that is autocalculated. -->
 * <Splits direction="horizontal">
 *  <Splits.Panel id={1} ratio={0.5}>First Panel</Splits.Panel>
 *  <Splits.Panel id={2}>Second Panel</Splits.Panel>
 *  <Splits.Panel id={3} ratio={0.25}>Third Panel</Splits.Panel>
 * </Splits>
 * ```
 */
const Splits: FC<SplitsProps> & SplitsComposition = ({
  direction,
  className,
  children,
  ...props
}) => {
  // This map is used to store the ratio of each panel.
  // When a ratio is designated as 0, it means that the panel should be autocalculated.
  const [ratios, setRatios] = useState<Map<string | number, number | null>>(
    new Map()
  );

  // These are functions that will be used to more easily calculate the ratio of each panel.
  const getSize = (id: string | number): number => {
    const ratio = ratios.get(id);
    if (ratio === undefined) return 0;
    else {
      if (ratio === null) {
        // We need to autocalculate the ratio of this panel.
        // Notice that we know that the number of auto panels is at least 1 since we are calculating such a size.
        let autoPanels = 0;
        for (const value of ratios.values()) {
          if (value === null) autoPanels++;
        }
        return 1.0 / autoPanels;
      } else return ratio;
    }
  };
  const setSize = (id: string | number, size: number | null) => {
    setRatios((ratios) => {
      // We need to clone the ratios map.
      const newRatios = new Map(ratios);
      newRatios.set(id, size);
      return newRatios;
    });
  };
  const unsetSize = (id: string | number) => {
    setRatios((ratios) => {
      // We need to clone the ratios map.
      const newRatios = new Map(ratios);
      newRatios.delete(id);
      return newRatios;
    });
  };

  return (
    <SplitsContext.Provider value={{ direction, getSize, setSize, unsetSize }}>
      <div className={classNames(className)} {...props}>
        {children}
      </div>
    </SplitsContext.Provider>
  );
};
Splits.Panel = Panel;

export default Splits;
export type { SplitsProps };
