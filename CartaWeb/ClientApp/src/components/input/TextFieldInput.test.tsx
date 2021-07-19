import "@testing-library/jest-dom";
import { fireEvent, render, screen } from "@testing-library/react";
import TextFieldInput, { TextFieldInputProps } from "./TextFieldInput";

const setup = (props?: TextFieldInputProps) => {
  const utils = render(<TextFieldInput data-testid="test" {...props} />);
  const element = screen.getByTestId("test");
  return {
    element,
    ...utils,
  };
};

test("ensure type assigned", () => {
  const { element } = setup();
  expect(element).toBeInTheDocument();
  expect(element).toHaveAttribute("type", "text");
});
test("ensure class assigned", () => {
  const { element } = setup();
  expect(element).toBeInTheDocument();
  expect(element).toHaveClass("form-control");
});
test("controlled value change fires", () => {
  const handleChange = jest.fn();
  const { element } = setup({ value: "test", onChange: handleChange });
  expect(element).toBeInTheDocument();
  expect(element).toHaveDisplayValue("test");
  fireEvent.change(element, { target: { value: "better text" } });
  fireEvent.change(element, { target: { value: "best text" } });
  expect(element).toHaveDisplayValue("test");
  expect(handleChange.mock.calls).toHaveLength(2);
  expect(handleChange.mock.calls[0][0]).toBe("better text");
  expect(handleChange.mock.calls[1][0]).toBe("best text");
});
test("uncontrolled value change fires", () => {
  const handleChange = jest.fn();
  const { element } = setup({ onChange: handleChange });
  expect(element).toBeInTheDocument();
  expect(element).toHaveDisplayValue("");
  fireEvent.change(element, { target: { value: "better text" } });
  fireEvent.change(element, { target: { value: "best text" } });
  expect(element).toHaveDisplayValue("best text");
  expect(handleChange.mock.calls).toHaveLength(2);
  expect(handleChange.mock.calls[0][0]).toBe("better text");
  expect(handleChange.mock.calls[1][0]).toBe("best text");
});
