import { FunctionComponent } from "react";
import { PageLayout } from "components/layout";
import { UserResetPasswordDialog } from "components/user";
import styles from "./AuthPage.module.css";

const ResetPasswordPage: FunctionComponent = () => {
    return (
        <PageLayout header footer className={styles.page}>
            <UserResetPasswordDialog showTitle />
        </PageLayout>
    );
};

export default ResetPasswordPage;