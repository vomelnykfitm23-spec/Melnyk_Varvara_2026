import { useState, useEffect, useCallback } from 'react'
import api from '../api/axios'
import LotCard from '../components/LotCard'

const PAGE_SIZE = 12

export default function LotCatalogPage() {
  const [lots, setLots] = useState([])
  const [tags, setTags] = useState([])
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [tagId, setTagId] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get('/tags').then(r => setTags(r.data)).catch(() => {})
  }, [])

  const fetchLots = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const params = { page, pageSize: PAGE_SIZE }
      if (search) params.search = search
      if (tagId)  params.tagId  = tagId
      const { data } = await api.get('/lots', { params })
      setLots(data.items)
      setTotalPages(data.totalPages)
    } catch {
      setError('Failed to load lots')
    } finally {
      setLoading(false)
    }
  }, [page, search, tagId])

  useEffect(() => { fetchLots() }, [fetchLots])

  // Debounce search input
  useEffect(() => {
    const id = setTimeout(() => {
      setPage(1)
      setSearch(searchInput)
    }, 400)
    return () => clearTimeout(id)
  }, [searchInput])

  const handleTagChange = (val) => {
    setTagId(val)
    setPage(1)
  }

  return (
    <div className="catalog-page">
      <div className="catalog-filters">
        <input
          className="search-input"
          type="search"
          placeholder="Search lots…"
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
        />
        <select
          className="tag-select"
          value={tagId}
          onChange={e => handleTagChange(e.target.value)}
        >
          <option value="">All tags</option>
          {tags.map(t => (
            <option key={t.id} value={t.id}>{t.name}</option>
          ))}
        </select>
      </div>

      {loading && <div className="loading">Loading…</div>}
      {error   && <div className="error">{error}</div>}

      {!loading && !error && lots.length === 0 && (
        <div className="empty">No lots found</div>
      )}

      <div className="lot-grid">
        {lots.map(lot => <LotCard key={lot.id} lot={lot} />)}
      </div>

      {totalPages > 1 && (
        <div className="pagination">
          <button
            className="btn btn-ghost"
            disabled={page === 1}
            onClick={() => setPage(p => p - 1)}
          >
            ← Prev
          </button>
          <span>{page} / {totalPages}</span>
          <button
            className="btn btn-ghost"
            disabled={page === totalPages}
            onClick={() => setPage(p => p + 1)}
          >
            Next →
          </button>
        </div>
      )}
    </div>
  )
}
