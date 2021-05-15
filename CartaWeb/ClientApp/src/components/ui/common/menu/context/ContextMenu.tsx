import React, { Component, createRef, RefObject, SyntheticEvent } from "react";

import "./ContextMenu.css";

export interface ContextMenuProps {
  visible: boolean;
  entries: { key: string; label: string }[];
  position: { x: number; y: number };

  onSelect?: (key: string) => void;
  onExit?: () => void;
}

export default class ContextMenu extends Component<ContextMenuProps> {
  ref: RefObject<HTMLUListElement>;

  constructor(props: ContextMenuProps) {
    super(props);

    this.ref = createRef();

    this.handleMouseClickOutside = this.handleMouseClickOutside.bind(this);
    this.handleMouseClickInside = this.handleMouseClickInside.bind(this);
  }

  handleMouseClickOutside(event: MouseEvent) {
    if (this.ref.current) {
      if (this.ref.current.contains(event.target as any)) return;
      if (this.props.onExit) this.props.onExit();
    }
  }
  handleMouseClickInside(event: SyntheticEvent, key: string) {
    if (this.props.onSelect) this.props.onSelect(key);
    if (this.props.onExit) this.props.onExit();
    event.stopPropagation();
    event.preventDefault();
  }

  componentDidMount() {
    document.addEventListener("mousedown", this.handleMouseClickOutside);
  }
  componentWillUnmount() {
    document.addEventListener("mousedown", this.handleMouseClickOutside);
  }

  render() {
    return (
      this.props.visible && (
        <ul
          className="context-menu"
          style={{
            left: this.props.position.x,
            top: this.props.position.y,
          }}
          ref={this.ref}
        >
          {this.props.entries.map((entry) => (
            <li
              key={entry.key}
              className="context-menu-item"
              onClick={(event) => this.handleMouseClickInside(event, entry.key)}
            >
              {entry.label}
            </li>
          ))}
        </ul>
      )
    );
  }
}
