import { FC, useEffect, useRef, useState } from "react";
import { Job } from "library/api";
import {
  ScatterPlot2d,
  ScatterPlot3d,
  IScatterPlotEvents,
  EventDriver,
  IScatterPoint3d,
  IScatterPoint2d,
} from "visualize-carta";
import { useViews } from "components/views";

// #region API-defined Structures
/** The structure of a visualization point retrieved from the API. */
interface IVisualizePoint {
  /** The x-coordinate of the point. */
  x?: number;
  /** The y-coordinate of the point. */
  y?: number;
  /** The z-coordinate of the point. */
  z?: number;

  /** The radius of the point. */
  radius?: number;
  /** The color value of the point. */
  value?: number;
}
/** The structure of a visualization retrieved from the API. */
interface IVisualizeScatter {
  /** The discrimant type of the plot. */
  type: "scatter";

  /** The points assigned to the scatter. */
  data: IVisualizePoint[];
  /** The number of dimensions for the scatter. */
  dimensions: 2 | 3;
  /** The colormap for the scatter. */
  colormap?: string;
}
// #endregion

interface ScatterVisualizerProps {
  /** The field name of the scatter visualization. */
  field: string;
  /** The base path to fetch scatter information from. */
  path: string;
  /** The container whether the plot should be visualized. */
  container: HTMLElement | null;
}
const ScatterVisualizer: FC<ScatterVisualizerProps> = ({
  field,
  path,
  container,
}) => {
  // We store a reference to the plot so that we can update it when the data changes.
  // Notice that we assume a 2D scatter plot until we initially receive the data.
  const plotRef = useRef<
    (ScatterPlot2d | ScatterPlot3d) & EventDriver<IScatterPlotEvents>
  >(new ScatterPlot2d() as any);

  // Construct the scatter plot.
  useEffect(() => {
    if (container) {
      container.innerHTML = "";
      plotRef.current.container = container;
      plotRef.current.render();
    }
  }, [container]);

  // We store the data in state.
  const [data, setData] = useState<{
    data: Map<string, IVisualizePoint>;
    colormap?: string;
  }>({ data: new Map() });

  // We keep a reference to the size of the element.
  const [size, setSize] = useState<[number, number]>([0, 0]);
  useEffect(() => {
    if (!container) return;
    const handleChangeSize = () => {
      setSize((size) => {
        const boundingRect = container.getBoundingClientRect();
        if (size[0] === boundingRect.width && size[1] === boundingRect.height)
          return size;
        else return [boundingRect.width, boundingRect.height];
      });
    };

    const interval = setInterval(handleChangeSize, 25);
    return () => clearInterval(interval);
  }, [container]);
  useEffect(() => {
    plotRef.current.layout = {
      ...plotRef.current.layout,
      size: { width: size[0], height: size[1] },
    };
    plotRef.current.render();
  }, [size]);

  // We perform actions on the view state when the user interacts with the plot.
  const {
    viewId,
    actions: { setTag },
  } = useViews();
  useEffect(() => {
    setTag(viewId, "visualization", field);
  }, [field, setTag, viewId]);

  // Whenever the data is updated, update the plot.
  useEffect(() => {
    plotRef.current.data = {
      data: Array.from(data.data).map(([id, point]) => {
        const { x, y, z, ...rest } = point;
        const datum: IScatterPoint2d | IScatterPoint3d = {
          id: id,
          x: x as number,
          y: y as number,
          z: z as number,
          ...rest,
        };
        return datum;
      }),
      colormap: data.colormap,
    };
    plotRef.current.render();
  }, [data]);

  useEffect(() => {
    const updateData = async () => {
      // Fetch the data.
      const response = await fetch(path);
      const data = (await response.json()) as Job<IVisualizeScatter | {}>;
      const scatter = data.result;

      // Check that the plot is well-defined.
      if (!scatter || !("type" in scatter) || scatter.type !== "scatter")
        return;

      // Filter out data that cannot be displayed.
      scatter.data = scatter.data.filter((d) => {
        if (scatter.dimensions >= 1 && d.x === undefined) return false;
        if (scatter.dimensions >= 2 && d.y === undefined) return false;
        if (scatter.dimensions >= 3 && d.z === undefined) return false;
        return true;
      });

      // Check if we need to replace the plot with a different dimensional plot.
      if (
        plotRef.current instanceof ScatterPlot2d &&
        scatter.dimensions === 3
      ) {
        const plot2d = plotRef.current;
        const plot3d = new ScatterPlot3d();

        // Clear the container if it exists.
        if (plot2d.container) plot2d.container.innerHTML = "";

        // Transfer the data and elements.
        plot3d.container = plot2d.container;
        plot3d.layout = plot2d.layout;
      }
      if (
        plotRef.current instanceof ScatterPlot3d &&
        scatter.dimensions === 3
      ) {
        const plot3d = plotRef.current;
        const plot2d = new ScatterPlot2d();

        // Clear the container if it exists.
        if (plot3d.container) plot3d.container.innerHTML = "";

        // Transfer the data and elements.
        plot2d.container = plot3d.container;
        plot2d.layout = plot3d.layout;
      }

      // Set the data.
      setData(() => {
        const points: Map<string, IScatterPoint2d | IScatterPoint3d> = new Map(
          scatter.data.map(({ x, y, z, ...rest }, i) => [
            i.toString(),
            {
              id: i.toString(),
              x: x as number,
              y: y as number,
              z: z as number,
              ...rest,
            },
          ])
        );
        return {
          data: points,
          colormap: scatter.colormap,
        };
      });
    };

    // TODO: Check if the plot actually needs to be updated depending on whether the field was previously complete.
    const interval = setInterval(updateData, 8192);
    updateData();
    return () => clearInterval(interval);
  }, [path]);

  return null;
};

export default ScatterVisualizer;
export type { ScatterVisualizerProps };
