import * as d3 from "d3";
import * as three from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import { Plot, Plotter } from "types";
import { createSvg, findColormap } from "utility";

/** The type of datum for each scatter plot point. */
interface ScatterPlotDatum {
  /** The x-component of the datum. */
  x?: number;
  /** The y-component of the datum. */
  y?: number;
  /** The z-component of the datum. */
  z?: number;

  /** The radius of the datum. Defaults to 1.0 if not specified. */
  radius?: number;
  /** The value of the datum. Defaults to 0.0 if not specified. */
  value?: number;

  /** The optional styles to apply to the datum point. */
  style?: Partial<CSSStyleDeclaration>;
}
/** The type of datum for each scatter plot point with 2D guaranteed. */
interface ScatterPlotDatum2d extends ScatterPlotDatum {
  x: number;
  y: number;
}
/** The type of datum for each scatter plot point with 3D guaranteed. */
interface ScatterPlotDatum3d extends ScatterPlotDatum {
  x: number;
  y: number;
  z: number;
}
/** The type of the combined data for the scatter plot. */
interface ScatterPlot<TDatum extends ScatterPlotDatum = ScatterPlotDatum>
  extends Plot {
  type: "scatter";

  /** The data to display in the plot itself. */
  data: TDatum[];

  /** The colormap to use for mapping values to colors. */
  colormap?: string;
}

/**
 * Creates a 3D scatter plot and attaches it to the specified container.
 * @param container The container to attach the plot to.
 * @param plot The data of the plot to generate.
 * @param events The events to attach to the plot.
 * @returns An updater function to update the plot.
 */
const PlotScatter3d = <TDatum extends ScatterPlotDatum3d>(
  container: HTMLElement,
  plot: ScatterPlot<TDatum>,
  events?: {}
): ((plot: ScatterPlot<TDatum>) => void) => {
  const width = plot.size?.width ?? 800;
  const height = plot.size?.height ?? 640;

  let extentX = d3.extent(plot.data, (d) => d.x) as [number, number];
  let extentY = d3.extent(plot.data, (d) => d.y) as [number, number];
  let extentZ = d3.extent(plot.data, (d) => d.z) as [number, number];
  const camera = new three.PerspectiveCamera(70, width / height, 0.01, 10000);
  camera.lookAt(
    (extentX[0] + extentX[1]) / 2,
    (extentY[0] + extentY[1]) / 2,
    (extentZ[0] + extentZ[1]) / 2
  );
  camera.position.x = (extentX[0] + extentX[1]) / 2;
  camera.position.y = (extentY[0] + extentY[1]) / 2;
  camera.position.z = extentZ[0];
  camera.frustumCulled = false;

  const scene = new three.Scene();
  const material = new three.MeshNormalMaterial();

  // TODO: Render an axis in the scene.
  // TODO: Take into consideration distance-based scaling of radii.
  // TODO: Use the point style or colormap to color the points.
  // TODO: Make the background transparent and apply the axis style.
  for (let k = 0; k < plot.data.length; k++) {
    const geometry = new three.DodecahedronGeometry(plot.data[k].radius, 1);
    geometry.translate(plot.data[k].x, plot.data[k].y, plot.data[k].z);
    const mesh = new three.Mesh(geometry, material);
    mesh.frustumCulled = false;

    scene.add(mesh);
  }

  const renderer = new three.WebGLRenderer({ antialias: true });
  const controls = new OrbitControls(camera, renderer.domElement);
  controls.target.set(
    (extentX[0] + extentX[1]) / 2,
    (extentY[0] + extentY[1]) / 2,
    (extentZ[0] + extentZ[1]) / 2
  );
  controls.enableDamping = true;
  controls.dampingFactor = 0.01;
  controls.rotateSpeed = 0.15;
  renderer.setSize(width, height);
  renderer.setAnimationLoop((time) => {
    controls.update();
    renderer.render(scene, camera);
  });

  container.appendChild(renderer.domElement);
  return () => {};
};
/**
 * Creates a 2D scatter plot and attaches it to the specified container.
 * @param container The container to attach the plot to.
 * @param plot The data of the plot to generate.
 * @param events The events to attach to the plot.
 * @returns An updater function to update the plot.
 */
