import { FunctionComponent } from "react";
import { useStoredState, useUserValue } from "hooks";
import { Container } from "reactstrap";
import { Mainbar, Sidebar, SidebarLayout } from "components/ui/layout";
import { Link } from "components/ui/common/link";
import { IndentList, Section, Subsection, Title } from "components/structure";
import { FormGroup } from "components/ui/form";
import { DropdownInput, TextFieldInput } from "components/ui/form/input";

const UserPage: FunctionComponent = () => {
  const userName = useUserValue("name");
  const userEmail = useUserValue("email");

  const [hyperthoughtKey, setHyperthoughtKey] = useStoredState(
    "",
    "hyperthoughtKey"
  );
  const [notificationLevel, setNotificationLevel] = useStoredState(
    "Info",
    "notificationVerbosity"
  );

  return (
    <Container>
      <SidebarLayout side="left">
        <Sidebar>
          <IndentList>
            {userName !== null && (
              <Link href="/user/profile#profile-general">General</Link>
            )}
            <Link href="/user/profile/#profile-integration">Integeration</Link>
            <IndentList>
              <Link href="/user/profile/#profile-integration-hyperthought">
                HyperThought&trade;
              </Link>
            </IndentList>
            <Link href="/user/profile/#profile-notifications">
              Notifications
            </Link>
          </IndentList>
        </Sidebar>
        <Mainbar>
          <div>
            <Title>Profile</Title>
            <p>
              Here, you will find a collection of information about your account
              along with some adjustable settings that affect your experience on
              Carta. All modifiable settings on this page are automatically
              saved when changed.
            </p>
          </div>
          {userName !== null && (
            <Section title="General" {...({ id: "profile-general" } as any)}>
              <FormGroup
                title="Username"
                description="The username associated with this account."
                density="dense"
              >
                <TextFieldInput value={userName!} disabled />
              </FormGroup>
              <FormGroup
                title="E-mail"
                description="The e-mail associated with this account."
                density="dense"
              >
                <TextFieldInput value={userEmail!} disabled />
              </FormGroup>
            </Section>
          )}
          <Section
            title="Integration"
            {...({ id: "profile-profile-integration" } as any)}
          >
            <Subsection
              title="HyperThought&trade;"
              {...({ id: "profile-integration-hyperthought" } as any)}
            >
              <FormGroup
                title="API Key"
                description={
                  "Your HyperThought&trade; API key is available in your [user profile](https://www.hyperthought.io/api/common/my_account/) on the HyperThought&trade; website. **IMPORTANT:** this is sensitive information that could be used to forge your identity - do not misplace this information or allow it to be leaked."
                }
              >
                <TextFieldInput
                  value={hyperthoughtKey}
                  onChange={setHyperthoughtKey}
                  password
                />
              </FormGroup>
            </Subsection>
          </Section>
          <Section
            title="Notifications"
            {...({ id: "profile-notifications" } as any)}
          >
            <FormGroup
              title="Verbsosity"
              description="The minimum level of information reported by notable events."
            >
              <DropdownInput
                value={notificationLevel}
                onChange={setNotificationLevel}
                options={["Debug", "Info", "Warning", "Error"]}
              />
            </FormGroup>
          </Section>
        </Mainbar>
      </SidebarLayout>
    </Container>
  );
};

export default UserPage;
