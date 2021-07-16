import { createContext } from "react";

/**
 * Represents the available themes in our application.
 * Each enumeration value represents a theme string used in the styling as the classname `.theme-${theme}`.
 */
enum Theme {
  Light = "light",
  Dark = "dark",
}

/** The type of value of {@link ThemeContext}. */
interface ThemeContextValue {
  /** The current theme of the application. */
  theme: Theme;
  /** Sets the current theme of the application. */
  setTheme: (theme: Theme) => void;
}

/** A context for the theme applied to the website. */
const ThemeContext = createContext<ThemeContextValue>({
  theme: Theme.Light,
  setTheme: () => {},
});

export default ThemeContext;
export { Theme };
export type { ThemeContextValue };
