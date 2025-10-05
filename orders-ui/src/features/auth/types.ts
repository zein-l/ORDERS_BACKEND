export type AuthUser = {
  id: string;
  email: string;
  fullName?: string | null;
};

export type AuthResponse = {
  accessToken: string;
  user: AuthUser;
};
