import { FunctionComponent } from "react";

import "./text.css";

/** The props used for the {@link Section} component. */
interface SectionProps {
  /** The title of the section. */
  title: string;
}

/** A component that renders a section of a document with a specific title. */
const Section: FunctionComponent<SectionProps> = ({
  title,
  children,
  ...props
}) => {
  // TODO: Refactor section into a component with a context that adjusts headings based on depth
  // and automatically generates anchor identifiers that can be used to create a navigation.
  return (
    <section className="section" {...props}>
      <h2 className="section-heading">{title}</h2>
      <hr />
      <div className="section-content">{children}</div>
    </section>
  );
};

export default Section;
export type { SectionProps };
