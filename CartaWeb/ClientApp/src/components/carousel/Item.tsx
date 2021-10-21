import { FunctionComponent } from "react";

import "./Carousel.css";

const Item: FunctionComponent = ({ children }) => {
  return <div className="Carousel-Item">{children}</div>;
};

export default Item;
