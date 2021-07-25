/**
 * A type for a generalized filter function which takes in a value and produces a boolean indicating whether the value
 * has passed or failed the filter.
 * @template T - The type of value.
 */
type Filter<T> = (value: T) => boolean;

export default Filter;
