import classNames from "classnames";
import { ComponentProps, FC, forwardRef, useCallback, useState } from "react";
import Tile from "./Tile";
import styles from "./Mosaic.module.css";
import MosaicContext from "./Context";

/** The props used for the {@link Mosaic} component. */
interface MosaicProps extends ComponentProps<"div"> {
  /** The grid size in CSS units. */
  gridSize?: string | number;
}

/**
 * Defines the composition of the compound {@link Mosaic} component.
 * @borrows Tile as Tile
 */
interface MosaicComposition {
  Tile: typeof Tile;
}

/** A component that renders a grid of mosaic-like tile components. */
const Mosaic: FC<MosaicProps> & MosaicComposition = forwardRef<
  HTMLDivElement,
  MosaicProps
>(({ gridSize, className, style, children, ...props }, ref) => {
  // We reference an element whose purpose is to provide a measurement of the current grid size.
  const [measureSize, setMeasureSize] = useState<[number, number]>([0, 0]);
  const measure = useCallback((element: HTMLDivElement | null) => {
    if (element) setMeasureSize([element.offsetWidth, element.offsetHeight]);
    else setMeasureSize([0, 0]);
  }, []);

  return (
    <div
      className={classNames(styles.mosaic, className)}
      style={{ ["--grid-size" as any]: gridSize, ...style }}
      ref={ref}
      {...props}
    >
      <div ref={measure} className={styles.measure} />
      <MosaicContext.Provider
        value={{
          gridSize: measureSize,
        }}
      >
        {children}
      </MosaicContext.Provider>
    </div>
  );
}) as any;
Mosaic.Tile = Tile;

export default Mosaic;
