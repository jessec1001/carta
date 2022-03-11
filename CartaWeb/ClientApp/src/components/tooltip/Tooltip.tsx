import { ComponentProps, FC, Fragment, ReactNode, useState } from "react";
import { usePopper } from "react-popper";
import styles from "./Tooltip.module.css";

// We need to get the type of the options from the Popper hook.
type PopperOptions = typeof usePopper extends (
  e: any,
  p: any,
  o: infer TOptions
) => any
  ? TOptions
  : never;

/** The props used for the {@link Tooltip} component. */
interface TooltipProps extends ComponentProps<"div"> {
  /** The component that should be rendered */
  component: ReactNode;

  /** The options for the Popper component */
  options?: PopperOptions;

  /** Whether the tooltip needs to be hovered over in order to display. */
  hover?: boolean;
  /** Whether the tooltip should be hidden. */
  hide?: boolean;
}
/** A component that displays a tooltip. */
const Tooltip: FC<TooltipProps> = ({
  component,
  options,
  hover,
  children,
  ...props
}) => {
  // We only want to show the tooltip when the mouse is over the component.
  const [show, setShow] = useState(!hover);

  // We create a tooltip for the header portion of the node.
  const [elementRef, setElementRef] = useState<HTMLDivElement | null>(null);
  const [popperRef, setPopperRef] = useState<HTMLDivElement | null>(null);
  const [arrowRef, setArrowRef] = useState<HTMLDivElement | null>(null);
  const { styles: tipStyles, attributes: tipAttributes } = usePopper(
    elementRef,
    popperRef,
    {
      modifiers: [
        ...(options?.modifiers ?? []),
        { name: "arrow", options: { element: arrowRef } },
        { name: "offset", options: { offset: [0, 12] } },
      ],
      ...options,
    }
  );

  return (
    <Fragment>
      <div
        ref={setElementRef}
        onMouseEnter={() => setShow(!hover || true)}
        onMouseLeave={() => setShow(!hover || false)}
        {...props}
      >
        {children}
      </div>
      {show && (
        <div
          ref={setPopperRef}
          className={styles.tooltip}
          style={tipStyles.popper}
          {...tipAttributes.popper}
        >
          {component}
          <div
            ref={setArrowRef}
            className={styles.arrow}
            style={tipStyles.arrow}
          />
        </div>
      )}
    </Fragment>
  );
};

export default Tooltip;
