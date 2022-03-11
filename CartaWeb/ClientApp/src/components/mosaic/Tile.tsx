import classNames from "classnames";
import React, {
  ComponentProps,
  FC,
  Fragment,
  useEffect,
  useRef,
  useState,
} from "react";
import { useMosaic } from "./Context";
import styles from "./Tile.module.css";

// TODO: Drabble handles.
// TODO: Draggable edges and corners for adjusting size.

/** The base structure of a tile. */
interface ITile {
  position: [number, number];
  dimensions: [number, number];
}

/** The props used for the {@link TileHandle} component. */
interface TileHandleProps extends ComponentProps<"div"> {
  /** An event listener that is called when the handle is offset via dragging. */
  onOffset?: (offset: [number, number]) => void;
}
/** Wraps an element with a handle that allows for adjusting the tile. */
const TileHandle: FC<TileHandleProps> = ({
  onOffset = () => {},
  className,
  children,
}) => {
  // We store the mouse position and a reference to the handle to allow for dragging.
  const element = useRef<HTMLDivElement>(null);
  const [, setMousePosition] = useState<[number, number] | null>(null);
  const [, setMouseRemainder] = useState<[number, number]>([0, 0]);

  // Fetch the grid size of the mosaic.
  const { gridSize } = useMosaic();

  // Prepare event handlers for mouse events.
  useEffect(() => {
    const handleMouseDown = (event: MouseEvent) => {
      // We only want to handle left clicks.
      if (event.button !== 0) return;
      if (element.current) {
        if (event.target && element.current.contains(event.target as Node)) {
          setMousePosition([event.clientX, event.clientY]);
          event.stopPropagation();
          event.cancelBubble = true;
        }
      }
    };
    const handleMouseUp = () => {
      // Once the mouse is released, we reset the mouse position.
      setMousePosition(null);
      setMouseRemainder([gridSize[0], gridSize[1]]);
    };
    const handleMouseMove = (event: MouseEvent) => {
      // Whenever the mouse is moved, we issue an offset event if we have moved more than a grid cell.
      setMousePosition((position) => {
        if (!position || !element.current) return position;
        else {
          // Update the mouse remainder.
          const movement = [
            (event.clientX - position[0]) / 2,
            (event.clientY - position[1]) / 2,
          ];
          setMouseRemainder((remainder) => {
            const offset: [number, number] = [0, 0];
            const remainderBounding: [number, number] = [
              remainder[0] + movement[0],
              remainder[1] + movement[1],
            ];

            // If the remainder is outside of the grid size, we offset the handle.
            while (remainderBounding[0] < 0) {
              offset[0]--;
              remainderBounding[0] += gridSize[0];
            }
            while (remainderBounding[0] >= gridSize[0]) {
              offset[0]++;
              remainderBounding[0] -= gridSize[0];
            }
            while (remainderBounding[1] < 0) {
              offset[1]--;
              remainderBounding[1] += gridSize[1];
            }
            while (remainderBounding[1] >= gridSize[1]) {
              offset[1]++;
              remainderBounding[1] -= gridSize[1];
            }

            // If the offset is non-zero, we issue an offset event.
            if (offset[0] !== 0 || offset[1] !== 0) onOffset(offset);

            // Return the new remainder.
            return remainderBounding;
          });

          // Update the mouse position.
          return [event.clientX, event.clientY];
        }
      });
    };

    // Setup and teardown for event listeners.
    window.addEventListener("mousedown", handleMouseDown);
    window.addEventListener("mouseup", handleMouseUp);
    window.addEventListener("mousemove", handleMouseMove);
    return () => {
      window.removeEventListener("mousedown", handleMouseDown);
      window.removeEventListener("mouseup", handleMouseUp);
      window.removeEventListener("mousemove", handleMouseMove);
    };
  }, [gridSize, onOffset]);

  return (
    <div ref={element} className={classNames(styles.handle, className)}>
      {children}
    </div>
  );
};

/** The props used for the {@link TileEdgeHandle} component. */
interface TileEdgeHandleProps extends ComponentProps<"div"> {
  /** The side on which the edge handle should be attached to. */
  side: "top" | "right" | "bottom" | "left";

  /** An event listener that is called when the handle is offset via dragging. */
  onOffset?: (offset: [number, number]) => void;
}
/** A component that maintains a tile edge handle. */
const TileEdgeHandle: FC<TileEdgeHandleProps> = ({ side, onOffset }) => {
  return (
    <TileHandle
      className={classNames(styles.handleEdge, styles[side])}
      onOffset={onOffset}
    />
  );
};

/** The props used for the {@link Tile} component. */
interface TileProps {
  /** The position on the grid that the tile begins the upper-left corner at. */
  position: [number, number];
  /** The dimensions on the grid of the tile. */
  dimensions: [number, number];

  /** Whether the tile should be able to be resized. */
  resizeable?: boolean;

  /** The event listener that is called when the position or dimension of the time has changed. */
  onLayoutChanged?: (
    position: [number, number],
    dimensions: [number, number]
  ) => void;
}

interface TileComposition {
  Handle: typeof TileHandle;
}

const Tile: FC<TileProps> & TileComposition = ({
  position,
  dimensions,
  resizeable,
  onLayoutChanged = () => {},
  children,
}) => {
  const { gridSize, gridPosition } = useMosaic();

  return (
    <div
      className={styles.tile}
      style={{
        ["--grid-x" as any]: position[0] + gridPosition[0] / gridSize[0],
        ["--grid-y" as any]: position[1] + gridPosition[1] / gridSize[1],
        ["--grid-width" as any]: dimensions[0],
        ["--grid-height" as any]: dimensions[1],
      }}
    >
      {resizeable && (
        <Fragment>
          <TileEdgeHandle
            side="left"
            onOffset={([x, y]) => {
              onLayoutChanged(
                [position[0] + x, position[1]],
                [dimensions[0] - x, dimensions[1]]
              );
            }}
          />
          <TileEdgeHandle
            side="right"
            onOffset={([x, y]) => {
              onLayoutChanged(position, [dimensions[0] + x, dimensions[1]]);
            }}
          />
          <TileEdgeHandle
            side="top"
            onOffset={([x, y]) => {
              onLayoutChanged(
                [position[0], position[1] + y],
                [dimensions[0], dimensions[1] - y]
              );
            }}
          />
          <TileEdgeHandle
            side="bottom"
            onOffset={([x, y]) => {
              onLayoutChanged(position, [dimensions[0], dimensions[1] + y]);
            }}
          />
        </Fragment>
      )}
      {children}
    </div>
  );
};
Tile.Handle = TileHandle;

export default Tile;
export type { ITile, TileProps };
