import { FunctionComponent } from "react";
import { LoadingIcon } from "components/icons";
import Text, { TextProps } from "./Text";

/** The props used for the {@link Loading} component. */
interface LoadingProps extends TextProps {
  /** Whether the loading icon should be animated. */
  animated?: boolean;
}

/** A component that renders some loading text accompanied by an animated loading symbol. */
const Loading: FunctionComponent<LoadingProps> = ({ children, ...props }) => {
  return (
    <Text align="middle" {...props}>
      {children} <LoadingIcon animated padded />
    </Text>
  );
};

export default Loading;
