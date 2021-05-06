import SchemaForm from "components/ui/form/schema/SchemaForm";
import React, { Component } from "react";

export default class TestPage extends Component {
  render() {
    return (
      <div>
        <SchemaForm
          schema={{
            $schema: "https://json-schema.org/draft/2020-12/schema",
            $id: "https://example.com/product.schema.json",
            title: "Product",
            description: "A product in the catalog",
            type: "object",
            properties: {
              metaproduct: {
                type: "object",
                properties: {
                  productId: {
                    title: "ID",
                    description: "The unique identifier for a product",
                    type: "integer",
                  },
                  productAmount: {
                    title: "Amount",
                    description: "The amount of product there is",
                    type: "integer",
                  },
                },
              },
              productId: {
                title: "ID",
                description: "The unique identifier for a product",
                type: "integer",
              },
              productAmount: {
                title: "Amount",
                description: "The amount of product there is",
                type: "integer",
              },
            },
            required: ["productId"],
          }}
          onSubmit={(value: number) => console.log(value)}
        />
      </div>
    );
  }
}
