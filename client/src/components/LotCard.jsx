import { Link } from 'react-router-dom'
import { getImageUrl } from '../api/axios'
import { useCountdown } from '../hooks/useCountdown'

function TimeLeft({ endsAt, status }) {
  const time = useCountdown(endsAt)

  if (status !== 'Active') return <span className={`badge badge-${status.toLowerCase()}`}>{status}</span>
  if (!time) return <span className="badge badge-sold">Ended</span>

  if (time.d > 0) return <span className="time-left">{time.d}d {time.h}h left</span>
  if (time.h > 0) return <span className="time-left time-soon">{time.h}h {time.m}m left</span>
  return <span className="time-left time-urgent">{time.m}m {time.s}s left</span>
}

export default function LotCard({ lot }) {
  return (
    <Link to={`/lots/${lot.id}`} className="lot-card">
      <div className="lot-card-image">
        {lot.imagePath
          ? <img src={getImageUrl(lot.imagePath)} alt={lot.title} />
          : <div className="lot-card-no-image">Фото відсутнє</div>
        }
      </div>
      <div className="lot-card-body">
        <h3 className="lot-card-title">{lot.title}</h3>
        <div className="lot-card-tags">
          {lot.tags?.slice(0, 2).map(t => (
            <span key={t.id} className="tag">{t.name}</span>
          ))}
        </div>
        <div className="lot-card-footer">
          <span className="lot-price">₴{lot.currentPrice.toFixed(2)}</span>
          <TimeLeft endsAt={lot.endsAt} status={lot.status} />
        </div>
      </div>
    </Link>
  )
}
