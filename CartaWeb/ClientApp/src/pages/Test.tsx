import { BlockButton } from "components/buttons";
import { SplitArea } from "components/containers";
import { PageLayout } from "components/layout";
import ViewContext from "components/views/ViewContext";
import ViewContainer from "components/views/_ViewContainer";
import ViewRenderer from "components/views/_ViewRenderer";
import {
  FunctionComponent,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";

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
      <p>Count: {count}</p>
      <p>Timer: {timerCount}</p>
      <BlockButton
        onClick={() => {
          if (parentView !== null) {
            actions.addChildElement(
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

  // Question.
  // 1. Do the actions apply to the current view?
  // 2. Do the actions apply to the parent view?
  // 3. How can we get the actions available for both?
};

const TestTreeLayout: Function = () => {
  const initializedRef = useRef<boolean>(false);
  const { viewId, actions } = useContext(ViewContext);

  useEffect(() => {
    if (!initializedRef.current) {
      initializedRef.current = true;

      actions.addChildElement(viewId, <TestTreeElement count={0} />);
    }
  }, [viewId, actions]);

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
