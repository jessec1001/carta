import { FunctionComponent, useEffect, useRef } from "react";
import {
  Options as VisNetworkOptions,
  Network as VisNetwork,
  Node as VisNode,
  Edge as VisEdge,
} from "vis-network";

interface VisNetworkRendererProps {
  options?: VisNetworkOptions;
}

const VisNetworkRenderer: FunctionComponent<VisNetworkRendererProps> = ({
  options,
}) => {
  const container = useRef<HTMLDivElement>(null);
  const network = useRef<VisNetwork | null>(null);

  // We need to create the network after our container element has been constructed.
  useEffect(() => {
    if (container.current === null) return;

    const numClusters = Math.floor(5 * Math.random() + 5);

    const nodes: VisNode[] = [{ id: "root", fixed: true, color: "gray" }];
    const edges: VisEdge[] = [];
    for (let k = 0; k < numClusters; k++) {
      const numNodes = Math.floor(5 * Math.random() + 5);
      const id = `${k}`;
      const color = `hsl(${360 * (k / numClusters)}, 100%, 50%)`;
      nodes.push({ id, color });
      edges.push({ from: "root", to: id });
      for (let n = 0; n < numNodes; n++) {
        const id = `${k}-${n}`;
        const color = `hsl(${
          360 * (k / numClusters + n / numClusters / numNodes)
        }, 100%, 50%)`;
        nodes.push({ id, color });
        edges.push({ from: `${k}`, to: id });
      }
    }

    network.current = new VisNetwork(container.current, { nodes, edges });
    network.current.focus("root");
  }, []);

  // When the network options are changed, we reset those options on the network.
  useEffect(() => {
    if (network.current === null) return;
    network.current.setOptions(options ?? {});
    network.current.focus("root", {
      locked: true,
      scale: 1,
    });
  }, [options]);

  /*
    We render only a container element for the network.
    VisJS Network handles the construction of the canvas and its internal elements.
  */
  return (
    <div
      className="vis-network-container"
      style={{
        position: "relative",
        width: "100%",
        height: "100%",
      }}
      ref={container}
    />
  );
};

export default VisNetworkRenderer;
export type { VisNetworkRendererProps };
