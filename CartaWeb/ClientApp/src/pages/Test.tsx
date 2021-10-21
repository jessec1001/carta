import { Accordian } from "components/accordian";
import { PageLayout } from "components/layout";
import { TabBar, Tabs } from "components/tabs";
import React, { FunctionComponent, useState } from "react";

interface ContainerItem {
  id: number;
  name: string;
  status: "none" | "unmodified" | "modified" | "info" | "warning" | "error";
  element: React.ReactNode;
}

const Container: FunctionComponent = () => {
  const vertical = false;
  const [hovering, setHovering] = useState<{
    id: number;
    side: "start" | "end";
  } | null>(null);
  const [items, setItems] = useState<ContainerItem[]>([
    {
      id: 1,
      name: "Tab",
      status: "none",
      element: (
        <Accordian>
          <Accordian.Header>
            <span>Blah</span>
            <Accordian.Toggle caret />
          </Accordian.Header>
          <Accordian.Content></Accordian.Content>
        </Accordian>
      ),
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
