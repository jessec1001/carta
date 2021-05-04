import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import { PrismWrapper } from "components/wrappers";
import { CopyButton } from "../../clipboard";
import "../Code.css";

export interface CodeExampleProps extends HTMLProps<HTMLDivElement> {
  code: string;
  language: string;
  inline?: boolean;
  copyable?: boolean;
  executable?: boolean;

  /** Called before code is executed. Should return false if the default action should be prevented. */
  onExecute?: (result: any) => void;
}

export default class CodeExample extends Component<CodeExampleProps> {
  constructor(props: CodeExampleProps) {
    super(props);
    this.handleExecute = this.handleExecute.bind(this);
  }

  executeJavaScriptCode(code: string): any {
    // eslint-disable-next-line no-new-func
    const func = new Function(code);
    return func();
  }

  handleExecute() {
    // Perform code execution depending on the language.
    let result;
    switch (this.props.language) {
      case "javascript":
      case "js":
        result = this.executeJavaScriptCode(this.props.code);
        break;
    }
    if (this.props.onExecute) {
      this.props.onExecute(result);
    }
  }

  render() {
    const {
      code,
      language,
      inline,
      copyable,
      executable,
      onExecute,
      className,
      ...rest
    } = this.props;

    return (
      <div {...rest} className={classNames({ "flex-row": inline }, className)}>
        <div
          className="relative"
          style={{ flexGrow: inline ? 1 : 0, overflow: "auto" }}
        >
          {copyable && (
            <span className="push-ne">
              <CopyButton className="hover-light" contents={code} />
            </span>
          )}
          <PrismWrapper
            code={code}
            language={language}
            plugins={["line-numbers"]}
            className={classNames(
              "code-block",
              { "merge-bottom": executable && !inline },
              { "merge-right": executable && inline },
              { inline: inline }
            )}
          />
        </div>
        {executable && (
          <button
            className={classNames(
              { "merge-top": !inline },
              { "merge-left": inline },
              "button",
              { "button-block": !inline },
              "bg-action"
            )}
            onClick={this.handleExecute}
          >
            Execute
          </button>
        )}
      </div>
    );
  }
}
