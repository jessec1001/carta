// TODO: Structure of arrow components:
/*

<Arrows>
  <Arrows.Node id="node1" element={el1} />
  <Arrows.Node id="node2" element={el2} />
  
  <Arrows.Arrow source="node1" target="node2" />
<Arrows>

*/

import { FC } from "react";

const Arrows: FC = ({ children }) => {
  return (
    <>
      <svg>
        <defs>
          <marker
            id="Arrows-arrow"
            viewBox="0 0 10 10"
            refX="1"
            refY="5"
            markerWidth="6"
            markerHeight="6"
            orient="auto"
          >
            <path d="M 0 0 L 10 5 L 0 10 z" />
          </marker>
        </defs>
      </svg>
      {children}
    </>
  );
};

export default Arrows;
