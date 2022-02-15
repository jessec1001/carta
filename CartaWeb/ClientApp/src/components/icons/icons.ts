/** A style that should be applied to keep SVG icons consistent. */
const SVGStyle = ({ padded, half }: IconProps): React.CSSProperties => {
  return {
    width: half ? "0.5em" : "1.0em",
    height: "1em",
    padding: "0rem",
    margin: padded ? "0rem 0.25rem" : "0rem",
  };
};

/** The props of any general icon component. */
interface IconProps {
  /** Whether the icon should be slightly padded. */
  padded?: boolean;
  /** Whether the icon should be half size. */
  half?: boolean;
}

export { SVGStyle };
export type { IconProps };
