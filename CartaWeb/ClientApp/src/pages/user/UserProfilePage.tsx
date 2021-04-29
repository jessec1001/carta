import React, { Component } from "react";
import { Sidebar, SidebarLayout } from "components/layouts";

export interface UserProfilePageProps {}
export interface UserProfilePageState {}

export default class UserProfilePage extends Component<
  UserProfilePageProps,
  UserProfilePageState
> {
  render() {
    return (
      <SidebarLayout side="left">
        <Sidebar>
          {/* <Navigation>
            <Link>General</Link>
            <Link>Notifications</Link>
            <Link>Integeration</Link>
            <IndentList>
              <Link>HyperThought&trade;</Link>
            </IndentList>
          </Navigation> */}
        </Sidebar>
        <Mainbar>
          {/* <Title>Profile</Title>
          <Section title="General">
            <UserInput disabled key="username">
              Username
            </UserInput>
            <UserInput disabled key="email">
              E-mail
            </UserInput>
          </Section>
          <Section title="Notifications">
            <StoredOption
              key="notificationVerbosity"
              options={["Debug", "Info", "Warning", "Error"]}
            ></StoredOption>
          </Section>
          <Section title="Integration">
            <Subection title="HyperThought&trade;">
              <StoredInput type="password" key="hyperthoughtKey">
                API Key
              </StoredInput>
            </Subection>
          </Section> */}
        </Mainbar>
      </SidebarLayout>
    );
  }
}
