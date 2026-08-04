export type LoginPayload = {
  email: string;
  password: string;
};

export type RegisterPayload = {
  email: string;
  password: string;
  displayName?: string;
};

export type AuthUser = {
  id: number;
  email: string;
  displayName: string | null;
};

export type AuthResponse = {
  token: string;
  user: AuthUser;
};
