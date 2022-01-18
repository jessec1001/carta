import * as d3 from "d3";
import * as three from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import { Plot, Plotter } from "types";

// #region Utility functions
/**
 * Creates an SVG element and attaches it to the specified container based on plot data.
 * @param container The container to attach the plot to.
 * @param plot The data of the plot to generate.
 * @returns The SVG element that was created.
 */
const createSvg = <TPlot extends Plot>(container: HTMLElement, plot: TPlot) => {
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
    .attr("viewBox", `0 0 ${size.width} ${size.height}`)
    .attr("style", style);

  // Return the SVG element and relevant computed information.
  return { svg, size };
};
/**
 * Finds a colormap specified by name.
 * @param name The name of the colormap.
 * @returns The colormap.
 */
const findColormap = (name?: string) => {
  // We use the names of schemes and interpolates directly from D3 to make a mapping from colormap names to colormap
  // ranges. We also add some additonal mappings for more sensible naming conventions especially for creating
  // enumerations.
  const schemesOrdinal: Record<string, readonly string[]> = {
    category: d3.schemeCategory10,
    category10: d3.schemeCategory10,
    accent: d3.schemeAccent,
    dark: d3.schemeDark2,
    dark1: d3.schemeDark2,
    dark2: d3.schemeDark2,
    paired: d3.schemePaired,
    pastel: d3.schemePastel1,
    pastel1: d3.schemePastel1,
    pastel2: d3.schemePastel2,
    set1: d3.schemeSet1,
    set2: d3.schemeSet2,
    set3: d3.schemeSet3,
    tableau: d3.schemeTableau10,
    tableau10: d3.schemeTableau10,
  };
  const schemesHues: Record<string, readonly (readonly string[])[]> = {
    // Diverging.
    BrBG: d3.schemeBrBG,
    PRGn: d3.schemePRGn,
    PiYG: d3.schemePiYG,
    PuOr: d3.schemePuOr,
    RdBu: d3.schemeRdBu,
    RdGy: d3.schemeRdGy,
    RdYlBu: d3.schemeRdYlBu,
    RdYlGn: d3.schemeRdYlGn,
    spectral: d3.schemeSpectral,

    // Non-diverging.
    BuGn: d3.schemeBuGn,
    BuPu: d3.schemeBuPu,
    GnBu: d3.schemeGnBu,
    OrRd: d3.schemeOrRd,
    PuBuGn: d3.schemePuBuGn,
    PuBu: d3.schemePuBu,
    PuRd: d3.schemePuRd,
    RdPu: d3.schemeRdPu,
    YlGnBu: d3.schemeYlGnBu,
    YlGn: d3.schemeYlGn,
    YlOrBr: d3.schemeYlOrBr,
    YlOrRd: d3.schemeYlOrRd,

    // Single hue.
    blues: d3.schemeBlues,
    greens: d3.schemeGreens,
    greys: d3.schemeGreys,
    oranges: d3.schemeOranges,
    purples: d3.schemePurples,
    reds: d3.schemeReds,
  };

  const interpolates: Record<string, (t: number) => string> = {
    // Diverging.
    BrBG: d3.interpolateBrBG,
    PRGn: d3.interpolatePRGn,
    PiYG: d3.interpolatePiYG,
    PuOr: d3.interpolatePuOr,
    RdBu: d3.interpolateRdBu,
    RdGy: d3.interpolateRdGy,
    RdYlBu: d3.interpolateRdYlBu,
    RdYlGn: d3.interpolateRdYlGn,
    spectral: d3.interpolateSpectral,

    // Non-diverging.
    BuGn: d3.interpolateBuGn,
    BuPu: d3.interpolateBuPu,
    GnBu: d3.interpolateGnBu,
    OrRd: d3.interpolateOrRd,
    PuBuGn: d3.interpolatePuBuGn,
    PuBu: d3.interpolatePuBu,
    PuRd: d3.interpolatePuRd,
    RdPu: d3.interpolateRdPu,
    YlGnBu: d3.interpolateYlGnBu,
    YlGn: d3.interpolateYlGn,
    YlOrBr: d3.interpolateYlOrBr,
    YlOrRd: d3.interpolateYlOrRd,

    // Single hue.
    blues: d3.interpolateBlues,
    greens: d3.interpolateGreens,
    greys: d3.interpolateGreys,
    oranges: d3.interpolateOranges,
    purples: d3.interpolatePurples,
    reds: d3.interpolateReds,

    // Multiple hue.
    turbo: d3.interpolateTurbo,
    viridis: d3.interpolateViridis,
    inferno: d3.interpolateInferno,
    magma: d3.interpolateMagma,
    plasma: d3.interpolatePlasma,
    cividis: d3.interpolateCividis,
    warm: d3.interpolateWarm,
    cool: d3.interpolateCool,
    cubehelix: d3.interpolateCubehelixDefault,
    cubehelixDefault: d3.interpolateCubehelixDefault,
    rainbow: d3.interpolateRainbow,
    sinebow: d3.interpolateSinebow,
  };

  // Ordinal color schemes do not support indices.
  // Diverging color schemes support indices in [3, 11].
  // Single hue color schemes support indices in [3, 9].
  // Multiple hue color schemes support indices in [3, 9].
  const defaultColors: readonly string[] = ["#000", "#000"];
  if (!name) return d3.scaleSequential(defaultColors);

  // Try categorical.
  if (name in schemesOrdinal)
    return d3.scaleOrdinal<number, string>(schemesOrdinal[name]);

  // Try interpolate to categorical.
  const match = name.match(/([a-zA-Z]+)(\d+)/);
  if (match !== null) {
    const index = parseInt(match[1]);
    name = match[0];

    // If we cannot find a categorical scheme with the correct number of items, we use a interpolative scheme.
    // Ideally, these should work identically in most circumstances.
    if (name in schemesHues) {
      const scheme = schemesHues[name][index];
      if (scheme)
        return d3.scaleOrdinal<number, string>(schemesHues[name][index]);
    } else {
      return d3.scaleSequential(interpolates[name]);
    }
  }

  // Try interpolate.
  if (name in interpolates) return d3.scaleSequential(interpolates[name]);

  // Default.
  return d3.scaleSequential(defaultColors);
};
// #endregion

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
  zoomElement
    .append("g")
    .attr("transform", `translate(0, ${size.height - margin.bottom})`)
    .call(d3.axisBottom(scaleX));
  zoomElement
    .append("g")
    .attr("transform", `translate(${margin.left}, 0)`)
    .call(d3.axisLeft(scaleY));

  // Get the colormap that is used for this plot.
  const extentColor = d3.extent(plot.data, (value) => value.value);
  let scaleColor = findColormap(plot.colormap);
  if (extentColor[0] !== undefined && extentColor[1] !== undefined)
    scaleColor.domain(extentColor as [number, number]);

  const zoom = d3.zoom<SVGSVGElement, unknown>().on("zoom", (event) => {
    zoomElement.attr("transform", event.transform);
  });
  svg.call(zoom).call(zoom.transform, d3.zoomIdentity);

  // Create the actual scatter plot elements as a specific shape.
  zoomElement
    .append("g")
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