const PlotScatter2d = <TDatum extends ScatterPlotDatum2d>(
  container: HTMLElement,
  plot: ScatterPlot<TDatum>,
  events?: {}
): ((data: ScatterPlot<TDatum>) => void) => {
  // Create the SVG element.
  const { svg, size } = createSvg(container, plot);

  // TODO: Try to automatically compute margins using canvas context (https://stackoverflow.com/questions/29031659/calculate-width-of-text-before-drawing-the-text).
  //       This requires an additional 'canvas' package. We could also use 'string-pixel-width'.
  // TODO: Abstract this into a set of defaults.
  // Compute the margin of the plot by assigning defaults if not specified.
  const margin = {
    left: 60,
    right: 20,
    top: 20,
    bottom: 40,
    ...plot.margin,
  };

  const zoomElement = svg.append("g");

  // If the plot has no data, simply return here.
  if (!plot.data) return () => {};

  // TODO: Abstract this.
  // TODO: Add labels to the axes.
  // Construct the ranges of values based on the data or ranges specified by the user.
  const extentX = d3.extent(plot.data, (value) => value.x);
  const scaleX = d3
    .scaleLinear()
    .domain([
      plot.axes?.x?.minimum ?? extentX[0] ?? 0,
      plot.axes?.x?.maximum ?? extentX[1] ?? 0,
    ])
    .range([margin.left, size.width - margin.right]);

  const extentY = d3.extent(plot.data, (value) => value.y);
  const scaleY = d3
    .scaleLinear()
    .domain([
      plot.axes?.y?.minimum ?? extentY[0] ?? 0,
      plot.axes?.y?.maximum ?? extentY[1] ?? 0,
    ])
    .range([size.height - margin.bottom, margin.top]);

  // TODO: Abstract this.
  // Use the ranges of the values to create axis elements within the plot.
  const xAxisElement = svg.append("g");
  const yAxisElement = svg.append("g");

  const xAxisCreate = (
    g: d3.Selection<SVGGElement, unknown, null, any>,
    scale: d3.ScaleLinear<number, number, never>
  ) => {
    g.attr("transform", `translate(0, ${size.height - margin.bottom})`).call(
      d3.axisBottom(scale)
    );
  };
  const yAxisCreate = (
    g: d3.Selection<SVGGElement, unknown, null, any>,
    scale: d3.ScaleLinear<number, number, never>
  ) => {
    g.attr("transform", `translate(${margin.left}, 0)`).call(
      d3.axisLeft(scale)
    );
  };

  // Get the colormap that is used for this plot.
  const extentColor = d3.extent(plot.data, (value) => value.value);
  let scaleColor = findColormap(plot.colormap);
  if (extentColor[0] !== undefined && extentColor[1] !== undefined)
    scaleColor.domain(extentColor as [number, number]);

  // Create the actual scatter plot elements as a specific shape.
  const dotsElement = zoomElement.append("g");
  const dots = dotsElement
    .selectAll("dot")
    .data(plot.data)
    .enter()
    .append("circle")
    .attr("cx", (d) => scaleX(d.x))
    .attr("cy", (d) => scaleY(d.y))
    .attr("r", 5)
    .style("fill", (d) =>
      plot.colormap && d.value ? scaleColor(d.value) : "currentColor"
    );

  const zoom = d3.zoom<SVGSVGElement, unknown>().on("zoom", ({ transform }) => {
    const scaleXZoom = transform.rescaleX(scaleX);
    const scaleYZoom = transform.rescaleY(scaleY);

    xAxisCreate(xAxisElement, scaleXZoom);
    yAxisCreate(yAxisElement, scaleYZoom);

    zoomElement.attr("transform", transform);
  });
  svg.call(zoom).call(zoom.transform, d3.zoomIdentity);

  return () => {};
};

/**
 * Creates a scatter plot and attaches it to the specified container.
 * @param container The container to attach the plot to.
 * @param plot The data of the plot to generate.
 * @param events The events to attach to the plot.
 * @returns An updater function to update the plot.
 */
const PlotScatter: Plotter<ScatterPlot, {}> = <TDatum extends ScatterPlotDatum>(
  container: HTMLElement,
  plot: ScatterPlot<TDatum>,
  events?: {}
): ((plot: ScatterPlot<TDatum>) => void) => {
  // Check if the data has all values in each component.
  const allX = plot.data.every((value) => value.x !== undefined);
  const allY = plot.data.every((value) => value.y !== undefined);
  const allZ = plot.data.every((value) => value.z !== undefined);

  // If the data has 3 components, use the 3d scatter plot.
  // If the data has 2 components, use the 2d scatter plot.
  if (allX && allY && allZ) {
    return PlotScatter3d(
      container,
      plot as ScatterPlot<ScatterPlotDatum3d>
    ) as (data: ScatterPlot<TDatum>) => void;
  }
  if (allX && allY) {
    return PlotScatter2d(
      container,
      plot as ScatterPlot<ScatterPlotDatum2d>
    ) as (data: ScatterPlot<TDatum>) => void;
  }

  // Otherwise, throw an error.
  throw new Error(
    "The data does not have all values in each datum. Please specify the data as an array of objects with x, y, and, optionally, z properties."
  );
};

export default PlotScatter;
