/**
 * The type of element of an array.
 * @template T The type of the array.
 */
type Element<T> = T extends (infer U)[] ? U : never;

export default Element;
