import { CognitoUser } from "@aws-amplify/auth";

/** Represents the information stored about local users. */
interface LocalCognitoUser extends CognitoUser {
  /** Attributes of the user stored in the Cognito user pool. */
  attributes: {
    /** The organization that the user belongs to. */
    "custom:tenantId": string;
    /** The email address of the user. */
    email: string;
    /** The first name of the user. */
    given_name: string;
    /** The last name of the user. */
    family_name: string;
    /** The unique user identifier. */
    sub: string;
  };
  /** The username of the user. */
  username: string;
  /** Optional challenge - will be set if user needs to change password */
  challengeName?: string;
}
/** Represents the response from a verification code send response. */
interface LocalCognitoCodeDelivery {
  /** The name of the attribute used to send the verification code. */
  attributeName: string;
  /** The medium over which the verification code is delivered. */
  deliveryMedium: string;
  /** The destination relative to the medium for which the verication code is delivered to. */
  destination: string;
}

export type { LocalCognitoUser, LocalCognitoCodeDelivery };
