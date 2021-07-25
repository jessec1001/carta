import { Filter } from "types";

/** The options used for the {@link ObjectFilter} class. */
interface ObjectFilterOptions {
  /** The default property name to check when no property name is specified. Defaults to "name". */
  defaultProperty: string;
  /** */
  mappedProperties: Map<string, string>;
  /** Whether to automatically try to convert a property name to plural by adding 's' or similar. */
  pluralAutomatic: boolean;
}

/** A filter capable of filtering through objects using a simple search language. */
class ObjectFilter implements ObjectFilterOptions {
  defaultProperty: string;
  mappedProperties: Map<string, string>;
  pluralAutomatic: boolean;

  /** The function that is used to filter elements. */
  private filterFunction: Filter<any>;

  constructor(
    pattern: string,
    {
      defaultProperty = "name",
      mappedProperties = new Map(),
      pluralAutomatic = true,
    }: ObjectFilterOptions
  ) {
    // Set the parameters.
    this.defaultProperty = defaultProperty;
    this.mappedProperties = mappedProperties;
    this.pluralAutomatic = pluralAutomatic;

    // Compile a filter function to filter objects more efficiently.
    this.filterFunction = this.compilePluralPattern(pattern);
  }

  /**
   * Attempts to make a word plural by altering the ending. Used for automatically detecting array properties.
   * @param word The word to convert to plural.
   * @returns The word converted to a plural form.
   */
  private makePlural(word: string) {
    // TODO: Use a more sophisticated algorithm to detect common patterns.
    return `${word}s`;
  }

  /**
   * Compiles a pattern from a set of namespaces (nested properties) and a value matcher.
   *
   * For instance can filter among common data types, property existance, and array inclusion.
   * @param namespaces A list that represents each property in the hierarchy that should be searched.
   * @param matcher A matcher that is used to compare against the resulting value after traversing the namespace tree.
   * @returns A function that filters objects.
   */
  private compileNamespaceMatcherPattern(
    namespaces: string[],
    matcher: string
  ): Filter<any> {
    // The namespaces determine the property chain to follow.
    // The matcher describes what we should be matched against.
    const filter = (value: any): boolean => {
      // Tunnel into the value filtering by namespace level.
      // For the following filters, the state of negate indicates whether a failure should filter successfully.
      let subvalue: any = value;
      for (let k = 0; k < namespaces.length; k++) {
        // If the value is not an object, we cannot filter it.
        if (typeof value !== "object") return false;

        // We try to determine a matching name by using predefined property mappings.
        let name = namespaces[k];
        while (this.mappedProperties.has(name))
          name = this.mappedProperties.get(name)!;

        // Check the singular form of the name.
        if (name in subvalue) {
          subvalue = subvalue[name];
          continue;
        }

        // Check the plural form of the name;
        if (this.pluralAutomatic) {
          name = this.makePlural(name);
          if (name in subvalue) {
            subvalue = subvalue[name];
            continue;
          }
        }

        // If the value is an array and we did not find a matching property, we search per object in the array.
        if (Array.isArray(subvalue)) {
          const arrayFilter = this.compileNamespaceMatcherPattern(
            namespaces.slice(k),
            matcher
          );
          return subvalue.some((arrayValue: any) => arrayFilter(arrayValue));
        }

        // Reaching this point means that the name did not match.
        return false;
      }

      // The nested value has been obtained. We now check it against the pattern value.
      if (Array.isArray(subvalue)) {
        // Non-singular array values are checked for inclusion.
        return subvalue
          .map((arrayValue) => arrayValue.toString())
          .includes(matcher);
      } else if (typeof subvalue === "object") {
        // Non-singular object values are checked against truthy values.
        subvalue = subvalue[matcher];
        return !!subvalue;
      } else {
        // Singular values are handled simply.
        let result: boolean = false;

        if (typeof subvalue === "string") {
          // Strings are checked for inclusion only.
          result = subvalue.includes(matcher);
        }
        if (typeof subvalue === "bigint" || typeof subvalue === "number") {
          // Numbers are checked for equality.
          result = subvalue === parseFloat(matcher);
        }
        if (typeof subvalue === "boolean") {
          // Booleans are checked for true/false-ness corresponding to the text.
          result = subvalue.toString() === matcher.toLowerCase();
        }
        if (typeof subvalue === "function") {
          // Functions are checked after calling.
          result = !!subvalue(matcher);
        }

        return result;
      }
    };
    return filter;
  }

