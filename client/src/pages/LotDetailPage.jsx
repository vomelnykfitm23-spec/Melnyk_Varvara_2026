import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api, { getImageUrl } from '../api/axios'
import useAuthStore from '../store/authStore'
import { useCountdown } from '../hooks/useCountdown'

function CountdownBadge({ endsAt, status }) {
  const time = useCountdown(endsAt)

  if (status !== 'Active') return <span className={`badge badge-${status.toLowerCase()}`}>{status}</span>
  if (!time) return <span className="badge badge-sold">Auction ended</span>

  const parts = []
  if (time.d) parts.push(`${time.d}d`)
  parts.push(`${String(time.h).padStart(2, '0')}:${String(time.m).padStart(2, '0')}:${String(time.s).padStart(2, '0')}`)

  return (
    <span className={`countdown ${time.d === 0 && time.h === 0 ? 'time-urgent' : ''}`}>
      ⏱ {parts.join(' ')}
    </span>
  )
}

function Lightbox({ src, alt, onClose }) {
  useEffect(() => {
    const handler = (e) => { if (e.key === 'Escape') onClose() }
    document.body.style.overflow = 'hidden'
    window.addEventListener('keydown', handler)
    return () => {
      window.removeEventListener('keydown', handler)
      document.body.style.overflow = ''
    }
  }, [onClose])

  return (
    <div className="lightbox" onClick={onClose}>
      <button className="lightbox-close" onClick={onClose}>✕</button>
      <img src={src} alt={alt} onClick={(e) => e.stopPropagation()} />
    </div>
  )
}

export default function LotDetailPage() {
  const { id } = useParams()
  const { user } = useAuthStore()
  const navigate = useNavigate()

  const [lot, setLot]           = useState(null)
  const [bids, setBids]         = useState([])
  const [amount, setAmount]     = useState('')
  const [bidError, setBidError] = useState('')
  const [bidLoading, setBidLoading] = useState(false)
  const [loading, setLoading]   = useState(true)
  const [error, setError]       = useState('')
  const [lightbox, setLightbox] = useState(false)

  const fetchLot = async () => {
    try {
      const [lotRes, bidsRes] = await Promise.all([
        api.get(`/lots/${id}`),
        api.get(`/lots/${id}/bids`),
      ])
      setLot(lotRes.data)
      setBids(bidsRes.data)
    } catch {
      setError('Lot not found')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchLot() }, [id])

  const handleBid = async (e) => {
    e.preventDefault()
    setBidError('')
    const val = parseFloat(amount)
    if (isNaN(val) || val <= lot.currentPrice) {
      setBidError(`Сума має бути більше ₴${lot.currentPrice.toFixed(2)}`)
      return
    }
    setBidLoading(true)
    try {
      await api.post('/bids', { lotId: lot.id, amount: val })
      setAmount('')
      await fetchLot()
    } catch (err) {
      setBidError(err.response?.data?.message ?? 'Failed to place bid')
    } finally {
      setBidLoading(false)
    }
  }

  if (loading) return <div className="loading">Loading…</div>
  if (error)   return <div className="error page-error">{error}</div>
  if (!lot)    return null

  const isSeller  = user?.id === lot.seller.id
  const isActive  = lot.status === 'Active'
  const canBid    = user && !isSeller && isActive
  const isExpired = isActive && new Date(lot.endsAt) < new Date()

  const winnerBid = lot.winnerBidId ? bids.find(b => b.id === lot.winnerBidId) : null
  const isWinner  = user && winnerBid && user.id === winnerBid.bidder?.id

  return (
    <div className="detail-page">
      {lightbox && lot.imagePath && (
        <Lightbox
          src={getImageUrl(lot.imagePath)}
          alt={lot.title}
          onClose={() => setLightbox(false)}
        />
      )}

      <div className="detail-main">
        <div
          className={`detail-image${lot.imagePath ? ' detail-image-clickable' : ''}`}
          onClick={() => lot.imagePath && setLightbox(true)}
        >
          {lot.imagePath
            ? <img src={getImageUrl(lot.imagePath)} alt={lot.title} />
            : <div className="detail-no-image">Фото відсутнє</div>
          }
        </div>

        <div className="detail-info">
          <div className="detail-tags">
            {lot.tags?.map(t => <span key={t.id} className="tag">{t.name}</span>)}
          </div>
          <h1>{lot.title}</h1>
          <p className="detail-seller">Продавець: <strong>{lot.seller.username}</strong></p>
          {lot.description && <p className="detail-desc">{lot.description}</p>}

          <div className="detail-price-row">
            <div>
              <div className="label">Поточна ціна</div>
              <div className="detail-price">₴{lot.currentPrice.toFixed(2)}</div>
            </div>
            <div>
              <div className="label">Закінчується</div>
              <CountdownBadge endsAt={lot.endsAt} status={lot.status} />
            </div>
          </div>

          {lot.status === 'Sold' && winnerBid && (
            <div className="winner-banner">
              <div>Аукціон завершено – переможець: <strong>{winnerBid.bidder?.username}</strong></div>
              {isWinner && lot.sellerEmail && (
                <div className="winner-contact">
                  Зв'яжіться з продавцем:{' '}
                  <a href={`mailto:${lot.sellerEmail}`}>{lot.sellerEmail}</a>
                </div>
              )}
            </div>
          )}

          {isExpired && lot.status === 'Active' && (
            <div className="winner-banner">Аукціон завершено</div>
          )}

          {canBid && !isExpired && (
            <form className="bid-form" onSubmit={handleBid}>
              <h3>Зробити ставку</h3>
              {bidError && <div className="form-error">{bidError}</div>}
              <div className="bid-row">
                <input
                  type="number"
                  step="0.01"
                  min={lot.currentPrice + 0.01}
                  placeholder={`> ₴${lot.currentPrice.toFixed(2)}`}
                  value={amount}
                  onChange={e => setAmount(e.target.value)}
                  required
                />
                <button className="btn btn-primary" disabled={bidLoading}>
                  {bidLoading ? 'Відправка…' : 'Ставка'}
                </button>
              </div>
            </form>
          )}

          {!user && isActive && (
            <p className="auth-hint">
              <a href="/login">Увійдіть</a>, щоб зробити ставку
            </p>
          )}
        </div>
      </div>

      <div className="bid-history">
        <h2>Історія ставок ({bids.length})</h2>
        {bids.length === 0
          ? <p className="empty">Ставок ще немає</p>
          : (
            <table className="bids-table">
              <thead>
                <tr><th>Учасник</th><th>Сума</th><th>Час</th></tr>
              </thead>
              <tbody>
                {[...bids].reverse().map(b => (
                  <tr key={b.id} className={b.id === lot.winnerBidId ? 'winner-row' : ''}>
                    <td>{b.bidder?.username}</td>
                    <td>₴{b.amount.toFixed(2)}</td>
                    <td>{new Date(b.placedAt).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )
        }
      </div>
    </div>
  )
}
