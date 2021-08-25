import { FunctionComponent, HTMLAttributes } from "react";
import Header from "./Header";
import Footer from "./Footer";
import Body from "./Body";

import "./card.css";

interface CardComposition {
  Header: FunctionComponent<any>;
  Footer: FunctionComponent<any>;
  Body: FunctionComponent<any>;
}

const Card: FunctionComponent<HTMLAttributes<HTMLDivElement>> &
  CardComposition = ({ children, ...props }) => {
  return (
    <div className="card" {...props}>
      {children}
    </div>
  );
};
Card.Header = Header;
Card.Footer = Footer;
Card.Body = Body;

export default Card;
