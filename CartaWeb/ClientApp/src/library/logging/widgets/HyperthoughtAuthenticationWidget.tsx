import React from "react";
import { Link } from "react-router-dom";

function HyperthoughtAuthenticationWidget() {
  return (
    <span>
      Your HyperThought&trade; API access key is either missing or expired.
      Please enter a new API access key in your
      <Link
        to="/user/profile/#profile-integration-hyperthought"
        target="_blank"
      >
        profile
      </Link>
      .
    </span>
  );
}

export default HyperthoughtAuthenticationWidget;
