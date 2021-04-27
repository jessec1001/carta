import { EventEmitter } from "ee-ts";
import UserApi from "../api/UserApi";

interface UserEvents {
  signin: () => void;
  signout: () => void;
}

class UserManager extends EventEmitter<UserEvents> {
  #authTimestamp: number;
  #authExpiration: number;

  user: any;

  constructor() {
    super();

    this.#authTimestamp = 0;
    this.#authExpiration = 30 * 60 * 1000; // 30 minutes.
  }

  async IsAuthenticated() {}
  async SignIn() {
    UserApi.SignInAsync()
      .then(() => {})
      .catch((err) => {});
  }
  async SignOut() {}
}

export default UserManager;
