import { ApiException } from "./Exceptions";

class UserApi {
  static POPOP_FEATURES = Object.entries({
    location: "no",
    toolbar: "no",
    width: 500,
    height: 500,
    left: 100,
    top: 100,
  })
    .map(([key, value]) => `${key}=${value}`)
    .join(",");

  static SignInAsync(preventRedirect?: boolean): Promise<void> {
    return fetch("/api/user/signin", {
      method: "GET",
      redirect: "manual",
    }).then((signinResponse) => {
      if (signinResponse.type === "opaqueredirect") {
        // We should prevent multiple redirects.
        if (preventRedirect) {
          return Promise.reject(
            new ApiException(
              signinResponse,
              "Error occurred while trying to sign in."
            )
          );
        }

        // The server is redirecting to an authentication server.
        // We try to handle the redirect as elegantly as possible by opening authentication in a new tab.
        const popup = window.open(
          signinResponse.url,
          "_blank",
          this.POPOP_FEATURES
        );
        if (popup) {
          // Here we need the popup to be directed to a page that automatically closes.
          // Perhaps we can use some session storage in order to track authentication state.
          // We could tie in the auth configuration from the server into this storage to also consider when auth expires.
          return new Promise((res, rej) => {
            const interval = setInterval(() => {
              if (popup.closed) {
                clearInterval(interval);
                this.SignInAsync(true)
                  .then(() => res())
                  .catch((reason) => rej(reason));
              }
            }, 500);
          });
        } else {
          console.log("TODO: Add browser exception.");
          return Promise.reject();
          // throw new BrowserException(
          //   "Error opening authentication popup."
          // )
        }
      } else {
        if (signinResponse.ok) {
          // Everything succeeded silently, we can just return.
          return Promise.resolve();
        } else {
          // Some error occurred when accessing the server.
          return Promise.reject(
            new ApiException(
              signinResponse,
              "Error occurred while trying to sign in."
            )
          );
        }
      }
    });
  }
  static SignOutAsync(): Promise<void> {
    return fetch("/api/user/signout", {
      method: "GET",
    }).then((signoutResponse) => {
      if (signoutResponse.ok) {
        // Everything succeeded silently, we can just return.
        return Promise.resolve();
      } else {
        // Some error occurred when accessing the server.
        return Promise.reject(
          new ApiException(
            signoutResponse,
            "Error occurred while trying to sign out."
          )
        );
      }
    });
  }
}

export default UserApi;
