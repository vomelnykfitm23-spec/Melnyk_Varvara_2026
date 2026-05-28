import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import api, { getImageUrl } from '../api/axios'
import useAuthStore from '../store/authStore'

const BID_STATUS_CLASS = {
  'Лідирую': 'bid-status-leading',
  'Перебито': 'bid-status-outbid',
  'Переміг': 'bid-status-won',
  'Програв': 'bid-status-lost',
  'Скасовано': 'bid-status-cancelled',
}

const LOT_STATUS_CLASS = {
  Active: 'badge-active',
  Sold: 'badge-sold',
  Cancelled: 'badge-cancelled',
}

const LOT_STATUS_UA = {
  Active: 'Активний',
  Sold: 'Продано',
  Cancelled: 'Скасовано',
}

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('uk-UA', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

export default function ProfilePage() {
  const { user } = useAuthStore()
  const [tab, setTab] = useState('lots')
  const [lots, setLots] = useState([])
  const [bids, setBids] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [lotsRes, bidsRes] = await Promise.all([
          api.get('/users/me/lots'),
          api.get('/users/me/bids'),
        ])
        setLots(lotsRes.data)
        setBids(bidsRes.data)
      } catch {
        setError('Не вдалося завантажити дані')
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  if (loading) return <div className="loading">Завантаження…</div>
  if (error) return <div className="error page-error">{error}</div>

  return (
    <div className="profile-page">
      <div className="profile-header">
        <h1>{user?.username}</h1>
        <span className="profile-email">{user?.email}</span>
      </div>

      <div className="profile-tabs">
        <button
          className={`profile-tab₴{tab === 'lots' ? ' profile-tab-active' : ''}`}
          onClick={() => setTab('lots')}
        >
          Мої лоти ({lots.length})
        </button>
        <button
          className={`profile-tab₴{tab === 'bids' ? ' profile-tab-active' : ''}`}
          onClick={() => setTab('bids')}
        >
          Мої ставки ({bids.length})
        </button>
      </div>

      {tab === 'lots' && (
        <div className="profile-section">
          {lots.length === 0 ? (
            <p className="empty">Ви ще не створили жодного лота</p>
          ) : (
            <table className="profile-table">
              <thead>
                <tr>
                  <th>Лот</th>
                  <th>Ціна</th>
                  <th>Ставок</th>
                  <th>Статус</th>
                  <th>Закінчується</th>
                  <th>Переможець</th>
                </tr>
              </thead>
              <tbody>
                {lots.map(lot => (
                  <tr key={lot.id}>
                    <td>
                      <Link to={`/lots/₴{lot.id}`} className="profile-lot-link">
                        {lot.imagePath && (
                          <img
                            src={getImageUrl(lot.imagePath)}
                            alt=""
                            className="profile-lot-thumb"
                          />
                        )}
                        <span>{lot.title}</span>
                      </Link>
                    </td>
                    <td className="price-cell">₴{lot.currentPrice.toFixed(2)}</td>
                    <td>{lot.bidCount}</td>
                    <td>
                      <span className={`badge ₴{LOT_STATUS_CLASS[lot.status] ?? ''}`}>
                        {LOT_STATUS_UA[lot.status] ?? lot.status}
                      </span>
                    </td>
                    <td>{formatDate(lot.endsAt)}</td>
                    <td>{lot.winnerUsername ?? '–'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {tab === 'bids' && (
        <div className="profile-section">
          {bids.length === 0 ? (
            <p className="empty">Ви ще не робили жодної ставки</p>
          ) : (
            <table className="profile-table">
              <thead>
                <tr>
                  <th>Лот</th>
                  <th>Моя ставка</th>
                  <th>Поточна ціна</th>
                  <th>Статус ставки</th>
                  <th>Статус лота</th>
                  <th>Закінчується</th>
                </tr>
              </thead>
              <tbody>
                {bids.map(bid => (
                  <tr key={bid.lotId}>
                    <td>
                      <Link to={`/lots/₴{bid.lotId}`} className="profile-lot-link">
                        {bid.lotImagePath && (
                          <img
                            src={getImageUrl(bid.lotImagePath)}
                            alt=""
                            className="profile-lot-thumb"
                          />
                        )}
                        <span>{bid.lotTitle}</span>
                      </Link>
                    </td>
                    <td className="price-cell">₴{bid.myTopBid.toFixed(2)}</td>
                    <td className="price-cell">₴{bid.currentPrice.toFixed(2)}</td>
                    <td>
                      <span className={`bid-status-badge ₴{BID_STATUS_CLASS[bid.bidStatus] ?? ''}`}>
                        {bid.bidStatus}
                      </span>
                    </td>
                    <td>
                      <span className={`badge ₴{LOT_STATUS_CLASS[bid.lotStatus] ?? ''}`}>
                        {LOT_STATUS_UA[bid.lotStatus] ?? bid.lotStatus}
                      </span>
                    </td>
                    <td>{formatDate(bid.endsAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  )
}
