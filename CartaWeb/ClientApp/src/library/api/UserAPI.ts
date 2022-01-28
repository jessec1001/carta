import queryString from "query-string";
import BaseAPI from "./BaseAPI";
import { User, UserSearcheableAttribute } from "./user";
import { BrowserException } from "library/exceptions";

/** Contains methods for accessing the Carta User API module. */
class UserAPI extends BaseAPI {
  /** The amount of time to wait for an authentication popup to display before aborting. */
  private PopupExpirationTimeout = 500;
  /** The display features of the authentication popup windows. */
  private PopupFeatures = Object.entries({
    location: "no",
    toolbar: "no",
    width: 500,
    height: 500,
    left: 100,
    top: 100,
  })
    .map(([key, value]) => `${key}=${value}`)
    .join(",");
  /** The time between authentication checks to the API. */
  private AuthenticationCheckInterval = 1000;

  protected getApiUrl() {
    return "/api/user";
  }

  /**
   * Determines whether the user is currently authenticated.
   * @returns `true` if the user is authenticated; otherwise, `false`.
   */
  public async isAuthenticated(): Promise<boolean> {
    const url = `${this.getApiUrl()}/authenticated`;
    const response = await fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to check authentication.",
      ["application/json"]
    );
    return await this.readJSON<boolean>(response);
  }

  /**
   * Retrieves information about the currently authenticated user.
   * @returns Information on the user.
   */
  public async getUserInfo(): Promise<User> {
    const url = `${this.getApiUrl()}`;
    const response = await fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch user information.",
      ["application/json"]
    );
    return await this.readJSON<User>(response);
  }
  /**
   * Retrives information about all users (limited to 256 entries) satisfying a optionally specified filter.
   * @param matchAttribute The attribute to use to filter users.
   * @param matchValue The value used to match against the specified attribute.
   * @param matchType The type of match to use in filtering.
   * - `"begin"` matches only the beginning of the string.
   * - `"end"` matches the entire string value.
   * @returns A list of information on users.
   */
  public async getUsersInfo(
    matchAttribute: UserSearcheableAttribute | null = null,
    matchValue: string | null = null,
    matchType: "exact" | "begin" = "begin"
  ): Promise<User[]> {
    const baseUrl = `${this.getApiUrl()}/users`;

    let response: Response;
    if (matchAttribute === null) {
      // We are not filtering the users so we make a more basic request.
      response = await fetch(baseUrl, this.defaultFetchParameters());
    } else {
      // We are filtering so we need to format and add the advanced request parameters.
      const attributeName = matchAttribute;
      const attributeValue = matchValue ?? "";
      let attributeFilter: "=" | "^=";
      switch (matchType) {
        case "exact":
          attributeFilter = "=";
          break;
        case "begin":
          attributeFilter = "^=";
          break;
      }

      const url = queryString.stringifyUrl({
        url: baseUrl,
        query: {
          attributeName,
          attributeValue,
          attributeFilter,
        },
      });
      response = await fetch(url, this.defaultFetchParameters());
    }

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch users information.",
      ["application/json"]
    );
    return await this.readJSON<User[]>(response);
  }
  /**
   * Retrieves information about all users within a specific group.
   * @param groupId The unique identifier of the group.
   * @returns A list of information on users.
   */
  public async getGroupUsersInfo(groupId: string): Promise<User[]> {
    const url = `${this.getApiUrl()}/group/${encodeURIComponent(groupId)}`;
    const response = await fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch user information.",
      ["application/json"]
    );
    return await this.readJSON<User[]>(response);
  }

  /**
   * Generates a popup and attempts to sign the user into the application.
   */
  public async signIn(): Promise<void> {
    const url = `${this.getApiUrl()}/signin`;
    const response = await fetch(url, {
      method: "GET",
      redirect: "manual",
    });

    if (response.type === "opaqueredirect") {
      // The server is redirecting to an authentication server.
      // We try to handle the redirect as elegantly as possible by opening authentication in a new window.
      const popup = window.open(response.url, "_blank", this.PopupFeatures);
      if (popup) {
        // Here we need the popup to be directed to a page that automatically closes.
        // We check on interval with the server to see if we have become authenticated which is a success.
        // We check on interval to see if the popup has been closed which is a failure.
        return await new Promise<void>((res, rej) => {
          const checkAuthIntervalId = setInterval(() => {
            // If the user became authenticated, close the popup automatically.
            this.isAuthenticated().then((authenticated) => {
              if (authenticated) {
                popup.close();
              }
            });
          }, this.AuthenticationCheckInterval);
          const checkPopupIntervalId = setInterval(() => {
            if (popup.closed) {
              // When the popup is closed, stop checking for authentication state.
              clearInterval(checkAuthIntervalId);
              clearInterval(checkPopupIntervalId);

              // If the user has become authenticated, we return successfully.
              // Else return an error on the user's behalf.
              this.isAuthenticated().then((authenticated) => {
                if (authenticated) res();
                else rej(new BrowserException("User authentication failed."));
              });
            }
          }, this.PopupExpirationTimeout);
        });
      } else {
        // If the authentication popup did not appear for some reason, we throw an error.
        throw new BrowserException(
          "Error opening authentication popup window."
        );
      }
    }

    // Check if sign in succeeded silently, so we can just return.
    await this.ensureSuccess(
      response,
      "Error occurred while trying to sign in."
    );
  }
  /**
   * Signs the user out of the application.
   */
  public async signOut(): Promise<void> {
    const url = `${this.getApiUrl()}/signout`;
    const response = await fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to sign out."
    );
  }
}

export default UserAPI;
