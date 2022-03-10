import { createContext, useContext } from "react";

/** The type of value used for the {@link MosaicContext} */
interface IMosaicContext {
  /** The current grid size of the mosaic in pixels. */
  gridSize: [number, number];
}

/** The context used to expose information about the {@link Mosaic} component. */
const MosaicContext = createContext<IMosaicContext | undefined>(undefined);

/**
 * Returns an object that allows for determining the state of the mosaic along with actions that allow changing the
 * state of the mosaic.
 * @returns The state along with state-mutating actions.
 */
const useMosaic = (): IMosaicContext => {
  const context = useContext(MosaicContext);
  if (context === undefined) {
    throw new Error("MosaicContext is undefined");
  }
  return context;
};

export default MosaicContext;
export { useMosaic };
export type { IMosaicContext };
