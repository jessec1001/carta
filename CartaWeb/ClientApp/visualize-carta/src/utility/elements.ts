import * as d3 from "d3";
import { Plot } from "types";

/**
 * Creates an SVG element and attaches it to the specified container based on plot data.
 * @param container The container to attach the plot to.
 * @param plot The data of the plot to generate.
 * @param centered Whether the viewport of the plot should be centered on the data.
 * @returns The SVG element that was created.
 */
const createSvg = <TPlot extends Plot>(
  container: HTMLElement,
  plot: TPlot,
  centered?: boolean
) => {
  // We define some default values for the SVG element.
  const defaultSize = { width: 800, height: 600 };

  // We find the size and style of the container.
  const size = { ...defaultSize, ...plot.size };
  const style = plot.style
    ? Object.entries(plot.style)
        .map(([key, value]) => `${key}: ${value};`)
        .join(" ")
    : "";

  // Construct the SVG element.
  const svg = d3
    .select(container)
    .append("svg")
    .attr(
      "viewBox",
      centered
        ? `${-size.width / 2} ${-size.height / 2} ${size.width} ${size.height}`
        : `0 0 ${size.width} ${size.height}`
    )
    .attr("style", style);

  // Return the SVG element and relevant computed information.
  return { svg, size };
};

export { createSvg };
