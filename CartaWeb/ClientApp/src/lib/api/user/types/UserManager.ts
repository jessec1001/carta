import { EventEmitter } from "ee-ts";
import { UserApi } from "lib/api";
import { User } from ".";

interface UserEvents {
  signin: (user: User) => void;
  signout: () => void;
}

class UserManager extends EventEmitter<UserEvents> {
  #authTimestamp: number;
  #authExpiration: number;

  user: User | null;

  constructor() {
    super();

    this.#authTimestamp = 0;
    this.#authExpiration = 30 * 60 * 1000; // 30 minutes.

    this.user = null;
  }

  private updateAuthentication(user: User) {
    this.#authTimestamp = Date.now();
    this.user = user;
  }
  private clearAuthentication() {
    this.#authTimestamp = 0;
    this.user = null;
  }

  async IsAuthenticatedAsync() {
    if (Date.now() - this.#authTimestamp < this.#authExpiration) {
      return true;
    } else {
      try {
        await UserApi.signInAsync({});
        return true;
      } catch {
        return false;
      }
    }
  }
  async SignInAsync() {
    await UserApi.signInAsync({});
    const user = await UserApi.getUserInfoAsync();

    this.emit("signin", user);
  }
  async SignOutAsync() {
    await UserApi.signOutAsync();

    this.clearAuthentication();
    this.emit("signout");
  }
}

export default UserManager;
