import { FC, useContext } from "react";
import { Theme, ThemeContext } from "components/theme";

/** An SVG icon for the Carta platform. */
const CartaIcon: FC = () => {
  const { theme } = useContext(ThemeContext);

  // Return an appropriate icon image based on theming.
  switch (theme) {
    case Theme.Light:
      return (
        <img
          src="images/carta-light.svg"
          alt="Carta"
          style={{ height: "2rem" }}
        />
      );
    case Theme.Dark:
      return (
        <img
          src="images/carta-dark.svg"
          alt="Carta"
          style={{ height: "2rem" }}
        />
      );
  }
};

export default CartaIcon;
