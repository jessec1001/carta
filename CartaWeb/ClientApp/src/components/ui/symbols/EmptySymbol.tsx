import { FunctionComponent } from "react";

/** A component that displays an empty value symbol. */
const EmptySymbol: FunctionComponent = () => {
  return <span style={{ color: "var(--color-empty)" }}>EMPTY</span>;
};

export default EmptySymbol;
