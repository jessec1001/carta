import { FunctionComponent } from "react";
import "./structure.css";

/** The props used for the {@link Section} component. */
interface SectionProps {
  title: string;
}

/** A component that renders a section of a document with a specific title. */
const Section: FunctionComponent<SectionProps> = ({
  title,
  children,
  ...props
}) => {
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
