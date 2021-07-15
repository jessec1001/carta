import { FunctionComponent } from "react";

/** A component that displays a footer at the bottom of a page. */
const Footer: FunctionComponent = ({ children, ...props }) => {
  return (
    <footer className="footer" {...props}>
      &copy; 2021 Contextualize, LLC
    </footer>
  );
};

export default Footer;
