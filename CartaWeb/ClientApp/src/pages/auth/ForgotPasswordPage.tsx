import { FunctionComponent } from "react";
import { PageLayout } from "components/layout";
import { UserForgotPasswordDialog } from "components/user";
import styles from "./AuthPage.module.css";

const ForgotPasswordPage: FunctionComponent = () => {
    return (
        <PageLayout header footer className={styles.page}>
            <UserForgotPasswordDialog showTitle />
        </PageLayout>
    );
};

export default ForgotPasswordPage;