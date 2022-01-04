import * as d3 from "d3";
import * as three from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";

// TODO: Different shapes of indicators
// TODO: Colorbars?
// TODO: Size of points (should be reflective of range of values: sqrt(area / pi) = sqrt(w h) ~ r)?
// TODO: Legend (classification/colors + shapes)?

/** The various numeric axes for the data. */
type AxisName = "x" | "y" | "z";

/** The expected type of the plot passed into the plotting function. */
interface IPlot<TData> {
  /** The data to display in the plot itself. */
  data: TData;

  /** The size of the plot rectangle. */
  size?: {
    width: number;
    height: number;
  };

  /** The margins inside the plot rectangle between the edges and the plot. */
  margin?: {
    left?: number;
    right?: number;
    top?: number;
    bottom?: number;
  };

  /** The properties of the axes for the scatter plot. */
  axes?: Partial<
    Record<
      AxisName,
      {
        label?: string;
        minimum?: number;
        maximum?: number;
      }
    >
  >;

  /** The key of the data where the color information is located. */
  color?: string;
  /** The colormap to use for mapping values to colors. */
  colormap?: string;
}

/**
 * The data in the axis dimensions of the data are expected to be numbers. Any other specified field may be of any type.
 * For instance, colors may be strings.
 */
type IScatterData = (Record<AxisName, any> & Record<string, any>)[];

const ScatterPlot3d = (container: HTMLElement, plot: IPlot<IScatterData>) => {
  const width = plot.size?.width ?? 800;
  const height = plot.size?.height ?? 640;

  let extentX = d3.extent(plot.data, (d) => d.x);
  let extentY = d3.extent(plot.data, (d) => d.y);
  let extentZ = d3.extent(plot.data, (d) => d.z);
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
};

/**
 * Appends an SVG element containing a generated scatter plot to a container using specified plot data.
 * @param container The container that will have the SVG element plot appended to it.
 * @param plot The scatter plot information.
 */
const ScatterPlot = (container: HTMLElement, plot: IPlot<IScatterData>) => {
  if (plot.axes && plot.axes.x && plot.axes.y && plot.axes.z)
    return ScatterPlot3d(container, plot);

  // TODO: Abstract this into a set of defaults.
  // Compute the size of the plot by assigning defaults if not specified.
  const width = plot.size?.width ?? 800;
  const height = plot.size?.height ?? 640;

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

  // TODO: Abstract this.
  // Create an SVG element with the correct viewbox size.
  const svgElement = d3
    .select(container)
    .append("svg")
    .attr("viewBox", `0 0 ${width} ${height}`);

  // If the plot has no data, simply return here.
  if (!plot.data) return;

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
    .range([margin.left, width - margin.right]);

  const extentY = d3.extent(plot.data, (value) => value.y);
  const scaleY = d3
    .scaleLinear()
    .domain([
      plot.axes?.y?.minimum ?? extentY[0] ?? 0,
      plot.axes?.y?.maximum ?? extentY[1] ?? 0,
    ])
    .range([height - margin.bottom, margin.top]);

  // TODO: Abstract this.
  // Use the ranges of the values to create axis elements within the plot.
  svgElement
    .append("g")
    .attr("transform", `translate(0, ${height - margin.bottom})`)
    .call(d3.axisBottom(scaleX));
  svgElement
    .append("g")
    .attr("transform", `translate(${margin.left}, 0)`)
    .call(d3.axisLeft(scaleY));

  // Get the colormap that is used for this plot.
  const extentColor = d3.extent(
    plot.data,
    (value) => plot.color && value[plot.color]
  );
  let scaleColor = findColormap(plot.colormap);
  if (typeof extentColor[0] === "number" && typeof extentColor[1] === "number")
    scaleColor.domain(extentColor as [number, number]);

  // Create the actual scatter plot elements as a specific shape.
  svgElement
    .append("g")
    .selectAll("dot")
    .data(plot.data)
    .enter()
    .append("circle")
    .attr("cx", (d) => scaleX(d.x))
    .attr("cy", (d) => scaleY(d.y))
    .attr("r", 5)
    .style("fill", (d) => {
      const c = d.color ?? plot.color;
      if (typeof c === "number") {
        return scaleColor(c);
      } else return c;
    });
};

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

export default ScatterPlot;
