import React, { Component } from "react";
import {
  IndentList,
  Link,
  Mainbar,
  Section,
  Sidebar,
  SidebarLayout,
  StoredInput,
  Subsection,
  Title,
  UserInput,
} from "components/ui/layout";
import { Container } from "reactstrap";
import UserContext from "components/ui/user";

export interface UserProfilePageProps {}
export interface UserProfilePageState {}

export default class UserProfilePage extends Component<
  UserProfilePageProps,
  UserProfilePageState
> {
  render() {
    return (
      <Container>
        <UserContext.Consumer>
          {({ manager, user }) => (
            <SidebarLayout side="left">
              <Sidebar>
                <IndentList>
                  {user && (
                    <Link href="/user/profile#profile-general">General</Link>
                  )}
                  {/* <Link href="/user/profile/#profile-notifications">
                    Notifications
                  </Link> */}
                  <Link href="/user/profile/#profile-integration">
                    Integeration
                  </Link>
                  <IndentList>
                    <Link href="/user/profile/#profile-integration-hyperthought">
                      HyperThought&trade;
                    </Link>
                  </IndentList>
                </IndentList>
              </Sidebar>
              <Mainbar>
                <div
                  style={{ display: "flex", justifyContent: "space-between" }}
                >
                  <Title>Profile</Title>
                </div>
                {user && (
                  <Section title="General" id="profile-general">
                    <UserInput disabled field="username">
                      Username
                    </UserInput>
                    <UserInput disabled field="email">
                      E-mail
                    </UserInput>
                  </Section>
                )}
                {/* <Section title="Notifications" id="profile-notifications"> */}
                {/* <StoredOption
                key="notificationVerbosity"
                options={["Debug", "Info", "Warning", "Error"]}
              ></StoredOption> */}
                {/* </Sectio n> */}
                <Section title="Integration" id="profile-integration">
                  <Subsection
                    title="HyperThought&trade;"
                    id="profile-integration-hyperthought"
                  >
                    <StoredInput type="password" field="hyperthoughtKey">
                      API Key
                    </StoredInput>
                  </Subsection>
                </Section>
              </Mainbar>
            </SidebarLayout>
          )}
        </UserContext.Consumer>
      </Container>
    );
  }
}
