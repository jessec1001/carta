import queryString from "query-string";
import { ApiException } from "lib/exceptions";

class GeneralApi {
  static $routes: {
    method: string;
    url: string;
    func: (...args: any[]) => any;
  }[] = [];
  static $method: string[] = [];
  static $url: string[] = [];

  private static formatRequest(request?: RequestInit): RequestInit {
    // Specify the default request properties.
    // Note that the method should be loaded from the state if possible.
    let formattedRequest: RequestInit = {
      method:
        this.$method.length > 0 ? this.$method[this.$method.length - 1] : "GET",
      headers: {
        "Content-Type": "application/json",
      },
    };

    // Return the default properties overidden by the specified properties.
    return { ...formattedRequest, ...request };
  }
  private static formatUrl(
    parameters?: Record<string, any>,
    url?: string
  ): string {
    // Get the currently set URL if one exists and a URL was not specified.
    let formattedUrl =
      url ?? this.$url.length > 0 ? this.$url[this.$url.length - 1] : "/api";

    if (parameters) {
      // Replace placeholders in the URL with the prescribed values.
      let extraParameters = parameters;
      Object.entries(parameters).forEach(([key, value]) => {
        const regex = RegExp(`\\{(${key})\\??(=.*?)?\\}`, "g");
        if (formattedUrl.match(regex)) {
          formattedUrl = formattedUrl.replace(regex, value);
          delete extraParameters[key];
        }
      });

      // Format the query parameters into the URL.
      formattedUrl = queryString.stringifyUrl({
        url: formattedUrl,
        query: extraParameters,
      });
    }

    return formattedUrl;
  }

  static route(method: string, url: string) {
    const wrapper = (
      target: any,
      key: string | symbol,
      descriptor: TypedPropertyDescriptor<(...args: any[]) => any>
    ) => {
      // If the descriptor is a function value, we wrap it.
      const original = descriptor.value;
      if (original) {
        const func = (...args: any[]) => {
          // Setup method and URL.
          this.$method.push(method);
          this.$url.push(url);

          // Original call.
          const result = original(...args);

          // Teardown method and URL.
          this.$method.pop();
          this.$url.pop();

          return result;
        };

        // We make sure to add this wrapped function to our routes.
        this.$routes.push({
          method,
          url,
          func,
        });

        // Return a modified descriptor.
        return {
          ...descriptor,
          value: func,
        } as TypedPropertyDescriptor<(...args: any[]) => any>;
      } else {
        return descriptor;
      }
    };
    return wrapper;
  }

  static async requestGeneralAsync(
    apiParameters?: Record<string, any>,
    fetchParameters?: RequestInit,
    url?: string
  ) {
    // Get the formatted fetch parameters.
    const formattedUrl = this.formatUrl(apiParameters, url);
    const formattedRequest = this.formatRequest(fetchParameters);

    // Make the general request.
    // Return the JSON data upon success and the an exception upon failure.
    const response = await fetch(formattedUrl, formattedRequest);
    if (response.ok) {
      return response.json();
    } else {
      throw new ApiException(response);
    }
  }
  static requestUnknownAsync(
    apiParameters?: Record<string, any>,
    fetchParameters?: RequestInit,
    url?: string
  ) {
    // Try to find a route that matches the signature of the passed in call.
    const route = this.$routes.find(
      (route) => route.method === fetchParameters?.method && route.url === url
    );

    if (route) {
      // We have found a matching route that we can call.
      return route.func({ ...apiParameters });
    } else {
      // If no known function in this library was found, just treat this API call normally.
      return this.requestGeneralAsync(apiParameters, fetchParameters, url);
    }
  }
}

export default GeneralApi;
