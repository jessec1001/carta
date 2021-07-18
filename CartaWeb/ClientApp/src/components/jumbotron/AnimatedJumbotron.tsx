import {
  FunctionComponent,
  useCallback,
  useEffect,
  useRef,
  useState,
} from "react";
import { Network as VisNetwork } from "vis-network";
import { SyntheticClusters } from "library/graph/data";
import { VisNetworkRenderer } from "components/wrappers";
import "./jumbotron.css";

/** A component that renders a swirling graph network as a jumbotron backdrop. */
const AnimatedJumbotron: FunctionComponent = ({ children }) => {
  // The period for the wind rotation is 15 seconds.
  const windPeriod = 15000;
  // The magnitude of the wind in 0.25 units/second.
  const windMagnitude = 0.25;
  // Update the wind rotation every 0.1 seconds.
  const windUpdateInterval = 100;

  // We initialize the data once.
  // We update the wind angle over time.
  const data = useRef(new SyntheticClusters({}));
  const network = useRef<VisNetwork | null>(null);
  const [angle, setAngle] = useState(0);

  // Update the angle of the wind by the specified amount on an interval.
  useEffect(() => {
    const updateAnimation = () => {
      // Update the wind angle.
      setAngle(
        (prevAngle) =>
          prevAngle + (2 * Math.PI) / (windPeriod / windUpdateInterval)
      );

      // Make sure the animation is going and focused correctly.
      network.current?.startSimulation();
      network.current?.moveNode("root", 0, 0);
      network.current?.focus("root", {
        locked: true,
        scale: 1.0,
      });
    };
    updateAnimation();

    const intervalId = setInterval(updateAnimation, windUpdateInterval);
    return () => clearInterval(intervalId);
  }, []);

  // We calculate the wind as a rotation based on the updating angle.
  const wind = {
    x: windMagnitude * Math.cos(angle),
    y: windMagnitude * Math.sin(angle),
  };

  // We need to start the simulation when the network is initially created.
  // Otherwise, some unknown circumstances can cause the graph to not update.
  const handleCreateNetwork = useCallback((visNetwork: VisNetwork) => {
    network.current = visNetwork;
  }, []);

  return (
    <div className="jumbotron">
      {/* The backdrop is a swirling graph network. */}
      <div className="jumbotron-backdrop">
        <VisNetworkRenderer
          data={data.current}
          options={{ physics: { wind } }}
          onCreateNetwork={handleCreateNetwork}
        />
      </div>

      {/* The content is whatever is passed into the component. */}
      <div className="jumbotron-content">{children}</div>
    </div>
  );
};

export default AnimatedJumbotron;
