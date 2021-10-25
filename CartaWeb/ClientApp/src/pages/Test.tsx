import { Accordian } from "components/accordian";
import {
  BlockButton,
  ButtonGroup,
  IconButton,
  IconButtonAdd,
  IconButtonRemove,
} from "components/buttons";
import { PageLayout } from "components/layout";
import { TabBar, Tabs } from "components/tabs";
import { Text, Title } from "components/text";
import React, { FunctionComponent, useState } from "react";

interface ContainerItem {
  id: number;
  name: string;
  status: "none" | "unmodified" | "modified" | "info" | "warning" | "error";
  element: React.ReactNode;
}

const Container: FunctionComponent = () => {
  // TODO: Make vertical tabs work.
  const vertical = false;
  const [hovering, setHovering] = useState<{
    id: number;
    side: "start" | "end";
  } | null>(null);
  const [items, setItems] = useState<ContainerItem[]>([
    {
      id: 1,
      name: "None",
      status: "none",
      element: "Test Panel #1",
    },
    {
      id: 2,
      name: "Unmodified",
      status: "unmodified",
      element: "Test Panel #2",
    },
    {
      id: 3,
      name: "Modified",
      status: "modified",
      element: "Test Panel #3",
    },
    {
      id: 4,
      name: "Info",
      status: "info",
      element: "Test Panel #4",
    },
    {
      id: 5,
      name: "Warn",
      status: "warning",
      element: "Test Panel #5",
    },
    {
      id: 6,
      name: "Error",
      status: "error",
      element: "Test Panel #6",
    },
  ]);

  return (
    <Tabs>
      <TabBar direction={vertical ? "vertical" : "horizontal"}>
        <Tabs.Tab id={0}>Content</Tabs.Tab>
        {items.map((item) => (
          <React.Fragment key={item.id}>
            {hovering && hovering.id === item.id && hovering.side === "end" && (
              <div
                style={{
                  width: "5px",
                  margin: "0 -2px",
                  backgroundColor: "var(--color-primary)",
                }}
              />
            )}
            <Tabs.Tab
              id={item.id}
              status={item.status}
              onDragStart={(event) => {
                event.dataTransfer.setData(
                  "text/json+tab",
                  JSON.stringify(item.id)
                );
                event.dataTransfer.effectAllowed = "move";
              }}
              closeable
              draggable
              onDragOver={(event) => {
                if (event.dataTransfer.types.includes("text/json+tab")) {
                  const rect = event.currentTarget.getBoundingClientRect();
                  const positionNormalized = vertical
                    ? (rect.bottom - event.clientY) / rect.height
                    : (rect.right - event.clientX) / rect.width;

                  setHovering({
                    id: item.id,
                    side: positionNormalized <= 0.5 ? "start" : "end",
                  });
                  event.preventDefault();
                }
              }}
              onDrop={(event) => {
                const data = event.dataTransfer.getData("text/json+tab");
                if (data.length > 0) {
                  const id = JSON.parse(data);
                  if (hovering && hovering.id !== id) {
                    const newItems = [...items];
                    const movingItemIndex = newItems.findIndex(
                      (item) => item.id === id
                    );
                    const movingItem = newItems[movingItemIndex];
                    newItems.splice(movingItemIndex, 1);

                    const hoveringItemIndex = newItems.findIndex(
                      (item) => item.id === hovering.id
                    );
                    newItems.splice(
                      hoveringItemIndex + (hovering.side === "start" ? 1 : 0),
                      0,
                      movingItem
                    );

                    setItems(newItems);
                    setHovering(null);
                  }
                }
              }}
            >
              {item.name}
            </Tabs.Tab>
            {hovering && hovering.id === item.id && hovering.side === "start" && (
              <div
                style={{
                  width: "5px",
                  margin: "0 -2px",
                  backgroundColor: "var(--color-primary)",
                }}
              />
            )}
          </React.Fragment>
        ))}
      </TabBar>
      <Tabs.Panel id={0}>
        <section>
          <Title>Accordians</Title>
          <Accordian>
            <Accordian.Header>
              <Text size="medium" padding="top">
                Header
              </Text>
              <Accordian.Toggle>Toggle</Accordian.Toggle>
            </Accordian.Header>
            <Accordian.Content>
              Content: Lorem ipsum dolor sit amet, consectetur adipiscing elit,
              sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
              Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris
              nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in
              reprehenderit in voluptate velit esse cillum dolore eu fugiat
              nulla pariatur. Excepteur sint occaecat cupidatat non proident,
              sunt in culpa qui officia deserunt mollit anim id est laborum.
            </Accordian.Content>
          </Accordian>
          <Accordian>
            <Accordian.Header>
              <Text size="medium" padding="top">
                Header
              </Text>
              <Accordian.Toggle caret />
            </Accordian.Header>
            <Accordian.Content>
              Content: Lorem ipsum dolor sit amet, consectetur adipiscing elit,
              sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
              Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris
              nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in
              reprehenderit in voluptate velit esse cillum dolore eu fugiat
              nulla pariatur. Excepteur sint occaecat cupidatat non proident,
              sunt in culpa qui officia deserunt mollit anim id est laborum.
            </Accordian.Content>
          </Accordian>
          <Accordian>
            <Accordian.Header>
              <Text size="medium" padding="top">
                Header 1
              </Text>
              <Accordian.Toggle>Toggle 1</Accordian.Toggle>
            </Accordian.Header>
            <Accordian.Content>
              Content: Lorem ipsum dolor sit amet, consectetur adipiscing elit,
              sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
              Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris
              nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in
              reprehenderit in voluptate velit esse cillum dolore eu fugiat
              nulla pariatur. Excepteur sint occaecat cupidatat non proident,
              sunt in culpa qui officia deserunt mollit anim id est laborum.
            </Accordian.Content>
            <Accordian.Header>
              <Text size="medium">Header 2</Text>
              <Accordian.Toggle>Toggle 2</Accordian.Toggle>
            </Accordian.Header>
          </Accordian>
        </section>
        <section>
          <Title>Buttons</Title>
          <section>
            <Title size="medium">Block Buttons</Title>
            <p>Ungrouped:</p>
            <div>
              <BlockButton color="notify">Notify</BlockButton>
              <BlockButton color="info">Info</BlockButton>
              <BlockButton color="warning">Warning</BlockButton>
              <BlockButton color="error">Error</BlockButton>
              <BlockButton color="muted">Muted</BlockButton>
            </div>
            <div>
              <BlockButton color="primary">Primary</BlockButton>
              <BlockButton color="secondary">Secondary</BlockButton>
            </div>
            <p>Grouped:</p>
            <div style={{ width: "100%", padding: "0.5rem" }}>
              <ButtonGroup>
                <BlockButton color="notify">Notify</BlockButton>
                <BlockButton color="info">Info</BlockButton>
                <BlockButton color="warning">Warning</BlockButton>
                <BlockButton color="error">Error</BlockButton>
                <BlockButton color="muted">Muted</BlockButton>
              </ButtonGroup>
              <div style={{ height: "1rem" }} />
              <ButtonGroup connected>
                <BlockButton color="notify">Notify</BlockButton>
                <BlockButton color="info">Info</BlockButton>
                <BlockButton color="warning">Warning</BlockButton>
                <BlockButton color="error">Error</BlockButton>
                <BlockButton color="muted">Muted</BlockButton>
              </ButtonGroup>
            </div>
          </section>
          <section>
            <Title size="medium">Icon Buttons</Title>
            <p>
              Add: <IconButtonAdd />
            </p>
            <p>
              Remove: <IconButtonRemove />
            </p>
            <p>
              Arbitrary: <IconButton>&sum;</IconButton>{" "}
              <IconButton>ab</IconButton>
            </p>
          </section>
        </section>
      </Tabs.Panel>
      {items.map((item) => (
        <Tabs.Panel key={item.id} id={item.id}>
          {item.element}
        </Tabs.Panel>
      ))}
    </Tabs>
  );
};

const TestPage: FunctionComponent = () => {
  return (
    <PageLayout header footer>
      <div style={{ height: "100%" }}>
        <Container />
      </div>
    </PageLayout>
  );
};

export default TestPage;
