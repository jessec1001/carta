import { FunctionComponent } from "react";
import { LoadingIcon } from "components/icons";

/** The props used for the {@link LoadingText} component. */
interface LoadingTextProps {
  /** The text used to indicate that a resource is loading. */
  text?: string;
}

/** A component that renders some loading text accompanied by an animated loading symbol. */
const LoadingText: FunctionComponent<LoadingTextProps> = ({
  text = "Loading",
}) => {
  return (
    <span className="normal-text">
      {text} <LoadingIcon animated padded />
    </span>
  );
};

export default LoadingText;
