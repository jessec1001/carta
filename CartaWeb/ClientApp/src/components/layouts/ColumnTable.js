import React, { Component } from 'react';
import { Table } from 'reactstrap';

export class ColumnTable extends Component {
    static displayName = ColumnTable.name;

    render() {
        return (
            <Table>
                <thead>
                    <tr className="d-flex">
                        {this.props.headers.map(header =>
                            <th key={header} className="col">{header}</th>
                        )}
                    </tr>
                </thead>
                <tbody>
                    {this.props.values.map((value, index) =>
                        <tr key={index} className="d-flex">
                            {this.props.headers.map(header =>
                                <td key={header} className="col">{value[header.toLowerCase()]}</td>
                            )}
                        </tr>   
                    )}
                </tbody>
            </Table>
        );
    }
}