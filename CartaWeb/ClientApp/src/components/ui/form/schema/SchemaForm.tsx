import JsonSchema, {
  JsonSchemaInteger,
  JsonSchemaObject,
  JsonSchemaString,
  JsonSchemaType,
} from "library/schema/JsonSchema";
import React, { ChangeEvent, Component, FormEvent, HTMLProps } from "react";

export interface SchemaFormProps extends HTMLProps<HTMLFormElement> {
  schema: JsonSchema;

  onChange?: (obj: any) => void;
  onSubmit?: (obj: any) => void;
}
export interface SchemaObjectFormProps {
  schema: JsonSchemaObject;

  onChange?: (value: any) => void;
}
export interface SchemaIntegerFormProps {
  schema: JsonSchemaInteger;

  onChange?: (value: number) => void;
}
export interface SchemaFormState {
  value: any;
}
export interface SchemaObjectFormState {
  value: any;
}
export interface SchemaIntegerFormState {
  value: number;
}

export default class SchemaForm extends Component<
  SchemaFormProps,
  SchemaFormState
> {
  constructor(props: SchemaFormProps) {
    super(props);

    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleChange = this.handleChange.bind(this);

    this.state = {
      value: null,
    };
  }

  handleSubmit(event: FormEvent<HTMLFormElement>) {
    if (this.props.onSubmit) {
      this.props.onSubmit(this.state.value);
      event.preventDefault();
    }
  }
  handleChange(value: any) {
    this.setState({
      value: value,
    });
  }

  render() {
    const { schema, ...restProps } = this.props;
    return (
      <form {...restProps} onSubmit={this.handleSubmit}>
        {this.renderProperty(schema)}
        <input type="submit" />
      </form>
    );
  }

  renderProperty(schema: JsonSchemaType): JSX.Element {
    switch (schema.type) {
      case "object":
        return (
          <SchemaObjectForm schema={schema} onChange={this.handleChange} />
        );
      case "integer":
        return (
          <SchemaIntegerForm schema={schema} onChange={this.handleChange} />
        );
      // case "integer":
      //   return this.renderInteger(schema);
      // case "string":
      //   return this.renderString(schema);
    }
    return null as any;
  }
}

export class SchemaObjectForm extends Component<
  SchemaObjectFormProps,
  SchemaObjectFormState
> {
  constructor(props: SchemaObjectFormProps) {
    super(props);

    this.handleChange = this.handleChange.bind(this);

    this.state = {
      value: {},
    };
  }

  handleChange(prop: string, value: any) {
    this.setState((state) => {
      const newValue = {
        ...state.value,
        [prop]: value,
      };

      if (this.props.onChange) {
        this.props.onChange(newValue);
      }

      return {
        value: newValue,
      };
    });
  }

  render() {
    const { schema } = this.props;
    const properties = schema.properties ?? {};
    return (
      <fieldset>
        <legend>{schema.title}</legend>
        {Object.entries(properties).map(([key, value]) => {
          return this.renderProperty(key, value);
        })}
        <p>{schema.description}</p>
      </fieldset>
    );
  }

  renderProperty(prop: string, schema: JsonSchemaType): JSX.Element {
    const handleChange = (value: any) => this.handleChange(prop, value);
    switch (schema.type) {
      case "object":
        return <SchemaObjectForm schema={schema} onChange={handleChange} />;
      case "integer":
        return <SchemaIntegerForm schema={schema} onChange={handleChange} />;
      // case "integer":
      //   return this.renderInteger(schema);
      // case "string":
      //   return this.renderString(schema);
    }
    return null as any;
  }
}

export class SchemaIntegerForm extends Component<
  SchemaIntegerFormProps,
  SchemaIntegerFormState
> {
  constructor(props: SchemaIntegerFormProps) {
    super(props);

    this.handleChange = this.handleChange.bind(this);

    this.state = {
      value: 0,
    };
  }

  handleChange(event: ChangeEvent<HTMLInputElement>) {
    this.setState({
      value: event.target.valueAsNumber,
    });
    if (this.props.onChange) {
      this.props.onChange(event.target.valueAsNumber);
    }
  }

  render() {
    const { schema } = this.props;
    return (
      <>
        <label>{schema.title}</label>
        <input
          type="number"
          onChange={this.handleChange}
          value={this.state.value}
        />
        <p>{schema.description}</p>
      </>
    );
  }
}
