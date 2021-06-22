import { GeneralApi } from "library/api";
import { User } from "./types";
import { ApiException, BrowserException } from "library/exceptions";

class UserApi {
  static get POPUP_EXPIRATION() {
    return 500;
  }
  static get POPUP_REFRESH() {
    return 2000;
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

  @GeneralApi.route("GET", "api/user")
  static async getUserInfoAsync() {
    return (await GeneralApi.requestGeneralAsync()) as User;
  }

  @GeneralApi.route("GET", "api/user/authenticated")
  static async getIsUserAuthenticatedAsync() {
    return (await GeneralApi.requestGeneralAsync()) as boolean;
  }

  @GeneralApi.route("GET", "api/user/signin")
  static async signInAsync({
    preventRedirect,
  }: {
    preventRedirect?: boolean;
  }): Promise<void> {
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
        UserApi.POPOP_FEATURES
      );
      if (popup) {
        // Here we need the popup to be directed to a page that automatically closes.
        // Perhaps we can use some session storage in order to track authentication state.
        // We could tie in the auth configuration from the server into this storage to also consider when auth expires.
        await new Promise<void>((res, rej) => {
          const intervalAuth = setInterval(() => {
            UserApi.getIsUserAuthenticatedAsync().then((isAuthenticated) => {
              if (isAuthenticated) {
                clearInterval(intervalAuth);
                popup.close();
              }
            });
          }, UserApi.POPUP_REFRESH);
          const intervalClosed = setInterval(() => {
            if (popup.closed) {
              clearInterval(intervalClosed);
              UserApi.getIsUserAuthenticatedAsync().then((isAuthenticated) => {
                if (isAuthenticated) res();
                else rej();
              });
            }
          }, UserApi.POPUP_EXPIRATION);
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
  @GeneralApi.route("GET", "api/user/signout")
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
