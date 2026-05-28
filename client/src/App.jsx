import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Navbar from './components/Navbar'
import PrivateRoute from './components/PrivateRoute'
import LotCatalogPage from './pages/LotCatalogPage'
import LotDetailPage from './pages/LotDetailPage'
import CreateLotPage from './pages/CreateLotPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ProfilePage from './pages/ProfilePage'

export default function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <main className="main-content">
        <Routes>
          <Route path="/"             element={<LotCatalogPage />} />
          <Route path="/lots/:id"     element={<LotDetailPage />} />
          <Route path="/lots/create"  element={<PrivateRoute><CreateLotPage /></PrivateRoute>} />
          <Route path="/login"        element={<LoginPage />} />
          <Route path="/register"     element={<RegisterPage />} />
          <Route path="/profile"      element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
        </Routes>
      </main>
    </BrowserRouter>
  )
}
