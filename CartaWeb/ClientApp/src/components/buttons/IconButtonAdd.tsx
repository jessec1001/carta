import { ComponentProps, FunctionComponent } from "react";
import IconButton from "./IconButton";

/** An icon-based button that represents adding or creating an object. */
const IconButtonAdd: FunctionComponent<ComponentProps<typeof IconButton>> = ({
  style,
  ...props
}) => {
  return (
    <IconButton style={{ color: "#2bb038", ...style }} {...props}>
      +
    </IconButton>
  );
};

export default IconButtonAdd;
