import React, {
  Component,
  Fragment,
  ReactElement,
  MouseEvent,
  createRef,
  RefObject,
} from "react";
import classNames from "classnames";

import SplitGutter from "./SplitGutter";

import "./SplitPane.css";

export interface SplitPaneProps {
  children?: ReactElement | ReactElement[];
  direction: "vertical" | "horizontal";
  initialSizes?: number[];
}

export interface SplitPaneState {
  dragging: null | number;
  childSizes: number[];
}

export default class SplitPane extends Component<
  SplitPaneProps,
  SplitPaneState
> {
  static displayName = SplitPane.name;

  containerRef: RefObject<HTMLDivElement>;
  gutterRefs: RefObject<HTMLDivElement>[];
  childrenRefs: RefObject<HTMLDivElement>[];

  constructor(props: SplitPaneProps) {
    super(props);

    // Create refs for all of the elements we need the metrics of.
    this.containerRef = createRef();
    this.gutterRefs = [];
    this.childrenRefs = [];
    for (let k = 0; k < this.getChildren().length; k++) {
      if (k !== 0) {
        this.gutterRefs.push(createRef());
      }
      this.childrenRefs.push(createRef());
    }

    this.state = {
      dragging: null,
      childSizes: this.childrenRefs.map(
        (_, index) => (props.initialSizes && props.initialSizes[index]) ?? 1
      ),
    };

    this.handleMouseDown = this.handleMouseDown.bind(this);
    this.handleMouseUp = this.handleMouseUp.bind(this);
    this.handleMouseEnter = this.handleMouseEnter.bind(this);
    this.handleMouseLeave = this.handleMouseLeave.bind(this);
    this.handleMouseMove = this.handleMouseMove.bind(this);
  }

  /** Gets an array of the children components passed to this component. */
  getChildren(): ReactElement[] {
    if (this.props.children) {
      if (Array.isArray(this.props.children)) {
        return this.props.children as ReactElement[];
      } else {
        return [this.props.children];
      }
    }
    return [];
  }

  handleMouseDown(event: MouseEvent) {
    for (let k = 0; k < this.gutterRefs.length; k++) {
      if (this.gutterRefs[k].current?.isSameNode(event.target as Node)) {
        this.setState({
          dragging: k,
        });
        event.stopPropagation();
        event.preventDefault();
      }
    }
  }
  handleMouseUp(event: MouseEvent) {
    if (this.state.dragging !== null) {
      this.setState({
        dragging: null,
      });
    }
  }
  handleMouseEnter(event: MouseEvent) {}
  handleMouseLeave(event: MouseEvent) {
    if (this.state.dragging !== null) {
      this.setState({
        dragging: null,
      });
    }
  }
  handleMouseMove(event: MouseEvent) {
    if (this.state.dragging !== null) {
      let containerSize: number = 0;
      if (this.containerRef.current) {
        // We use the container size to adjust the sizes of all the children using flex.
        containerSize =
          this.props.direction === "horizontal"
            ? this.containerRef.current.clientWidth
            : this.containerRef.current.clientHeight;

        // Proportionately scale up each component.
        // Note that the sum of children flex values should always equal the most recent dimension of the container.
        let totalSize: number = 0;
        let childSizes: number[] = [];
        for (let k = 0; k < this.childrenRefs.length; k++) {
          const childRef = this.childrenRefs[k];
          if (childRef.current) {
            const childSize = parseInt(childRef.current.style.flexGrow);
            totalSize += childSize;
            childSizes.push(childSize);
          } else {
            childSizes.push(0);
          }
        }
        if (containerSize !== totalSize) {
          for (let k = 0; k < childSizes.length; k++) {
            childSizes[k] *= containerSize / totalSize;
          }
        }

        // Adjust the child sizes to match where the gutter is being dragged to.
        let sumBefore: number = 0;
        let sumAfter: number = 0;
        for (let k = 0; k < childSizes.length; k++) {
          if (k < this.state.dragging) {
            sumBefore += childSizes[k];
          }
          if (k <= this.state.dragging + 1) {
            sumAfter += childSizes[k];
          }
        }
        let containerRect = this.containerRef.current?.getBoundingClientRect();
        const mouseDimension =
          this.props.direction === "horizontal"
            ? event.pageX - containerRect.left
            : event.clientY - containerRect.top;
        childSizes[this.state.dragging] = mouseDimension - sumBefore;
        childSizes[this.state.dragging + 1] = sumAfter - mouseDimension;

        // Set the new child size state.
        this.setState({
          childSizes: childSizes,
        });
      }
    }
  }

  render() {
    const children = this.getChildren();

    return (
      <div
        className={classNames("split-pane", this.props.direction)}
        onMouseDown={this.handleMouseDown}
        onMouseUp={this.handleMouseUp}
        onMouseEnter={this.handleMouseEnter}
        onMouseLeave={this.handleMouseLeave}
        onMouseMove={this.handleMouseMove}
        ref={this.containerRef}
      >
        {children.map((child, index) => (
          <Fragment key={index}>
            {/* Add a split gutter on every element but the first. */}
            {index > 0 && (
              <SplitGutter
                direction={this.props.direction}
                innerRef={this.gutterRefs[index - 1]}
              />
            )}
            <div
              className={classNames("split-pane-container")}
              style={{ flex: this.state.childSizes[index] }}
              ref={this.childrenRefs[index]}
            >
              {child}
            </div>
          </Fragment>
        ))}
      </div>
    );
  }
}
