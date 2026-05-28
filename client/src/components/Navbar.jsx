import { Link, useNavigate } from 'react-router-dom'
import useAuthStore from '../store/authStore'

export default function Navbar() {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-logo">AuctionApp</Link>
      <div className="navbar-links">
        {user ? (
          <>
            <Link to="/lots/create" className="btn btn-outline">+ Новий лот</Link>
            <Link to="/profile" className="btn btn-ghost">Профіль</Link>
            <span className="navbar-username">{user.username}</span>
            <button onClick={handleLogout} className="btn btn-ghost">Вийти</button>
          </>
        ) : (
          <>
            <Link to="/login" className="btn btn-ghost">Login</Link>
            <Link to="/register" className="btn btn-primary">Register</Link>
          </>
        )}
      </div>
    </nav>
  )
}
