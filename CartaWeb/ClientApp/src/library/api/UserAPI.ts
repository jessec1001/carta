import queryString from "query-string";
import { User, UserSearcheableAttribute } from "./user";
import BaseAPI from "./BaseAPI";

/** Contains methods for accessing the Carta User API module. */
class UserAPI extends BaseAPI {
  public getApiUrl() {
    return "/api/user";
  }

  /**
   * Determines whether the user is currently authenticated.
   * @returns `true` if the user is authenticated; otherwise, `false`.
   */
  public async isAuthenticated(): Promise<boolean> {
    const url = `${this.getApiUrl()}/authenticated`;
    const response = await this.fetch(url, this.defaultFetchParameters());

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
    const response = await this.fetch(url, this.defaultFetchParameters());

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
      response = await this.fetch(baseUrl, this.defaultFetchParameters());
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
      response = await this.fetch(url, this.defaultFetchParameters());
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
    const response = await this.fetch(url, this.defaultFetchParameters());

    await this.ensureSuccess(
      response,
      "Error occurred while trying to fetch user information.",
      ["application/json"]
    );
    return await this.readJSON<User[]>(response);
  }
}

export default UserAPI;
