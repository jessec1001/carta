import { FunctionComponent, useEffect, useState } from "react";
import { useAPI, useControllableState, useSequentialRequest } from "hooks";
import {
  UserAPI,
  User,
  WorkspaceUser,
  UserSearcheableAttribute,
} from "library/api";
import { UserIcon } from "components/icons";
import { JoinContainer, JoinInputLabel } from "components/join";
import ComboboxInput from "../general/ComboboxInput";
import OptionInput from "../general/OptionInput";
import { Loading } from "components/text";

/**
 * Converts a query string entered by the user to a request for user information.
 * @param query The query string that will be used to search for users.
 * @param api The user API object.
 * @returns The request used to retrieve filtered users.
 */
const constructUserRequest = async (
  query: string,
  api: UserAPI
): Promise<User[]> => {
  /*
  Using Gitlab's conventions, the following determines how to search for a user.
  
  1. If prefixed by @ symbol, refers to beginning of username.
  2. If matches an email (i.e. @ symbol not at beginning), refers to exact email match.
  3. Otherwise, matches the name of a user with first name and last name separated by the first space.
  */
  const atSymbolIndex = query.indexOf("@");

  if (atSymbolIndex === 0) {
    // Case 1: Beginning of username.
    query = query.substring(1);
    return api.getUsersInfo(UserSearcheableAttribute.Name, query, "begin");
  } else if (atSymbolIndex > 0) {
    // Case 2: Exact email.
    return api.getUsersInfo(UserSearcheableAttribute.Email, query, "exact");
  } else {
    // Case 3: Beginning of name.
    const names = query.split(" ", 2);
    const [firstName, lastName] = names;
    if (lastName === undefined) {
      // If only the first name was specified, we search by that.
      // We allow any last name in this instance.
      return api.getUsersInfo(
        UserSearcheableAttribute.FirstName,
        firstName.toLowerCase(),
        "begin"
      );
    } else {
      // If the last name was also specified, we search by that primarily.
      // Secondarily, we check for containment of the first name.
      const value = await api.getUsersInfo(
        UserSearcheableAttribute.LastName,
        lastName.toLowerCase(),
        "begin"
      );
      return value.filter((user) =>
        (user.firstName ?? "").toLowerCase().includes(firstName.toLowerCase())
      );
    }
  }
};

/** The props used for the {@link UserInput} component. */
interface UserInputProps {
  /** The value that this option component takes on. Set to `null` if no value is selected. */
  value?: WorkspaceUser | null;

  /** The event handler for when the choice of user has changed. */
  onChange?: (value: WorkspaceUser | null) => void;
}

/**
 * A component that inputs a user using a searcheable combobox input.
 *
 * - A search starting with `@` will be interpreted as a username.
 * - A search formatted as an email will be interpreted as an email.
 * - Any other search is interpreted as a first and/or last name.
 */
const UserInput: FunctionComponent<UserInputProps> = ({
  value,
  onChange,
  children,
}) => {
  // We need to allow this component to be optionally controllable because we are not using a native UI element.
  let [actualValue, setValue] = useControllableState(null, value, onChange);
  if (actualValue && actualValue?.userInformation.id.length === 0)
    actualValue = null;

  // We need access to the workspace API to handle requests.
  const { userAPI } = useAPI();

  // We store the user query and its results.
  const [query, setQuery] = useState("");
  const [users, setUsers] = useState<User[] | null>(null);

  // We use a sequential request maker to update the queried users.
  const makeUserRequest = useSequentialRequest<User[]>(setUsers);

  // When the query has changed, we need to retrieve new user information.
  useEffect(() => {
    // We construct a specific request for users from the query.
    makeUserRequest(() => constructUserRequest(query, userAPI));
  }, [makeUserRequest, userAPI, query]);

  return (
    /* We render a combobox based on the current user query. */
    <JoinContainer direction="horizontal" grow="grow-last">
      <JoinInputLabel>
        <UserIcon />
      </JoinInputLabel>
      <ComboboxInput
        comparer={(user1?: WorkspaceUser, user2?: WorkspaceUser) => {
          if (!user1 || !user2) return false;
          return user1.userInformation.id === user2.userInformation.id;
        }}
        text={query}
        value={actualValue}
        onTextChanged={setQuery}
        onValueChanged={setValue}
      >
        {/* If the users have not loaded yet, display a loading symbol. */}
        {!users && (
          <OptionInput unselectable>
            <Loading />
          </OptionInput>
        )}

        {/* If there are no users found in the search, display as such. Notice that a non-value option works here. */}
        {users && users.length === 0 && (
          <OptionInput unselectable>No users found</OptionInput>
        )}

        {/* Each combobox option displays as `{firstName} {lastName} (@{userName})`. */}
        {users &&
          users.map((user) => {
            // We need to convert the user format the from User API into the format for the Workspace API.
            const value: WorkspaceUser = {
              userInformation: {
                id: user.id,
                name: user.name,
              },
            };

            return (
              <OptionInput key={user.id} value={value} alias={`@${user.name}`}>
                {user.firstName} {user.lastName}{" "}
                <span style={{ color: "var(--color-stroke-lowlight)" }}>
                  (@{user.name})
                </span>
              </OptionInput>
            );
          })}
      </ComboboxInput>
    </JoinContainer>
  );
};

export default UserInput;
export type { UserInputProps };
