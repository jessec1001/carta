import React, { Component } from 'react';
import './HeightScroll.css';

export class HeightScroll extends Component {
    static displayName = HeightScroll.name;

    render() {
        return (
            <div className={`height-scroll-container`}>
                <div className={`height-scroll ${this.props.className}`} style={this.props.style}>
                    {this.props.children}
                </div>
            </div>
        );
    }
}