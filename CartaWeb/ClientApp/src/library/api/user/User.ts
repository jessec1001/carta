/** Represents the identifying information of Carta authenticated user. */
interface User {
  /** The unique identifier for a user. */
  id: string;

  /** The usually unique username for the user. */
  name: string;
  /** The usually unique email for the user. */
  email: string;

  /** The user's first name. */
  firstName: string;
  /** The user's last name. */
  lastName: string;

  /** The groups that a user is part of. */
  groups: string[];
}

export default User;
