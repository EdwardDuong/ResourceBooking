import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()

  return (
    <div className="app-shell">
      <nav className="app-nav">
        <Link to="/" className="brand">
          Resource Booking
        </Link>
        <div className="app-nav-links">
          {user ? (
            <>
              <Link to="/">Resources</Link>
              <Link to="/my-bookings">My Bookings</Link>
              {user.role === 'Admin' && <Link to="/admin/resources">Admin</Link>}
              <span className="app-nav-user">{user.email}</span>
              <button type="button" onClick={logout}>
                Log out
              </button>
            </>
          ) : (
            <>
              <Link to="/login">Log in</Link>
              <Link to="/register">Register</Link>
            </>
          )}
        </div>
      </nav>
      <main>{children}</main>
    </div>
  )
}
