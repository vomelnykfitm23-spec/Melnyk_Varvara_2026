import { create } from 'zustand'

const stored = localStorage.getItem('user')

const useAuthStore = create((set) => ({
  token: localStorage.getItem('token') ?? null,
  user: stored ? JSON.parse(stored) : null,

  login(token, user) {
    localStorage.setItem('token', token)
    localStorage.setItem('user', JSON.stringify(user))
    set({ token, user })
  },

  logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    set({ token: null, user: null })
  },
}))

export default useAuthStore
