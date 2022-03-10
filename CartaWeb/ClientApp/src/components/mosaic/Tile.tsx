import React, { ComponentProps, FC, useEffect, useRef, useState } from "react";
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

const TileHandle: FC<TileHandleProps> = ({ onOffset = () => {}, children }) => {
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
        if (event.target && element.current.contains(event.target as Node))
          setMousePosition([event.clientX, event.clientY]);
      }
    };
    const handleMouseUp = () => {
      // Once the mouse is released, we reset the mouse position.
      setMousePosition(null);
      setMouseRemainder([gridSize[0] / 2, gridSize[1] / 2]);
    };
    const handleMouseMove = (event: MouseEvent) => {
      // Whenever the mouse is moved, we issue an offset event if we have moved more than a grid cell.
      setMousePosition((position) => {
        if (!position || !element.current) return position;
        else {
          // Update the mouse remainder.
          const movement = [
            event.clientX - position[0],
            event.clientY - position[1],
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
    <div ref={element} className={styles.handle}>
      {children}
    </div>
  );
};

/** The props used for the {@link Tile} component. */
interface TileProps {
  /** The position on the grid that the tile begins the upper-left corner at. */
  position?: [number, number];
  /** The dimensions on the grid of the tile. */
  dimensions?: [number, number];

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
  children,
}) => {
  return (
    <div
      className={styles.tile}
      style={{
        ["--grid-x" as any]: position?.[0] ?? 0,
        ["--grid-y" as any]: position?.[1] ?? 0,
        ["--grid-width" as any]: dimensions?.[0] ?? 1,
        ["--grid-height" as any]: dimensions?.[1] ?? 1,
      }}
    >
      {children}
    </div>
  );
};
Tile.Handle = TileHandle;

export default Tile;
export type { ITile, TileProps };
