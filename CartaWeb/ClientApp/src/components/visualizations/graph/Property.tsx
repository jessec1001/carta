import { Component, MouseEvent } from "react";
import { Property as GraphProperty } from "library/api/data";
import { ObservationList } from "./ObservationList";
import "./Property.css";
import { PropertyList } from "./PropertyList";
import { CaretIcon } from "components/icons";

interface PropertyProps {
  property: GraphProperty;
  selected?: boolean;

  onClick?: (event: MouseEvent) => void;
}

interface PropertyState {
  expanded: boolean;
}

export class PropertyItem extends Component<PropertyProps, PropertyState> {
  static displayName = PropertyItem.name;

  constructor(props: PropertyProps) {
    super(props);

    this.state = {
      expanded: false,
    };

    this.handleExpand = this.handleExpand.bind(this);
  }

  handleExpand() {
    this.setState((state) => ({
      expanded: !state.expanded,
    }));
  }

  render() {
    let className = "property-item";
    if (this.props.selected) className += " selected";
    if (this.props.onClick) className += " clickable";

    return (
      <div>
        <div className={className} onClick={this.props.onClick}>
          <p className="property-item-name">{this.props.property.id}:</p>
          <div className="property-item-occurrences">
            ×{this.props.property.values.length} &nbsp;
            <div
              className="d-inline property-expand"
              onClick={this.handleExpand}
            >
              {!this.state.expanded && <CaretIcon direction="down" />}
              {this.state.expanded && <CaretIcon direction="up" />}
            </div>
          </div>
        </div>
        {this.state.expanded && (
          <div className="indented">
            {this.props.property.properties && (
              <PropertyList properties={this.props.property.properties} />
            )}
            <ObservationList observations={this.props.property.values} />
          </div>
        )}
      </div>
    );
  }
}
