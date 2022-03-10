import classNames from "classnames";
import { ComponentProps, FC, useRef } from "react";
import Tile from "./Tile";
import styles from "./Mosaic.module.css";

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
const Mosaic: FC<MosaicProps> & MosaicComposition = ({
  gridSize,
  className,
  style,
  children,
  ...props
}) => {
  // We reference an element whose purpose is to provide a measurement of the current grid size.
  const measureElement = useRef<HTMLDivElement>(null);

  return (
    <div
      className={classNames(styles.mosaic, className)}
      style={{ ["--grid-size" as any]: gridSize, ...style }}
      {...props}
    >
      <div ref={measureElement} className={styles.measure} />
      {children}
    </div>
  );
};
Mosaic.Tile = Tile;

export default Mosaic;
