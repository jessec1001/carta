import { FunctionComponent, useEffect, useState } from "react";
import { VisNetworkRenderer } from "components/wrappers";

const AnimatedJumbotron: FunctionComponent = ({ children }) => {
  const [angle, setAngle] = useState(0);

  useEffect(() => {
    const intervalId = setInterval(() => {
      setAngle((prevAngle) => prevAngle + (2 * Math.PI) / 10 / 15);
    }, 100);
    return () => clearInterval(intervalId);
  }, []);

  return (
    <div
      style={{
        position: "relative",
        boxShadow: "inset 0px 0px 4px 4px rgba(0, 0, 0, 0.25)",
      }}
    >
      <div
        style={{
          position: "absolute",
          width: "100%",
          height: "100%",
          zIndex: -1,
        }}
      >
        <VisNetworkRenderer
          options={{
            physics: {
              wind: {
                x: 0.25 * Math.cos(angle),
                y: 0.25 * Math.sin(angle),
              },
            },
          }}
        />
      </div>
      <div
        style={{
          backdropFilter: "blur(2px)",
          backgroundColor: "rgba(255, 255, 255, 0.5)",
        }}
      >
        {children}
      </div>
    </div>
  );
};

export default AnimatedJumbotron;
