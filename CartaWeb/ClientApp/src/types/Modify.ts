/**
 * Modifies an existing type by mergining the properties of another type and overwriting
 * properties that existed in the original type.
 * @template T - The type to modify.
 * @template U - The overriding type to merge.
 */
type Modify<T, U> = Omit<T, keyof U> & U;

export default Modify;
