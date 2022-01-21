import { FunctionComponent } from "react";
import { Workspace } from "library/api";
import { Carousel } from "components/carousel";
import WorkspaceCard from "./WorkspaceCard";

/** The props used for the {@link WorkspaceCarousel} component. */
interface WorkspaceCarouselProps {
  /** A list of workspaces that can be used for the carousel. */
  workspaces: Workspace[];
}

/** Renders a carousel of workspace cards that are either specified by the user */
const WorkspaceCarousel: FunctionComponent<WorkspaceCarouselProps> = ({
  workspaces,
}) => {
  return (
    <Carousel>
      {workspaces.map((workspace) => (
        <Carousel.Item key={workspace.id}>
          {/* We render each element of the carousel as a card. */}
          <WorkspaceCard workspace={workspace} />
        </Carousel.Item>
      ))}
    </Carousel>
  );
};

export default WorkspaceCarousel;
