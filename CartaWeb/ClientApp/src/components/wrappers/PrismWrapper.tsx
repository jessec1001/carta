import React, { Component, createRef, RefObject } from "react";
import classNames from "classnames";

// We need to manually import all of the necessary Prism modules.
import Prism from "prismjs";
import "prismjs/themes/prism-okaidia.css";
import "prismjs/components/prism-javascript";
import "prismjs/components/prism-json";
import "prismjs/components/prism-python";
import "prismjs/plugins/line-numbers/prism-line-numbers";

// TODO: Change to using prism-react-renderer library.
export interface PrismWrapperProps extends React.HTMLProps<HTMLPreElement> {
  code: string;
  language: string;
  plugins?: string[];
}

export default class PrismWrapper extends Component<PrismWrapperProps> {
  ref: RefObject<HTMLElement>;

  constructor(props: PrismWrapperProps) {
    super(props);
    this.ref = createRef();
    this.highlight = this.highlight.bind(this);
  }

  highlight() {
    // The code is re-highlighted when the component is mounted or updated.
    if (this.ref.current) {
      Prism.highlightElement(this.ref.current);
    }
  }

  componentDidMount() {
    this.highlight();
  }
  componentDidUpdate() {
    this.highlight();
  }

  render() {
    const { code, language, plugins, children, className, ...rest } =
      this.props;

    return (
      <pre
        {...rest}
        className={classNames(className, ...(plugins ? plugins : []))}
      >
        <code className={`language-${language}`} ref={this.ref}>
          {code}
        </code>
      </pre>
    );
  }
}
