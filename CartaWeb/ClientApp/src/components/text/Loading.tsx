import { FunctionComponent } from "react";
import { LoadingIcon } from "components/icons";
import Text from "./Text";

/** The props used for the {@link Loading} component. */
interface LoadingProps {
  /** The text used to indicate that a resource is loading. */
  text?: string;
}

/** A component that renders some loading text accompanied by an animated loading symbol. */
const Loading: FunctionComponent<LoadingProps> = ({ text = "Loading" }) => {
  return (
    <Text>
      {text} <LoadingIcon animated padded />
    </Text>
  );
};

export default Loading;
