import { FC, useEffect, useMemo, useRef, useState } from "react";
import { ScatterPlot, HistogramPlot, GraphPlot } from "visualize-carta";
import { Text } from "components/text";
import { VisualizeIcon } from "components/icons";
import { Views } from "components/views";
import { useMounted } from "hooks";

/** The props used for the {@link VisualizerView} component. */
interface VisualizerViewProps {
  type: string;
  name: string;

  /** A function that is called to attach a listener for updates to the visualization data. */
  onUpdate?: (fn: null | ((data: any) => void)) => void;
  onClose?: () => void;
}

/** A view that displays a visualizer opened in a workflow editor. */
const VisualizerView: FC<VisualizerViewProps> = ({
  type,
  name,
  onUpdate = () => {},
  onClose = () => {},
}) => {
  const [data, setData] = useState<any>(null);
  const ref = useRef<HTMLDivElement>(null);
  const mounted = useMounted();

  const [width, setWidth] = useState(0);
  const [height, setHeight] = useState(0);

  useEffect(() => {
    const intervalId = setInterval(() => {
      if (ref.current) {
        const { width, height } = ref.current.getBoundingClientRect();
        setWidth(() => width);
        setHeight(() => height);
      }
    }, 100);

    return () => {
      clearInterval(intervalId);
    };
  }, [ref]);

  // TODO: Replace this logic with some reference to the workflow context or visualizer output.
  //       Alternatively, we could also use some object that emits events when the data changes.
  useEffect(() => {
    const listener = (data: any) => {
      setData(data);
    };
    onUpdate(listener);
    return () => {
      onUpdate(null);
    };
  }, [onUpdate]);

  useEffect(() => {
    if (!ref.current) return;
    if (!data) return;

    // Clear the previous visualization.
    ref.current.innerHTML = "";

    switch (type) {
      case "scatter":
        ScatterPlot(ref.current, {
          ...data,
          size: { width, height },
        });
        break;
      case "histogram":
        HistogramPlot(ref.current, {
          ...data,
          size: { width, height },
        });
        break;
      case "graph":
        GraphPlot(ref.current, {
          ...data,
          size: { width, height },
        });
        break;
    }
  }, [type, data, width, height]);

  console.log();

  useEffect(() => {
    return () => {
      if (!mounted.current) onClose();
    };
  }, [mounted, onClose]);

  const title = useMemo(() => {
    return (
      <Text align="middle">
        <VisualizeIcon padded />
        {name}
        &nbsp;
        <Text size="small" color="notify">
          [Visualizer]
        </Text>
      </Text>
    );
  }, [name]);

  return (
    <Views.Container title={title}>
      <div ref={ref} style={{ width: "100%", height: "100%" }} />
    </Views.Container>
  );
};

export default VisualizerView;
