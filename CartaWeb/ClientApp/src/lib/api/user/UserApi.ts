import { ApiException, BrowserException } from "lib/exceptions";

class UserApi {
  static get POPUP_EXPIRATION() {
    return 500;
  }
  static get POPOP_FEATURES() {
    return Object.entries({
      location: "no",
      toolbar: "no",
      width: 500,
      height: 500,
      left: 100,
      top: 100,
    })
      .map(([key, value]) => `${key}=${value}`)
      .join(",");
  }

  static async getUserInfo(): Promise<void> {
    // This should be an authorized API endpoint.
    // We need to determine what actions should be taken if the user is not logged in when calling this endpoint.
    // Perhaps we need an exception that represents an authentication error to indicate that a sign-in method should
    // eventually be called. Handling seems to be possible using the "instanceof" operator.
  }

  static async signInAsync(preventRedirect?: boolean): Promise<void> {
    const signinResponse = await fetch("/api/user/signin", {
      method: "GET",
      redirect: "manual",
    });
    if (signinResponse.type === "opaqueredirect") {
      // We should prevent multiple redirects.
      if (preventRedirect) {
        throw new ApiException(
          signinResponse,
          "Error occurred while redirecting during sign in."
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
        await new Promise<void>((res, rej) => {
          const interval = setInterval(() => {
            if (popup.closed) {
              clearInterval(interval);
              this.signInAsync(true)
                .then(() => res())
                .catch((reason) => rej(reason));
            }
          }, this.POPUP_EXPIRATION);
        });
      } else {
        throw new BrowserException(
          "Error opening authentication popup window."
        );
      }
    } else {
      if (signinResponse.ok) {
        // Everything succeeded silently, we can just return.
        return;
      } else {
        // Some error occurred when accessing the server.
        throw new ApiException(
          signinResponse,
          "Error occurred while trying to sign in."
        );
      }
    }
  }
  static async signOutAsync(): Promise<void> {
    const response = await fetch("/api/user/signout", {
      method: "GET",
    });
    if (response.ok) {
      // Everything succeeded silently, we can just return.
      return;
    } else {
      // Some error occurred when accessing the server.
      throw new ApiException(
        response,
        "Error occurred while trying to sign out."
      );
    }
  }
}

export default UserApi;
