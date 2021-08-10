import { WorkflowIcon } from "components/icons";
import { Tab, TabContainer } from "components/tabs";
import ViewContext from "components/views/ViewContext";
import React, { FunctionComponent, useContext } from "react";

const WorkspaceCreateView: FunctionComponent = () => {
  const { viewId, actions } = useContext(ViewContext);

  const handleClose = () => {
    actions.removeChildElement(viewId);
  };

  return (
    <TabContainer>
      <Tab
        title={
          <React.Fragment>
            <WorkflowIcon padded /> Create Workflow
          </React.Fragment>
        }
        onClose={handleClose}
        closeable
      ></Tab>
    </TabContainer>
  );
};

export default WorkspaceCreateView;
