import { FunctionComponent } from "react";
import { useHistory } from "react-router";
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
  // We need to use this history object to move to a different URL.
  // When a workspace card is clicked, we navigate to the corresponding workspace page.
  const history = useHistory();
  const navigateExistingWorkspace = (workspaceId: string) => {
    history.push({
      pathname: `/workspace`,
      search: `?id=${workspaceId}`,
    });
  };

  return (
    <Carousel sizing="outer">
      {workspaces.map((workspace) => (
        <Carousel.Item key={workspace.id}>
          {/* We render each element of the carousel as a card. */}
          <WorkspaceCard
            workspace={workspace}
            onClick={() => navigateExistingWorkspace(workspace.id)}
          />
        </Carousel.Item>
      ))}
    </Carousel>
  );
};

export default WorkspaceCarousel;
