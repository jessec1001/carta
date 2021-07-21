import "@testing-library/jest-dom";
import { fireEvent, render, screen } from "@testing-library/react";
import TextAreaInput, { TextAreaInputProps } from "./TextAreaInput";

const setup = (props?: TextAreaInputProps) => {
  const utils = render(<TextAreaInput data-testid="test" {...props} />);
  const element = screen.getByTestId("test");
  return {
    element,
    ...utils,
  };
};

test("ensure type assigned", () => {
  const { element } = setup();
  expect(element).toBeInTheDocument();
  expect((element as HTMLTextAreaElement).tagName).toBe("TEXTAREA");
  expect((element as HTMLTextAreaElement).type).toBe("textarea");
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
  fireEvent.change(element, { target: { value: "better\ntext" } });
  fireEvent.change(element, { target: { value: "best\n\ntext" } });
  expect(element).toHaveDisplayValue("test");
  expect(handleChange.mock.calls).toHaveLength(2);
  expect(handleChange.mock.calls[0][0]).toBe("better\ntext");
  expect(handleChange.mock.calls[1][0]).toBe("best\n\ntext");
});
test("uncontrolled value change fires", () => {
  const handleChange = jest.fn();
  const { element } = setup({ onChange: handleChange });
  expect(element).toBeInTheDocument();
  expect(element).toHaveDisplayValue("");
  fireEvent.change(element, { target: { value: "better\ntext" } });
  fireEvent.change(element, { target: { value: "best\n\ntext" } });
  expect(element).toHaveDisplayValue("best\n\ntext");
  expect(handleChange.mock.calls).toHaveLength(2);
  expect(handleChange.mock.calls[0][0]).toBe("better\ntext");
  expect(handleChange.mock.calls[1][0]).toBe("best\n\ntext");
});
