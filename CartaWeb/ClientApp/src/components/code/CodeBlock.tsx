import { ComponentProps, FC } from "react";

/** The props used for the {@link CodeBlock} component. */
interface CodeBlockProps extends ComponentProps<"div"> {
  /** The code to render in the code block. */
  code?: string;
  /** The code language to use for syntax highlighting. */
  language?: string;

  /** Whether to display a button to copy code. */
  copyable?: boolean;
  /** Whether to display a button to paste code. */
  pasteable?: boolean;
  // TODO: For now, we will not support executing code blocks.
  /** Whether to display a button to execute code. */
  executable?: boolean;

  // TODO: For now, we will not support editing code blocks.
  /** Whether the code in the code block should be modifiable by the user. */
  editable?: boolean;

  /** An event listener that is called whenever the code is edited. */
  onEdit?: (code: string) => void;
  /** An event listener that is called whenever the code is executed. */
  onExecute?: (code: string) => void;
}

/** A component that renders a block of text that supports syntax highlighting and modification. */
const CodeBlock: FC<CodeBlockProps> = ({
  code,
  language,
  copyable,
  pasteable,
  editable,
  executable,
  onEdit,
  onCopy,
  onPaste,
  onExecute,
}) => {
  return <div></div>;
};

export default CodeBlock;
export type { CodeBlockProps };
