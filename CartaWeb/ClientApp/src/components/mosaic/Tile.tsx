import { FC } from "react";
import styles from "./Tile.module.css";

// TODO: Draggable edges and corners for adjusting size.

const TileHandle: FC = ({ children }) => {
  return <div className={styles.handle}>{children}</div>;
};

interface TileProps {
  id: string | number;

  position?: [number | null, number | null];
  dimensions?: [number | null, number | null];
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
export type { TileProps };
