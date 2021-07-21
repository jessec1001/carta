import { FunctionComponent } from "react";

import "./text.css";

/** The props used for the {@link Subsection} component. */
interface SubsectionProps {
  title: string;
}

/** A component that renders a subsection of a section of a document with a specific title. */
const Subsection: FunctionComponent<SubsectionProps> = ({
  title,
  children,
  ...props
}) => {
  return (
    <section className="subsection" {...props}>
      <h3 className="subsection-heading">{title}</h3>
      <div className="subsection-content">{children}</div>
    </section>
  );
};

export default Subsection;
export type { SubsectionProps };
