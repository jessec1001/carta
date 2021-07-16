import { DataSet } from "vis-data";
import { Data as VisData, Node as VisNode, Edge as VisEdge } from "vis-network";

/** The options for {@link SyntheticClusters} data generation. */
interface SyntheticClustersOptions {
  /** The minimum (inclusive) number of node clusters to generate. */
  minClusters?: number;
  /** The maximum (exclusive) number of node clusters to generate. */
  maxClusters?: number;

  /** The minimum (inclusive) number of nodes per cluster to generate. */
  minClusterNodes?: number;
  /** The maximum (exclusive) number of nodes per cluster to generate. */
  maxClusterNodes?: number;
}

/** A synthetic graph dataset consisting of a random number of clusters of nodes. */
class SyntheticClusters implements VisData {
  nodes: DataSet<VisNode>;
  edges: DataSet<VisEdge>;

  constructor({
    minClusters = 5,
    maxClusters = 10,
    minClusterNodes = 5,
    maxClusterNodes = 10,
  }: SyntheticClustersOptions) {
    const nodes: VisNode[] = [];
    const edges: VisEdge[] = [];

    // Create root node.
    const rootId = "root";
    nodes.push({ id: rootId, fixed: true, color: `hsl(0, 0%, 50%)` });

    // Determine number of clusters.
    const numberClusters = Math.floor(
      (maxClusters - minClusters) * Math.random() + minClusters
    );

    // Construct each cluster.
    for (let cluster = 0; cluster < numberClusters; cluster++) {
      const clusterId = `${cluster}`;
      const clusterHue = 360 * (cluster / numberClusters);
      nodes.push({ id: clusterId, color: `hsl(${clusterHue}, 100%, 50%)` });
      edges.push({ from: rootId, to: clusterId });

      // Determine number of nodes in the cluster.
      const numberClusterNodes = Math.floor(
        (maxClusterNodes - minClusterNodes) * Math.random() + minClusterNodes
      );

      // Construct each node.
      for (
        let clusterNode = 0;
        clusterNode < numberClusterNodes;
        clusterNode++
      ) {
        const nodeId = `${cluster}-${clusterNode}`;
        const nodeHue =
          clusterHue +
          360 * (clusterNode / numberClusterNodes / numberClusters);
        nodes.push({ id: nodeId, color: `hsl(${nodeHue}, 100%, 50%)` });
        edges.push({ from: clusterId, to: nodeId });
      }
    }

    this.nodes = new DataSet(nodes);
    this.edges = new DataSet(edges);
  }
}

export default SyntheticClusters;
