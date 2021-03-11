import { Component, ReactNode } from "react";

export interface TabProps {
  children?: ReactNode | ReactNode[];

  icon?: JSX.Element;
  label: string;
  closable?: boolean;
  selected?: boolean;

  onContextMenu?: (event: any) => void;
}

export default class Tab extends Component<TabProps> {
  static displayName = Tab.name;

  render() {
    return this.props.children || null;
  }
}
