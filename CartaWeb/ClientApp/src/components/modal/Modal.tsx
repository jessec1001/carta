import classNames from "classnames";
import { ComponentProps, FC, useEffect, useRef, useState } from "react";
import styles from "./Modal.module.css";

/** The props used for the {@link Modal} component. */
interface ModalProps extends ComponentProps<"div"> {
  /** Whether the modal is active. */
  active?: boolean;
  /** Whether to blur the background behind the modal. */
  blur?: boolean;
  /** Whether the background should be interactive when the modal is active. */
  uninteractive?: boolean;

  /** An event listener that is called when the modal is closed. */
  onClose?: () => void;
}
/** A component that renders a modal on the page. */
const Modal: FC<ModalProps> = ({
  active = false,
  blur = false,
  uninteractive = false,
  onClose = () => {},
  className,
  children,
  ...props
}) => {
  // We use this to achieve the transition effect.
  const [blurred, setBlurred] = useState<boolean>(false);
  useEffect(() => {
    setBlurred(active && blur);
  }, [active, blur]);

  // When we click outside of the modal, we close it.
  const elementRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const handleClick = (event: MouseEvent) => {
      if (
        active &&
        elementRef.current &&
        !elementRef.current.contains(event.target as Node)
      ) {
        onClose();
      }
    };
    document.addEventListener("click", handleClick);
    return () => document.removeEventListener("click", handleClick);
  }, [active, onClose]);

  return (
    <div
      className={classNames(styles.modalBackdrop, {
        [styles.active]: active,
        [styles.uninteractive]: uninteractive,
        [styles.blur]: blurred,
      })}
    >
      <div
        className={classNames(
          styles.modal,
          { [styles.active]: active },
          className
        )}
        ref={elementRef}
        {...props}
      >
        {children}
      </div>
    </div>
  );
};

export default Modal;
export type { ModalProps };
