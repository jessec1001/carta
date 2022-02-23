import { useArrows } from "components/arrows/Context";
import { Operation } from "library/api";
import { FunctionComponent } from "react";
import "./OperationNodeConnector.css";
import { useWorkflow } from "./WorkflowContext";

interface OperationNodeConnectorProps {
  operation: Operation;
  connector: { name: string; type: string };
  attachment: "input" | "output";
}

const OperationNodeConnector: FunctionComponent<
  OperationNodeConnectorProps
> = ({ operation, connector, attachment }) => {
  const { nodes } = useArrows();
  const { name, type } = connector;
  const { tryConnect } = useWorkflow();

  return (
    <div className={`OperationNodeConnector ${attachment}`}>
      <div
        className="point"
        title={`Type: ${type}`}
        style={{ background: "var(--color-stroke-normal)" }}
        onClick={() => tryConnect(operation.id, name, attachment)}
        id={`connector_${operation.id}_${connector.name}`}
        ref={nodes(`${operation.id}:${connector.name}`).set}
      />
      {name}
    </div>
  );
};

// const findTypeColor = (type: string): string => {
//   switch (type) {
//     case "any":
//       return "linear-gradient(-0.125turn, #ff4444, #4444ff)";
//     default:
//       return "#888888";
//   }
// };

export default OperationNodeConnector;
export type { OperationNodeConnectorProps };
