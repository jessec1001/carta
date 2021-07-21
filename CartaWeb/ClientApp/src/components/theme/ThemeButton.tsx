import { FunctionComponent, useContext } from "react";
import { Theme, ThemeContext } from "context";

/** A theme button component that cycles through themes when clicked. */
const ThemeButton: FunctionComponent = () => {
  const { theme, setTheme } = useContext(ThemeContext);

  // Compute the next theme in the cycle..
  const themes = Object.values(Theme);
  const themeIndexPrev = themes.indexOf(theme);
  const themeIndexNext = (themeIndexPrev + 1) % themes.length;
  const themeNext = themes[themeIndexNext];

  // Whenever the theme button is clicked, we cycle through the themes in order.
  const handleClick = () => setTheme(themeNext);

  return (
    <button
      onClick={handleClick}
      className={`theme-button theme-${themeNext}`}
    />
  );
};

export default ThemeButton;
