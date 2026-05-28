import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import api, { getImageUrl } from '../api/axios'

export default function CreateLotPage() {
  const navigate = useNavigate()
  const fileRef  = useRef(null)

  const [tags, setTags]         = useState([])
  const [preview, setPreview]   = useState(null)   // local blob URL for preview
  const [imageFile, setImageFile] = useState(null) // File object
  const [form, setForm] = useState({
    title: '',
    description: '',
    startingPrice: '',
    tagIds: [],
    endsAt: '',
  })
  const [error, setError]     = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    api.get('/tags').then(r => setTags(r.data)).catch(() => {})
  }, [])

  const handleFileChange = (e) => {
    const file = e.target.files?.[0]
    if (!file) return
    setImageFile(file)
    setPreview(URL.createObjectURL(file))
  }

  const toggleTag = (id) => {
    setForm(f => ({
      ...f,
      tagIds: f.tagIds.includes(id)
        ? f.tagIds.filter(t => t !== id)
        : [...f.tagIds, id],
    }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')

    if (!form.title.trim())                          { setError('Введіть назву'); return }
    if (!form.startingPrice || +form.startingPrice <= 0) { setError('Стартова ціна має бути більше 0'); return }
    if (!form.endsAt)                                { setError('Вкажіть дату закінчення'); return }
    if (new Date(form.endsAt) <= new Date())         { setError('Дата закінчення має бути в майбутньому'); return }

    setLoading(true)
    try {
      // 1. Upload image if selected
      let imagePath = null
      if (imageFile) {
        const fd = new FormData()
        fd.append('file', imageFile)
        const { data } = await api.post('/uploads/image', fd, {
          headers: { 'Content-Type': 'multipart/form-data' },
        })
        imagePath = data.path
      }

      // 2. Create lot
      const { data } = await api.post('/lots', {
        title:         form.title.trim(),
        description:   form.description.trim() || null,
        startingPrice: parseFloat(form.startingPrice),
        tagIds:        form.tagIds,
        endsAt:        new Date(form.endsAt).toISOString(),
        imagePath,
      })

      navigate(`/lots/${data.id}`)
    } catch (err) {
      setError(err.response?.data?.message ?? 'Помилка при створенні лота')
    } finally {
      setLoading(false)
    }
  }

  const minDate = new Date(Date.now() + 3600000).toISOString().slice(0, 16)

  return (
    <div className="create-page">
      <h1>Новий лот</h1>
      <form className="create-form" onSubmit={handleSubmit}>
        {error && <div className="form-error">{error}</div>}

        <label>
          Назва *
          <input
            type="text"
            value={form.title}
            onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
            maxLength={255}
            required
            autoFocus
          />
        </label>

        <label>
          Опис
          <textarea
            rows={4}
            value={form.description}
            onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
          />
        </label>

        <label>
          Стартова ціна (₴) *
          <input
            type="number"
            step="0.01"
            min="0.01"
            value={form.startingPrice}
            onChange={e => setForm(f => ({ ...f, startingPrice: e.target.value }))}
            required
          />
        </label>

        <label>
          Закінчення аукціону *
          <input
            type="datetime-local"
            min={minDate}
            value={form.endsAt}
            onChange={e => setForm(f => ({ ...f, endsAt: e.target.value }))}
            required
          />
        </label>

        <div className="image-upload">
          <span className="label">Фото</span>
          {preview && (
            <div className="image-preview">
              <img src={preview} alt="Попередній перегляд" />
              <button
                type="button"
                className="image-remove"
                onClick={() => { setPreview(null); setImageFile(null); fileRef.current.value = '' }}
              >
                ✕
              </button>
            </div>
          )}
          <button
            type="button"
            className="btn btn-outline"
            onClick={() => fileRef.current.click()}
          >
            {preview ? 'Змінити фото' : 'Завантажити фото'}
          </button>
          <input
            ref={fileRef}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            onChange={handleFileChange}
            style={{ display: 'none' }}
          />
          <span className="upload-hint">JPG, PNG або WebP, до 5 МБ</span>
        </div>

        {tags.length > 0 && (
          <div className="tags-picker">
            <span className="label">Теги</span>
            <div className="tags-list">
              {tags.map(t => (
                <button
                  type="button"
                  key={t.id}
                  className={`tag tag-btn ${form.tagIds.includes(t.id) ? 'tag-selected' : ''}`}
                  onClick={() => toggleTag(t.id)}
                >
                  {t.name}
                </button>
              ))}
            </div>
          </div>
        )}

        <div className="form-actions">
          <button type="button" className="btn btn-ghost" onClick={() => navigate(-1)}>
            Скасувати
          </button>
          <button className="btn btn-primary" disabled={loading}>
            {loading ? 'Збереження…' : 'Створити лот'}
          </button>
        </div>
      </form>
    </div>
  )
}
