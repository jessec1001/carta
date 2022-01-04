/**
 * The basic properties of every input field.
 */
interface IBaseInputProps<TValue> {
  /** The current value of the the input field. If not specified, the component is uncontrolled. */
  value?: TValue;
  /** The event handler that is called when the value of the input field is changed. */
  onChange?: (value: TValue) => void;
}

export default IBaseInputProps;
