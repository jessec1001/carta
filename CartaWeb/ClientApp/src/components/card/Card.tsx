import { ComponentProps, FC } from "react";
import classNames from "classnames";
import Header from "./Header";
import Footer from "./Footer";
import Body from "./Body";
import styles from "./Card.module.css";

/** The props used for the {@link Card} component. */
interface CardProps extends ComponentProps<"div"> {}

/**
 * Defines the composition of the compound {@link Card} component.
 * @borrows Header as Header
 * @borrows Footer as Footer
 * @borrows Body as Body
 */
interface CardComposition {
  Header: typeof Header;
  Footer: typeof Footer;
  Body: typeof Body;
}

/**
 * A component that renders a card element. The card element is a container that can be used to group other elements.
 * @example
 * ```jsx
 * <Card>
 *  <Card.Header>Card Header</Card.Header>
 *  <Card.Body>Card Body</Card.Body>
 *  <Card.Footer>Card Footer</Card.Footer>
 * </Card>
 * ```
 */
const Card: FC<CardProps> & CardComposition = ({
  children,
  className,
  ...props
}) => {
  return (
    <div className={classNames(styles.card, className)} {...props}>
      {children}
    </div>
  );
};
Card.Header = Header;
Card.Footer = Footer;
Card.Body = Body;

export default Card;
