import { WorkflowIcon } from "components/icons";
import { JoinContainer, JoinInputLabel } from "components/join";
import { useControllableState } from "hooks";
import { WorkspaceWorkflow } from "library/api";
import { ObjectFilter } from "library/search";
import { FunctionComponent, useState } from "react";
import { OptionInput } from "../general";
import ComboboxInput from "../general/ComboboxInput";

interface WorkflowInputProps {
  workflows: WorkspaceWorkflow[];
  /** The value that this option component takes on. Set to `null` if no value is selected. */
  value?: WorkspaceWorkflow | null;

  /** The event handler for when the choice of workflow has changed. */
  onChange?: (value: WorkspaceWorkflow | null) => void;
}

const WorkflowInput: FunctionComponent<WorkflowInputProps> = ({
  workflows,
  value,
  onChange,
  children,
}) => {
  const [actualValue, setValue] = useControllableState(null, value, onChange);

  const [query, setQuery] = useState<string>("");
  const filter = new ObjectFilter(query, {});

  return (
    <JoinContainer direction="horizontal" grow="grow-last">
      <JoinInputLabel>
        <WorkflowIcon />
      </JoinInputLabel>
      <ComboboxInput
        comparer={(
          workflow1?: WorkspaceWorkflow,
          workflow2?: WorkspaceWorkflow
        ) => {
          if (!workflow1 || !workflow2) return false;
          return workflow1.id === workflow2.id;
        }}
        text={query}
        value={actualValue}
        onTextChanged={setQuery}
        onValueChanged={setValue}
      >
        {filter.filter(workflows).map((workflow) => {
          return (
            <OptionInput
              key={workflow.id}
              value={workflow}
              alias={workflow.name}
            >
              {workflow.name}
            </OptionInput>
          );
        })}
      </ComboboxInput>
    </JoinContainer>
  );
};

export default WorkflowInput;
export type { WorkflowInputProps };
