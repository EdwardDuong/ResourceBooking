import { createContext, useContext, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { login as apiLogin, register as apiRegister } from '../api/auth'
import { getToken, setToken as persistToken } from '../api/client'
import type { UserRole } from '../api/types'

export interface AuthUser {
  userId: string
  email: string
  role: UserRole
}

interface AuthContextValue {
  user: AuthUser | null
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

// Decodes the token's claims for display purposes only (restoring a session
// after a page reload) - the backend re-validates the signature on every
// request, so nothing security-relevant depends on this being tamper-proof.
function decodeUser(token: string): AuthUser | null {
  try {
    const payload = token.split('.')[1]
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const padded = normalized.padEnd(normalized.length + ((4 - (normalized.length % 4)) % 4), '=')
    const claims = JSON.parse(atob(padded)) as Record<string, string>
    return { userId: claims.sub, email: claims.email, role: claims.role as UserRole }
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const token = getToken()
    return token ? decodeUser(token) : null
  })

  const login = async (email: string, password: string) => {
    const result = await apiLogin(email, password)
    persistToken(result.token)
    setUser({ userId: result.userId, email: result.email, role: result.role })
  }

  const register = async (email: string, password: string) => {
    const result = await apiRegister(email, password)
    persistToken(result.token)
    setUser({ userId: result.userId, email: result.email, role: result.role })
  }

  const logout = () => {
    persistToken(null)
    setUser(null)
  }

  const value = useMemo(() => ({ user, login, register, logout }), [user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
