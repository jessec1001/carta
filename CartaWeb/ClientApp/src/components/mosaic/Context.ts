import { createContext } from "react";

/** The type of value used for the {@link MosaicContext} */
interface IMosaicContext {
  /** The current grid size of the mosaic in pixels. */
  gridSize: [number, number];
}

const MosaicContext = createContext<IMosaicContext | undefined>(undefined);

const useMosaic = () => {};

export default MosaicContext;
