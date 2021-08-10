import { WorkflowIcon } from "components/icons";
import { Tab, TabContainer } from "components/tabs";
import ViewContext from "components/views/ViewContext";
import React, { useContext } from "react";

const renderWorkflow = () => {};

const WorkspaceToolboxView = () => {
  const { viewId, actions } = useContext(ViewContext);

  const handleClose = () => {
    actions.removeChildElement(viewId);
  };

  return (
    <TabContainer>
      <Tab
        title={
          <React.Fragment>
            <WorkflowIcon padded /> Toolbox
          </React.Fragment>
        }
        onClose={handleClose}
        closeable
      ></Tab>
    </TabContainer>
  );
};

export default WorkspaceToolboxView;
