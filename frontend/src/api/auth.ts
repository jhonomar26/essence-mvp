import api from './client'

export interface AuthResponse {
  token: string
  user: { id: number; email: string; displayName: string }
}

export const login = (email: string, password: string) =>
  api.post<AuthResponse>('/auth/login', { email, password })

export const register = (email: string, password: string, displayName?: string) =>
  api.post<AuthResponse>('/auth/register', { email, password, displayName })
