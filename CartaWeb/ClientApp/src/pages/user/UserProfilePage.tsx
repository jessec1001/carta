import React, { Component } from "react";
import {
  IndentList,
  Link,
  Mainbar,
  Section,
  Sidebar,
  SidebarLayout,
  SignInOutButton,
  StoredInput,
  Title,
  UserInput,
} from "components/layouts";
import { UserContext } from "App";

export interface UserProfilePageProps {}
export interface UserProfilePageState {}

export default class UserProfilePage extends Component<
  UserProfilePageProps,
  UserProfilePageState
> {
  render() {
    return (
      <UserContext.Consumer>
        {({ manager, user }) => (
          <SidebarLayout side="left">
            <Sidebar>
              <IndentList>
                {user && (
                  <Link href="/user/profile#profile-general">General</Link>
                )}
                <Link href="/user/profile/#profile-notifications">
                  Notifications
                </Link>
                <Link href="/user/profile/#profile-integration">
                  Integeration
                </Link>
                <IndentList>
                  <Link>HyperThought&trade;</Link>
                </IndentList>
              </IndentList>
            </Sidebar>
            <Mainbar>
              <div style={{ display: "flex", justifyContent: "space-between" }}>
                <Title>Profile</Title>
                <SignInOutButton className={`button bg-action`} />
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
              {/* </Section> */}
              <Section title="Integration" id="profile-integration">
                {/* <Subection title="HyperThought&trade;"> */}
                <StoredInput type="password" field="hyperthoughtKey">
                  API Key
                </StoredInput>
                {/* </Subection> */}
              </Section>
            </Mainbar>
          </SidebarLayout>
        )}
      </UserContext.Consumer>
    );
  }
}
