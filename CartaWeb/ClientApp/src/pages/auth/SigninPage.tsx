import { FunctionComponent } from "react";
import { PageLayout } from "components/layout";
import { UserSignInDialog } from "components/user";
import styles from "./AuthPage.module.css";

const SignInPage: FunctionComponent = () => {
  return (
    <PageLayout header footer className={styles.page}>
      <UserSignInDialog showTitle />
    </PageLayout>
  );
};

export default SignInPage;
