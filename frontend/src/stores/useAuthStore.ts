import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthUser } from '../features/auth/types/auth';

type AuthState = {
  token: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
  setSession: (token: string, user: AuthUser) => void;
  clear: () => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,
      setSession: (token, user) => set({ token, user, isAuthenticated: true }),
      clear: () => set({ token: null, user: null, isAuthenticated: false }),
    }),
    { name: 'auth' },
  ),
);
