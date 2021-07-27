import { FunctionComponent } from "react";
import { WorkspaceDataset } from "library/api";
import { SearchboxInput } from "components/input";
import { DatasetIcon } from "components/icons";
import { Heading } from "components/text";
import { IconAddButton } from "components/buttons";

/** The props used for the {@link DatasetListView} component. */
interface DatasetListViewProps {
  datasets: WorkspaceDataset[];
}

/** A component that renders a list of datasets that can be searched and sorted. */
const DatasetListView: FunctionComponent<DatasetListViewProps> = ({
  datasets,
  children,
}) => {
  return (
    <div>
      <Heading>
        <SearchboxInput clearable />
        <IconAddButton />
      </Heading>
      <ul>
        {datasets.map((dataset) => (
          <li>
            <span
              className="normal-text"
              style={{
                color: "var(--color-stroke-lowlight)",
              }}
            >
              <DatasetIcon />
              {dataset.name ?? `(${dataset.source}/${dataset.resource})`}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
};

// TODO: Toggle for resource/source hierarchy:
// <Database Icon> <Accordian> Source
//   <Database Icon> <Accordian> Resource
//     <Dataset Icon> Dataset Name
//     <Dataset Icon> Dataset Name
//   <Database Icon> <Accordian> Resource
//     <Dataset Icon> Dataset Name

export default DatasetListView;
export type { DatasetListViewProps };
