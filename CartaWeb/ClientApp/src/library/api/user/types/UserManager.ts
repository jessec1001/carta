import { EventEmitter } from "ee-ts";
import { UserApi } from "library/api";
import Logging, { LogSeverity } from "library/logging";
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

    this.isAuthenticatedAsync = this.isAuthenticatedAsync.bind(this);
    this.signInAsync = this.signInAsync.bind(this);
    this.signOutAsync = this.signOutAsync.bind(this);

    this.user = null;
    this.loadAuthentication();

    window.addEventListener("storage", ({ key, storageArea }) => {
      if (storageArea === localStorage && key === "auth-timestamp") {
        this.loadAuthentication();
      }
    })
  }

  private loadAuthentication() {
    const timestamp = localStorage.getItem("auth-timestamp");
    const user = localStorage.getItem("auth-user");

    if (timestamp && user) {
      this.#authTimestamp = JSON.parse(timestamp) as number;
      this.user = JSON.parse(user) as User;

      setTimeout(() => this.emit("signin", this.user as User), 0);
    } else setTimeout(() => this.emit("signout"), 0);
  }
  private updateAuthentication(user: User) {
    this.#authTimestamp = Date.now();
    this.user = user;

    localStorage.setItem(
      "auth-timestamp",
      JSON.stringify(this.#authTimestamp)
    );
    localStorage.setItem("auth-user", JSON.stringify(this.user));
  }
  private clearAuthentication() {
    this.#authTimestamp = 0;
    this.user = null;

    localStorage.removeItem("auth-timestamp");
    localStorage.removeItem("auth-user");
  }

  async isAuthenticatedAsync() {
    if (Date.now() - this.#authTimestamp < this.#authExpiration) {
      return true;
    } else {
      try {
        await UserApi.signInAsync({ preventRedirect: true });
        return true;
      } catch {
        return false;
      }
    }
  }
  async signInAsync() {
    if (!(await this.isAuthenticatedAsync())) await UserApi.signInAsync({});
    const user = await UserApi.getUserInfoAsync();

    Logging.log({
      severity: LogSeverity.Info,
      source: "User Manager",
      title: "Successfully Signed In",
    });

    this.updateAuthentication(user);
    this.emit("signin", user);
  }
  async signOutAsync() {
    await UserApi.signOutAsync();

    Logging.log({
      severity: LogSeverity.Info,
      source: "User Manager",
      title: "Successfully Signed Out",
    });

    this.clearAuthentication();
    this.emit("signout");
  }
}

export default UserManager;