  /**
   * Compiles a pattern that may contain multiple colon-delimited namespaces.
   *
   * For instance, converts a pattern such as `"abc:123"` to `"abc"` and `"123"` namespaces or
   * `"fruit:(apple banana cherry)"` to `"fruit"` and `"(apple banana cherry)"` namepsaces. Notice that colons within
   * quotes or parentheses do not separate namespaces. Also, negates any patterns that start with a negate `"-"` sign.
   * @param pattern The pattern to compile.
   * @returns A function that filters objects.
   */
  private compileSingularPattern(pattern: string): Filter<any> {
    // TODO: Account for parentheses to allow for grouping.

    // Determine if the pattern should be negated.
    let negate: boolean;
    if (pattern.length > 0 && pattern[0] === "-") {
      pattern = pattern.substring(1); // Remove the negate symbol if it exists.
      negate = true;
    } else {
      negate = false;
    }

    // Determine all namespaces in the string along with the final value.
    const namespaces: string[] = [];
    let matcher: string = "";

    // Quotes and parentheses get special treatment.
    let quotes: boolean = false;
    let parentheses: number = 0;
    for (let k = 0; k < pattern.length; k++) {
      // An opening parenthesis indicates the start of a group. We escape characters once inside a group.
      if (pattern[k] === "(" && !quotes) {
        if (parentheses > 0) matcher += pattern[k];
        parentheses = parentheses + 1;
        continue;
      }

      // A closing parenthesis indicates the end of a group. We stop escaping characters once outside a group.
      if (pattern[k] === ")" && !quotes) {
        parentheses = Math.max(0, parentheses - 1);
        if (parentheses > 0) matcher += pattern[k];
        continue;
      }

      // A quotation mark toggles a quoted region.
      if (pattern[k] === '"' && parentheses === 0) {
        quotes = !quotes;
        continue;
      }

      // An unquoted and ungrouped colon indicates a new namespace.
      if (pattern[k] === ":" && !quotes && parentheses === 0) {
        namespaces.push(matcher);
        matcher = "";
        continue;
      }

      // Add any other character to the matcher string.
      matcher += pattern[k];
    }

    // A lack of namespaces implies that we should be matching against the default property.
    if (namespaces.length === 0) namespaces.push(this.defaultProperty);

    // Return the filter (negated if necessary) compiled from the namespaces and matcher.
    const filter = this.compileNamespaceMatcherPattern(namespaces, matcher);
    return (value: any) => (negate ? !filter(value) : filter(value));
  }

  /**
   * Compiles a pattern that may contain multiple space-delimited subpatterns.
   *
   * For instance, converts a pattern such as `"abc:123 fruit:(apple banana cherry)"` to subpatterns `"abc:123"` and
   * `"fruit:(apple banana cherry)"`. Notice that space within quotes or parentheses do not separate subpatterns.
   * @param pattern The pattern to compile.
   * @returns A function that filters objects.
   */
  private compilePluralPattern(pattern: string): Filter<any> {
    // Trim the pattern so there is no extra whitespace surrounding it.
    // We perform a small hack to add a space at the end so that the subpattern list is complete.
    pattern = pattern.trim() + " ";

    // Split the pattern into subpatterns divided by spaces (ignoring double quotes).
    const subpatterns: string[] = [];
    let subpatternCurrent: string = "";

    // We toggle quotes on and off.
    // We nest parentheses inside of each other to any arbitrary depth.
    let subpatternQuotes: boolean = false;
    let subpatternParentheses: number = 0;

    for (let k = 0; k < pattern.length; k++) {
      // A starting parenthesis not within quotes indicates the start of a group.
      if (pattern[k] === "(" && !subpatternQuotes) {
        subpatternParentheses = subpatternParentheses + 1;
        subpatternCurrent += pattern[k];
        continue;
      }

      // A terminating parenthesis not within quotes indicates the end of a group.
      if (pattern[k] === ")" && !subpatternQuotes) {
        // Clamp the depth to keep consistency in case of user error.
        subpatternParentheses = Math.max(0, subpatternParentheses - 1);
        subpatternCurrent += pattern[k];
        continue;
      }

      // A quotation mark toggles a quoted region.
      if (pattern[k] === '"') {
        subpatternQuotes = !subpatternQuotes;
        subpatternCurrent += pattern[k];
        continue;
      }

      // An unquoted, unparenthesized space indicates a new subpattern if not empty.
      if (
        pattern[k] === " " &&
        !subpatternQuotes &&
        !(subpatternParentheses > 0)
      ) {
        if (subpatternCurrent.length === 0) continue;

        subpatterns.push(subpatternCurrent);
        subpatternCurrent = "";
      }

      // Any other character is treated normally.
      subpatternCurrent += pattern[k];
    }

    // We handle each subpattern as its own singular pattern that all need to match to pass filter.
    // This is equivalent to ANDing all of the filter subpatterns together.
    return (value: any) =>
      subpatterns
        .map((subpattern) => this.compileSingularPattern(subpattern))
        .every((subfilter) => subfilter(value));
  }

  /**
   * Filters a list of objects through this filter instance.
   * @param values The values to filter.
   * @returns The filtered values with values that failed to pass through the filter removed.
   */
  public filter(values: any[]): any[] {
    return values.filter(this.filterFunction);
  }
}

export default ObjectFilter;
export type { ObjectFilterOptions };
