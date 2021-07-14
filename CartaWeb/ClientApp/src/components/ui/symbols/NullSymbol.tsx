import { FunctionComponent } from "react";

/** A component that displays a null-value symbol. */
const NullSymbol: FunctionComponent = () => {
  return <span style={{ color: "var(--color-null)" }}>NULL</span>;
};

export default NullSymbol;
