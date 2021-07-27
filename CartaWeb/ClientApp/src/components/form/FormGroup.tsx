import { FunctionComponent } from "react";
import classNames from "classnames";
import ReactMarkdown from "react-markdown";

import "./form.css";

/** The props used for the {@link FormGroup} component. */
interface FormGroupProps {
  density?: "dense" | "sparse";

  error?: Error;

  title?: string;
  description?: string;
}

/** A component that displays inputs with a label and a description. */
const FormGroup: FunctionComponent<FormGroupProps> = ({
  density,
  error,
  title,
  description,
  children,
  ...props
}) => {
  // We display the form group differently depending on whether it is supposed to be rendered "dense" or "sparse".
  // By default, we will use "sparse" because it provides a more descriptive experience.
  const actualDensity = density ?? "sparse";
  switch (actualDensity) {
    case "dense":
      return (
        <label className={classNames("form-group", actualDensity)} {...props}>
          {/* TODO: Add tooltip functionality. */}
          {/* <Tooltip className="form-group-tooltip" anchor="top left">
            {description}
          </Tooltip> */}
          {title && <span className="form-group-label">{title}</span>}
          <span className="form-group-content">
            {children}
            {error !== undefined && (
              <small className="form-group-error">{error.message}</small>
            )}
          </span>
        </label>
      );
    case "sparse":
      return (
        <label className={classNames("form-group", actualDensity)} {...props}>
          <span className="form-group-label">{title}</span>
          {description && (
            <span className="form-group-description">
              <ReactMarkdown
                linkTarget="_blank"
                components={{
                  a: ({ children, href }) => {
                    return (
                      <a
                        className="link"
                        href={href as string}
                        target="_blank"
                        rel="noreferrer"
                      >
                        {children}
                      </a>
                    );
                  },
                }}
              >
                {description ?? ""}
              </ReactMarkdown>
            </span>
          )}
          <span className="form-group-content">
            {children}
            {error !== undefined && (
              <small className="form-group-error">{error.message}</small>
            )}
          </span>
        </label>
      );
  }
};

// Export React component and props.
export default FormGroup;
export type { FormGroupProps };
