import { Auth } from "aws-amplify";
import { ApiException } from "library/exceptions";

class BaseAPI {
  /**
   * Gets the URL of the API. Endpoint routes should be appended onto the end of this URL to form endpoint URLs.
   * @returns The base URL of the API.
   */
  public getApiUrl() {
    return "/api/";
  }

  /**
   * Retrieves the default parameters for an API request.
   * @param method The HTTP method verb to use.
   * @param body An optional body to include in the request.
   * @returns The parameters to the fetcher.
   */
  public defaultFetchParameters(
    method: string = "GET",
    body?: any
  ): RequestInit {
    let parameters: RequestInit = { method };

    if (body !== undefined) {
      parameters.body = JSON.stringify(body);
      parameters.headers = {
        "Content-Type": "application/json",
      };
    }

    return parameters;
  }

  /**
   * Wraps the fetch function to add the default fetch parameters.
   * @param input The input to the fetch function.
   * @param init The initializer to the fetch function.
   * @returns The result of the fetch function.
   */
  public async fetch(
    input: RequestInfo,
    init?: RequestInit
  ): Promise<Response> {
    // Initialize the request.
    init = init ?? {};

    // Attach the identity JWT if it is set.
    try {
      const session = await Auth.currentSession();
      const idToken = session.getIdToken().getJwtToken();
      init.headers = {
        ...init.headers,
        Authorization: `Bearer ${idToken}`,
      };
    } catch {}

    // Make the request.
    return fetch(input, init);
  }

  /**
   * Ensures that an HTTP response has a successful status code (200-299). If not, raises an {@link ApiException}.
   * @param response The response to check.
   * @param errorMessage The error message that should be passed to the exception.
   * @param contentType The type of content that is expected in the response. If not specified, does not check the content type.
   */
  public async ensureSuccess(
    response: Response,
    errorMessage: string,
    contentTypes?: string[]
  ) {
    // Check if the response was okay.
    if (!response.ok) {
      throw await ApiException.create(response, errorMessage);
    }

    // Check if the content type is correct.
    if (contentTypes) {
      const responseType = response.headers.get("content-type");
      if (!contentTypes.some((type) => responseType?.includes(type))) {
        throw await ApiException.create(
          response,
          `Expected a content type in [${contentTypes.join(
            ", "
          )}] but got ${responseType}.`
        );
      }
    }
  }
  /**
   * Reads a JSON value from a response.
   * If the response has no body, throws an {@link ApiException}.
   * If the response has an empty body, throws an {@link ApiException}.
   * @param response The response to read JSON from.
   * @returns The parsed JSON object.
   */
  public async readJSON<T>(response: Response): Promise<T> {
    // Check that response body is set.
    if (response.body === null)
      throw await ApiException.create(response, "No response body to be read.");

    // Check if the response body is empty.
    const text = await response.text();
    if (text.length === 0)
      throw await ApiException.create(response, "Response body was empty.");

    // Return the correctly-typed JSON version.
    return JSON.parse(text) as T;
  }
  /**
   * Writes a JSON value to a string.
   * @param object The object to write to a string.
   * @returns The stringified JSON object.
   */
  public writeJSON<T>(object: T): string {
    // Simply convert using standard JSON stringify.
    return JSON.stringify(object);
  }
}

export default BaseAPI;
