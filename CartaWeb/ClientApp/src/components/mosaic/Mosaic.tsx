import classNames from "classnames";
import React, {
  ComponentProps,
  FC,
  forwardRef,
  useCallback,
  useEffect,
  useRef,
  useState,
} from "react";
import Tile from "./Tile";
import styles from "./Mosaic.module.css";
import MosaicContext from "./Context";
import { useCombinedRefs } from "hooks";

// TODO: Finish implementation.
// // We use A* to find the shortest path from the source to the target.
// // We have a preference of traversing diagonally, then horizontally, then vertically.
// class MosaicPathfinding {
//   private heuristic(
//     [x1, y1]: [number, number],
//     [x2, y2]: [number, number]
//   ): number {
//     const dx = Math.abs(x1 - x2);
//     const dy = Math.abs(y1 - y2);
//     const diag = Math.min(dx, dy);
//     const straight = dx + dy;
//     return Math.sqrt(2 * diag) + (straight - 2 * diag);
//   }

//   private bounds(
//     tiles: ITile[]
//   ): [xMin: number, xMax: number, yMin: number, yMax: number] {
//     let xMin =
//       tiles.map((tile) => tile.position[0]).reduce((a, b) => Math.min(a, b)) -
//       1;
//     let xMax =
//       tiles
//         .map((tile) => tile.position[0] + tile.dimensions[0])
//         .reduce((a, b) => Math.max(a, b)) + 1;
//     let yMin =
//       tiles.map((tile) => tile.position[1]).reduce((a, b) => Math.min(a, b)) -
//       1;
//     let yMax =
//       tiles
//         .map((tile) => tile.position[1] + tile.dimensions[1])
//         .reduce((a, b) => Math.max(a, b)) + 1;
//     return [xMin, xMax, yMin, yMax];
//   }

//   private neighbors(
//     bounds: [xMin: number, xMax: number, yMin: number, yMax: number],
//     [x, y]: [number, number]
//   ): [number, number][] {
//     const neighbors = [];
//     const [xMin, xMax, yMin, yMax] = bounds;

//     // We include vertical, horizontal, and diagonal neighbors.
//     // We don't include neighbors that are out of bounds.
//     if (x > xMin) neighbors.push([x - 1, y]);
//     if (x < xMax) neighbors.push([x + 1, y]);
//     if (y > yMin) neighbors.push([x, y - 1]);
//     if (y < yMax) neighbors.push([x, y + 1]);
//     if (x > xMin && y > yMin) neighbors.push([x - 1, y - 1]);
//     if (x < xMax && y > yMin) neighbors.push([x + 1, y - 1]);
//     if (x > xMin && y < yMax) neighbors.push([x - 1, y + 1]);
//     if (x < xMax && y < yMax) neighbors.push([x + 1, y + 1]);

//     return neighbors as [number, number][];
//   }

//   public pathfind(
//     tiles: ITile[],
//     [x1, y1]: [number, number],
//     [x2, y2]: [number, number]
//   ) {
//     // We calculate the bounds to use in neighbors calculations.
//     const bounds = this.bounds(tiles);

//     // We store tiles that we have visited and are pending.
//     const openTiles = new Set<[number, number]>();
//     const closedTiles = new Set<[number, number]>();
//   }
// }

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
  const [measureElement, setMeasureElement] = useState<HTMLDivElement | null>(
    null
  );
  useEffect(() => {
    if (!measureElement) setMeasureSize([0, 0]);
    else {
      const handleResize = () =>
        setMeasureSize([
          measureElement.offsetWidth,
          measureElement.offsetHeight,
        ]);

      // Setup an interval to update the grid size.
      const interval = setInterval(handleResize, 100);
      return () => clearInterval(interval);
    }
  }, [measureElement]);

  // We store a reference to the mosaic element.
  const innerRef = useRef<HTMLDivElement>(null);
  const combinedRef = useCombinedRefs<HTMLDivElement>(ref as any, innerRef);

  // We store a position of the grid.
  const [panning, setPanning] = useState<boolean>(false);
  const [position, setPosition] = useState<[number, number]>([0, 0]);
  const [zoom, setZoom] = useState<number>(1);

  // TODO: Reimplement zooming.
  // We update the position of the grid on mouse move.
  const handleStartPan = useCallback(
    (event: React.MouseEvent<HTMLDivElement>) => {
      if (event.target !== combinedRef.current) return;
      setPanning(true);
    },
    [combinedRef]
  );
  const handleEndPan = useCallback(
    (event: React.MouseEvent<HTMLDivElement>) => {
      setPanning(false);
    },
    []
  );
  const handlePan = useCallback(
    (event: React.MouseEvent) => {
      // We only pan if the measure size is non-zero and the mouse is being pressed.
      if (!panning) return;
      if (event.target !== event.currentTarget) return;

      // We add the mouse movement to the remainder.
      const [px, py] = position;
      const [dx, dy] = [event.movementX, event.movementY];
      setPosition([px + dx, py + dy]);
    },
    [panning, position]
  );
  const handleZoom = useCallback(
    (event: React.WheelEvent) => {
      if (event.target !== combinedRef.current) return;
      const effect = 1 - event.deltaY / 1000;
      setZoom((zoom) => Math.min(2, Math.max(0.5, zoom * effect)));
    },
    [combinedRef]
  );

  return (
    <div
      className={classNames(styles.mosaic, className)}
      style={{
        ["--grid-size" as any]: gridSize,
        ["--grid-zoom" as any]: zoom,
        backgroundPositionX: position[0],
        backgroundPositionY: position[1],
        cursor: panning ? "grab" : "default",
        ...style,
      }}
      onMouseDown={handleStartPan}
      onMouseUp={handleEndPan}
      onMouseLeave={() => setPanning(false)}
      onMouseMove={handlePan}
      onWheel={handleZoom}
      ref={combinedRef}
      {...props}
    >
      <div ref={setMeasureElement} className={styles.measure} />
      <MosaicContext.Provider
        value={{
          gridSize: measureSize,
          gridPosition: position,
          gridZoom: zoom,
        }}
      >
        {children}
      </MosaicContext.Provider>
    </div>
  );
}) as any;
Mosaic.Tile = Tile;

export default Mosaic;
