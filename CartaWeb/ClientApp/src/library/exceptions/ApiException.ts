/** Represents an exception thrown when an API call fails. */
class ApiException {
  /** The URL of the API call that caused the exception. */
  public url: string;
  /** The HTTP status code that was returned. */
  public status: number;

  /** An optional message specified for the exception. */
  public message?: string;
  /** Additional data that may have been included in the response. */
  public data?: any;

  private constructor(response: Response, message?: string) {
    this.url = response.url;
    this.status = response.status;
    this.message = message;
  }

  /**
   * Creates a new API exception from a erraneous response and an optional exception message.
   * @param response The exception-causing response.
   * @param message The optional exception message.
   */
  public static async create(response: Response, message?: string) {
    const error = new ApiException(response, message);
    try {
      error.data = await response.json();
    } catch {}
    return error;
  }
}

export default ApiException;
