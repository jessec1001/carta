import React, { FunctionComponent } from "react";

/* A component that lists navigation links. */
const NavigationLinkList: FunctionComponent = ({ children }) => {
  return (
    <nav className="nav-link-list">
      <ul role="navigation">
        {React.Children.map(children, (child, index) => (
          <li key={index}>{child}</li>
        ))}
      </ul>
    </nav>
  );
};

export default NavigationLinkList;
