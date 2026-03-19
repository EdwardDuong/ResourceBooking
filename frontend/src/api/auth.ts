import { apiRequest } from './client'
import type { AuthResult } from './types'

export function register(email: string, password: string): Promise<AuthResult> {
  return apiRequest<AuthResult>('/api/auth/register', {
    method: 'POST',
    body: { email, password },
    auth: false,
  })
}

export function login(email: string, password: string): Promise<AuthResult> {
  return apiRequest<AuthResult>('/api/auth/login', {
    method: 'POST',
    body: { email, password },
    auth: false,
  })
}
