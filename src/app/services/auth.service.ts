import { Injectable, inject, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { firstValueFrom } from "rxjs";

export interface TmsUser {
  email: string;
  displayName: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

@Injectable({ providedIn: "root" })
export class AuthService {
  private http = inject(HttpClient);

  private accessToken = signal<string | null>(null);
  private refreshToken = signal<string | null>(null);

  currentUser = signal<TmsUser | null>(null);

  constructor() {
    this.restoreSession();
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  isAuthenticated(): boolean {
  return this.accessToken() !== null;
}

  hasRole(role: string): boolean {
    const user = this.currentUser();

    return user?.role === role || user?.role === "Admin";
  }

  async login(credentials: LoginRequest): Promise<void> {
    const res = await firstValueFrom(
      this.http.post<AuthResponse>(
        "/api/v1/auth/login",
        credentials
      )
    );

    this.setSession(res);
  }

  async refresh(): Promise<boolean> {
    const refreshToken = this.refreshToken();

    if (!refreshToken) {
      return false;
    }

    try {
      const res = await firstValueFrom(
        this.http.post<AuthResponse>(
          "/api/v1/auth/refresh",
          { refreshToken }
        )
      );

      this.setSession(res);

      return true;
    } catch {
      this.logout();
      return false;
    }
  }

  logout(): void {
    this.accessToken.set(null);
    this.refreshToken.set(null);
    this.currentUser.set(null);

    sessionStorage.removeItem("tms_access_token");
    sessionStorage.removeItem("tms_refresh_token");
  }

  private setSession(res: AuthResponse): void {
    console.log("Saving authentication session");

    this.accessToken.set(res.accessToken);
    this.refreshToken.set(res.refreshToken);

    sessionStorage.setItem(
      "tms_access_token",
      res.accessToken
    );

    sessionStorage.setItem(
      "tms_refresh_token",
      res.refreshToken
    );

    this.setCurrentUser(res.accessToken);
  }

  private restoreSession(): void {
    const accessToken = sessionStorage.getItem(
      "tms_access_token"
    );

    const refreshToken = sessionStorage.getItem(
      "tms_refresh_token"
    );

    if (!accessToken || !refreshToken) {
      return;
    }

    this.accessToken.set(accessToken);
    this.refreshToken.set(refreshToken);

    try {
      this.setCurrentUser(accessToken);
    } catch {
      this.logout();
    }
  }

  private setCurrentUser(token: string): void {
    const payload = JSON.parse(
      atob(token.split(".")[1])
    );

    this.currentUser.set({
      email: payload.email || payload.sub,
      displayName:
        payload.name ||
        payload.email ||
        "User",
      role:
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        payload.role ||
        "Student"
    });
  }
}




































// import { Injectable, inject, signal } from "@angular/core";
// import { HttpClient } from "@angular/common/http";
// import { firstValueFrom } from "rxjs";

// export interface TmsUser {
//   email: string;
//   displayName: string;
//   role: string;
// }

// export interface LoginRequest {
//   email: string;
//   password: string;
// }

// export interface AuthResponse {
//   accessToken: string;
//   refreshToken: string;
// }

// @Injectable({ providedIn: "root" })
// export class AuthService {
//   private http = inject(HttpClient);

//   private accessToken = signal<string | null>(null);

// currentUser = signal<TmsUser | null>(null);

// getAccessToken(): string | null {
//   return this.accessToken();
// }

// isAuthenticated(): boolean {
//   return this.accessToken() !== null && this.currentUser() !== null;
// }

// hasRole(role: string): boolean {
//   const user = this.currentUser();
//   return user?.role === role || user?.role === "Admin";
// }
//   async login(credentials: LoginRequest): Promise<void> {
//     const res = await firstValueFrom(
//       this.http.post<AuthResponse>(
//         "/api/v1/auth/login",
//         credentials
//       )
//     );

//     this.accessToken.set(res.accessToken);

//     const payload = JSON.parse(
//       atob(res.accessToken.split(".")[1])
//     );

//     this.currentUser.set({
//       email: payload.email || payload.sub,
//       displayName:
//         payload.name ||
//         payload.email ||
//         "User",
//       role:
//         payload[
//           "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
//         ] ||
//         payload.role ||
//         "Student"
//     });
//   }

//   logout(): void {
//     this.accessToken.set(null);
//     this.currentUser.set(null);
//   }
// }
