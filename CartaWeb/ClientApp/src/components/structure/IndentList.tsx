import React, { FunctionComponent } from "react";
import "./structure.css";

/** A component that lists children elements and indents them all. */
const IndentList: FunctionComponent = ({ children, ...props }) => {
  return (
    <ul className="indent-list" {...props}>
      {React.Children.map(children, (child, index) => (
        <li className="indent-list-item" key={index}>
          {child}
        </li>
      ))}
    </ul>
  );
};

export default IndentList;
