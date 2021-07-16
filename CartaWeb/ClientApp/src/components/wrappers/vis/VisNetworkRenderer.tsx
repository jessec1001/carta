import { FunctionComponent, useEffect, useLayoutEffect, useRef } from "react";
import {
  Options as VisNetworkOptions,
  Network as VisNetwork,
  Data as VisData,
} from "vis-network";
import "./vis.css";

/** The props used for the {@link VisNetworkRenderer} component. */
interface VisNetworkRendererProps {
  /** The graph data that should be rendered. */
  data: VisData;
  /** The options for the renderer. */
  options?: VisNetworkOptions;

  /** Called when the graph network is initially created. */
  onCreateNetwork?: (network: VisNetwork) => void;
  /** Called when the graph network is destroyed. */
  onDestroyNetwork?: () => void;
}

/** A component that renders a graph network using VisJS. */
const VisNetworkRenderer: FunctionComponent<VisNetworkRendererProps> = ({
  data,
  options,
  onCreateNetwork,
  onDestroyNetwork,
}) => {
  const container = useRef<HTMLDivElement>(null);
  const network = useRef<VisNetwork | null>(null);

  // To efficiently update props, we use a reference to store the most recent data and events values.
  const localData = useRef(data);

  // The network needs to be created and destroyed on mount and unmount respectively.
  useLayoutEffect(() => {
    // Construct the VisJS Network.
    if (container.current === null) return;
    network.current = new VisNetwork(container.current, localData.current);
    if (onCreateNetwork) onCreateNetwork(network.current);

    // Destroy the VisJS Network.
    return () => {
      if (network.current === null) return;
      network.current.destroy();
      network.current = null;
      if (onDestroyNetwork) onDestroyNetwork();
    };
  }, [onCreateNetwork, onDestroyNetwork]);

  // When the network data are changed, we reset the data on the network.
  useEffect(() => {
    if (localData.current !== data) {
      localData.current = data;
      network.current?.setData(data);
    }
  }, [data]);

  // When the network options are changed, we reset those options on the network.
  useEffect(() => {
    network.current?.setOptions(options ?? {});
  }, [options]);

  /*
    We render only a container element for the network.
    VisJS Network handles the construction of the canvas and its internal elements.
  */
  return <div className="vis-network-container" ref={container} />;
};

export default VisNetworkRenderer;
export type { VisNetworkRendererProps };
