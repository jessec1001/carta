import { Accordian } from "components/accordian";
import {
  Button,
  ButtonGroup,
  CloseButton,
  IconButton,
  IconButtonAdd,
  IconButtonRemove,
} from "components/buttons";
import { Card } from "components/card";
import { CheckboxInput, TextFieldInput } from "components/input";
import { PageLayout } from "components/layout";
import { Link } from "components/link";
import { Tabs } from "components/tabs";
import { Text, Title } from "components/text";
import React, { FunctionComponent, useState } from "react";

interface ContainerItem {
  id: number;
  name: string;
  status: "none" | "unmodified" | "modified" | "info" | "warning" | "error";
  element: React.ReactNode;
}

const Container: FunctionComponent = () => {
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

  const [username, setUsername] = useState<string>("");
  const [password, setPassword] = useState<string>("");

  return (
    <Tabs>
      <Tabs.Area direction="horizontal">
        <Tabs.Bar>
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
                    const positionNormalized = false
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
              {hovering &&
                hovering.id === item.id &&
                hovering.side === "start" && (
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
        </Tabs.Bar>
        <Tabs.Panel id={0}>
          <div style={{ padding: "1rem 2rem" }}>
            <section>
              <Title>Authentication</Title>
              <div>
                Username:{" "}
                <TextFieldInput value={username} onChange={setUsername} />
                Password:{" "}
                <TextFieldInput
                  value={password}
                  onChange={setPassword}
                  password
                />
                <Button
                  onClick={async () => {
                    try {
                      // const user = await Auth.signIn(username, password);
                      // console.log("Successfully signed in!", user);
                    } catch (error: any) {
                      alert(error.message);
                    }
                  }}
                >
                  Sign In
                </Button>
              </div>
            </section>
            <section>
              <Title>Accordians</Title>
              <Accordian>
                <Accordian.Header>
                  <Text size="medium">Header</Text>
                  <Accordian.Toggle>Toggle</Accordian.Toggle>
                </Accordian.Header>
                <Accordian.Content>
                  Content: Lorem ipsum dolor sit amet, consectetur adipiscing
                  elit, sed do eiusmod tempor incididunt ut labore et dolore
                  magna aliqua. Ut enim ad minim veniam, quis nostrud
                  exercitation ullamco laboris nisi ut aliquip ex ea commodo
                  consequat. Duis aute irure dolor in reprehenderit in voluptate
                  velit esse cillum dolore eu fugiat nulla pariatur. Excepteur
                  sint occaecat cupidatat non proident, sunt in culpa qui
                  officia deserunt mollit anim id est laborum.
                </Accordian.Content>
              </Accordian>
              <Accordian>
                <Accordian.Header>
                  <Text size="medium">Header</Text>
                  <Accordian.Toggle caret />
                </Accordian.Header>
                <Accordian.Content>
                  Content: Lorem ipsum dolor sit amet, consectetur adipiscing
                  elit, sed do eiusmod tempor incididunt ut labore et dolore
                  magna aliqua. Ut enim ad minim veniam, quis nostrud
                  exercitation ullamco laboris nisi ut aliquip ex ea commodo
                  consequat. Duis aute irure dolor in reprehenderit in voluptate
                  velit esse cillum dolore eu fugiat nulla pariatur. Excepteur
                  sint occaecat cupidatat non proident, sunt in culpa qui
                  officia deserunt mollit anim id est laborum.
                </Accordian.Content>
              </Accordian>
              <Accordian>
                <Accordian.Header>
                  <Text size="medium">Header 1</Text>
                  <Accordian.Toggle>Toggle 1</Accordian.Toggle>
                </Accordian.Header>
                <Accordian.Content>
                  Content: Lorem ipsum dolor sit amet, consectetur adipiscing
                  elit, sed do eiusmod tempor incididunt ut labore et dolore
                  magna aliqua. Ut enim ad minim veniam, quis nostrud
                  exercitation ullamco laboris nisi ut aliquip ex ea commodo
                  consequat. Duis aute irure dolor in reprehenderit in voluptate
                  velit esse cillum dolore eu fugiat nulla pariatur. Excepteur
                  sint occaecat cupidatat non proident, sunt in culpa qui
                  officia deserunt mollit anim id est laborum.
                </Accordian.Content>
                <Accordian.Header>
                  <Text size="medium">Header 2</Text>
                  <Accordian.Toggle>Toggle 2</Accordian.Toggle>
                </Accordian.Header>
              </Accordian>
            </section>
            <section>
              <Title>Arrows</Title>
              {/* <Arrows.Arrow
                source=""
                target=""
                // points={[
                //   [0, 0],
                //   [20, 0],
                // ]}
              />
              <div />
              <Arrows.Arrow
                source=""
                target=""
                // points={[
                //   [1, 1],
                //   [1, 200],
                //   [200, 200],
                //   [200, 1],
                // ]}
              /> */}
            </section>
            <section>
              <Title>Buttons</Title>
              <section>
                <Title size="medium">Close Button</Title>
                <div>
                  <CloseButton />
                  <CloseButton disabled />
                </div>
                <Title size="medium">Buttons</Title>
                <p>Ungrouped:</p>
                <div>
                  <Button color="notify">Notify</Button>
                  <Button color="info">Info</Button>
                  <Button color="warning">Warning</Button>
                  <Button color="error">Error</Button>
                  <Button color="muted">Muted</Button>
                </div>
                <div>
                  <Button color="primary">Primary</Button>
                  <Button color="secondary">Secondary</Button>
                </div>
                <div>
                  <Button disabled color="notify">
                    Notify
                  </Button>
                  <Button disabled color="info">
                    Info
                  </Button>
                  <Button disabled color="warning">
                    Warning
                  </Button>
                  <Button disabled color="error">
                    Error
                  </Button>
                  <Button disabled color="muted">
                    Muted
                  </Button>
                </div>
                <div>
                  <Button disabled color="primary">
                    Primary
                  </Button>
                  <Button disabled color="secondary">
                    Secondary
                  </Button>
                </div>
                <div>
                  <Button outline color="notify">
                    Notify
                  </Button>
                  <Button outline color="info">
                    Info
                  </Button>
                  <Button outline color="warning">
                    Warning
                  </Button>
                  <Button outline color="error">
                    Error
                  </Button>
                  <Button outline color="muted">
                    Muted
                  </Button>
                </div>
                <div>
                  <Button outline color="primary">
                    Primary
                  </Button>
                  <Button outline color="secondary">
                    Secondary
                  </Button>
                </div>
                <p>Grouped:</p>
                <div>
                  <ButtonGroup>
                    <Button color="notify">Notify</Button>
                    <Button color="info">Info</Button>
                    <Button color="warning">Warning</Button>
                    <Button color="error">Error</Button>
                    <Button color="muted">Muted</Button>
                  </ButtonGroup>
                  <div style={{ height: "0.5rem" }} />
                  <ButtonGroup connected>
                    <Button color="notify">Notify</Button>
                    <Button color="info">Info</Button>
                    <Button color="warning">Warning</Button>
                    <Button color="error">Error</Button>
                    <Button color="muted">Muted</Button>
                  </ButtonGroup>
                  <div style={{ height: "0.5rem" }} />
                  <ButtonGroup connected>
                    <Button outline color="notify">
                      Notify
                    </Button>
                    <Button outline color="info">
                      Info
                    </Button>
                    <Button outline color="warning">
                      Warning
                    </Button>
                    <Button outline color="error">
                      Error
                    </Button>
                    <Button outline color="muted">
                      Muted
                    </Button>
                  </ButtonGroup>
                </div>
              </section>
              <section>
                <Title size="medium">Icon Buttons</Title>
                <p>
                  Add: <IconButtonAdd />
                  <IconButtonAdd shape="square" />
                  <IconButtonAdd disabled />
                </p>
                <p>
                  Remove: <IconButtonRemove />
                  <IconButtonRemove shape="square" />
                  <IconButtonRemove disabled />
                </p>
                <p>
                  Arbitrary: <IconButton>&sum;</IconButton>
                  <IconButton>ab</IconButton>
                </p>
              </section>
            </section>
            <section>
              <Title>Cards</Title>
              <Card>
                <Card.Body>
                  <Title size="medium">Card</Title>
                  <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed
                    do eiusmod tempor incididunt ut labore et dolore magna
                    aliqua. Ut enim ad minim veniam, quis nostrud exercitation.
                  </p>
                </Card.Body>
              </Card>
              <div style={{ height: "1rem" }} />
              <Card>
                <Card.Header>
                  <Title size="medium">Card Header</Title>
                </Card.Header>
                <Card.Body>
                  <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed
                    do eiusmod tempor incididunt ut labore et dolore magna
                    aliqua. Ut enim ad minim veniam, quis nostrud exercitation.
                  </p>
                </Card.Body>
              </Card>
              <div style={{ height: "1rem" }} />
              <Card>
                <Card.Header>
                  <Title size="medium">Card Header</Title>
                </Card.Header>
                <Card.Body>
                  <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed
                    do eiusmod tempor incididunt ut labore et dolore magna
                    aliqua. Ut enim ad minim veniam, quis nostrud exercitation.
                  </p>
                </Card.Body>
                <Card.Footer>
                  <Text color="muted">
                    <time>Monday, Nov. 7th</time>
                  </Text>
                </Card.Footer>
              </Card>
            </section>
            <section>
              <Title>Links</Title>
              <div>
                <Link to="https://www.google.com" color="notify">
                  Notify
                </Link>
                &nbsp;
                <Link to="https://www.google.com" color="info">
                  Info
                </Link>
                &nbsp;
                <Link to="https://www.google.com" color="warning">
                  Warning
                </Link>
                &nbsp;
                <Link to="https://www.google.com" color="error">
                  Error
                </Link>
                &nbsp;
                <Link to="https://www.google.com" color="muted">
                  Muted
                </Link>
                &nbsp;
                <Link to="https://www.google.com" color="primary">
                  Primary
                </Link>
                &nbsp;
                <Link to="https://www.google.com" color="secondary">
                  Secondary
                </Link>
              </div>
            </section>
            <section>
              <Title>Tabs</Title>
              <div
                style={{
                  width: "100%",
                  height: "32rem",
                  border: "1px solid #666",
                }}
              >
                <Tabs draggableTabs>
                  {/* TODO: Make a component that allows these tabs to be moved around. */}
                  <Tabs.Area direction="horizontal">
                    <Tabs.Bar>
                      <Tabs.Tab id={0}>Tab 1</Tabs.Tab>
                      <Tabs.Tab id={1}>Tab 2</Tabs.Tab>
                      <Tabs.Tab id={2}>Tab 3</Tabs.Tab>
                    </Tabs.Bar>
                    <Tabs.Panel id={0}>
                      <Text>Panel 1</Text>
                      <CheckboxInput />
                    </Tabs.Panel>
                    <Tabs.Panel id={1}>
                      <Text>Panel 2</Text>
                      <CheckboxInput />
                    </Tabs.Panel>
                    <Tabs.Panel id={2}>
                      <Text>Panel 3</Text>
                      <CheckboxInput />
                    </Tabs.Panel>
                  </Tabs.Area>
                </Tabs>
              </div>
              <div style={{ margin: "1rem 0rem" }} />
              <div
                style={{
                  width: "100%",
                  height: "32rem",
                  border: "1px solid #666",
                }}
              >
                <Tabs>
                  {/* TODO: Make a component that allows these tabs to be moved around. */}
                  <Tabs.Area direction="vertical">
                    <Tabs.Bar>
                      <Tabs.Tab id={0}>Tab 1</Tabs.Tab>
                      <Tabs.Tab id={1}>Tab 2</Tabs.Tab>
                      <Tabs.Tab id={2}>Tab 3</Tabs.Tab>
                    </Tabs.Bar>
                    <Tabs.Panel id={0}>
                      <Text>Panel 1</Text>
                    </Tabs.Panel>
                    <Tabs.Panel id={1}>
                      <Text>Panel 2</Text>
                    </Tabs.Panel>
                    <Tabs.Panel id={2}>
                      <Text>Panel 3</Text>
                    </Tabs.Panel>
                  </Tabs.Area>
                </Tabs>
              </div>
            </section>
          </div>
        </Tabs.Panel>
        {items.map((item) => (
          <Tabs.Panel key={item.id} id={item.id}>
            {item.element}
          </Tabs.Panel>
        ))}
      </Tabs.Area>
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
