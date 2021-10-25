import { ComponentProps, FunctionComponent } from "react";
import IconButton from "./IconButton";

/** An icon-based button that represents removing or deleting an object. */
const IconButtonRemove: FunctionComponent<ComponentProps<typeof IconButton>> =
  ({ style, ...props }) => {
    return (
      <IconButton style={{ color: "#d83c3c", ...style }} {...props}>
        ×
      </IconButton>
    );
  };

export default IconButtonRemove;
