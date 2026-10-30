import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, ApiError } from '../api/client'
import type { UserProfile } from './types'

// Cached locally so the UI has something to show immediately on reload,
// without waiting for a round trip to /api/me. The real session lives in
// the httpOnly cookie the API sets on login - this cache is just the
// profile the frontend already knows and doesn't want to keep re-fetching.
const PROFILE_CACHE_KEY = 'sb_profile_cache'

interface AuthContextValue {
  user: UserProfile | null
  loading: boolean
  login: (username: string, password: string, rememberMe: boolean) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function readCachedProfile(): UserProfile | null {
  try {
    const raw = localStorage.getItem(PROFILE_CACHE_KEY)
    return raw ? (JSON.parse(raw) as UserProfile) : null
  } catch {
    return null
  }
}

function writeCachedProfile(profile: UserProfile | null) {
  try {
    if (profile) {
      localStorage.setItem(PROFILE_CACHE_KEY, JSON.stringify(profile))
    } else {
      localStorage.removeItem(PROFILE_CACHE_KEY)
    }
  } catch {
    // localStorage may be unavailable (private browsing, storage full,
    // etc.) - the app still works, it just re-fetches the profile each load.
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(() => readCachedProfile())
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api
      .get<UserProfile>('/api/me')
      .then((profile) => {
        setUser(profile)
        writeCachedProfile(profile)
      })
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          setUser(null)
          writeCachedProfile(null)
        }
      })
      .finally(() => setLoading(false))
  }, [])

  async function login(username: string, password: string, rememberMe: boolean) {
    const profile = await api.post<UserProfile>('/api/auth/login', {
      username,
      password,
      rememberMe,
    })
    setUser(profile)
    writeCachedProfile(profile)
  }

  async function logout() {
    await api.post('/api/auth/logout')
    setUser(null)
    writeCachedProfile(null)
  }

  return <AuthContext.Provider value={{ user, loading, login, logout }}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return ctx
}
