import { BlockButton } from "components/buttons";
import { PageLayout } from "components/layout";
import ViewContext from "components/views/ViewContext";
import ViewContainer from "components/views/ViewContainer";
import ViewRenderer from "components/views/ViewRenderer";
import {
  FunctionComponent,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import LoadingIcon from "components/icons/LoadingIcon";

const TestTreeElement: FunctionComponent<{ count: number }> = ({ count }) => {
  const { viewId, actions } = useContext(ViewContext);
  const parentView = actions.getParentView(viewId);

  const [timerCount, setTimerCount] = useState<number>(0);
  useEffect(() => {
    const intervalId = setInterval(() => {
      setTimerCount((timerCount) => timerCount + 1);
    }, 1000);
    return () => clearInterval(intervalId);
  }, []);

  return (
    <div>
      <p className="normal-text">
        Loading <LoadingIcon animated padded />
      </p>
      <p>Count: {count}</p>
      <p>Timer: {timerCount}</p>
      <BlockButton
        onClick={() => {
          if (parentView !== null) {
            actions.addElementToContainer(
              parentView.currentId,
              <TestTreeElement count={count + 1} />
            );
          }
        }}
      >
        Click me!
      </BlockButton>
    </div>
  );
};

const TestTreeLayout: Function = () => {
  const initializedRef = useRef<boolean>(false);
  const { rootId, actions } = useContext(ViewContext);

  useEffect(() => {
    if (!initializedRef.current) {
      initializedRef.current = true;

      actions.addElementToContainer(rootId, <TestTreeElement count={0} />);
    }
  }, [rootId, actions]);

  return null;
};

const TestPage: FunctionComponent = () => {
  return (
    <PageLayout header footer>
      <ViewContainer>
        <TestTreeLayout />
        <ViewRenderer />
      </ViewContainer>
    </PageLayout>
  );
};

export default TestPage;
