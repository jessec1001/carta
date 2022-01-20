import { FunctionComponent, useEffect } from "react";
import { useStoredState } from "hooks";
import { Theme, ThemeContext } from "components/theme";

/** A component that wraps a theme context around its children components. */
const ThemeWrapper: FunctionComponent = ({ children }) => {
  // Store the theme in storage so it persists across sessions.
  // This also synchronizes across windows.
  const [theme, setTheme] = useStoredState(Theme.Light, "theme");

  // Whenever the theme changes, we need to apply a specific class to the body.
  // This should change the appearance of the entire page.
  useEffect(() => {
    const body = document.body;
    body.classList.add(`theme-${theme}`);
    return () => body.classList.remove(`theme-${theme}`);
  }, [theme]);

  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};

export default ThemeWrapper;
