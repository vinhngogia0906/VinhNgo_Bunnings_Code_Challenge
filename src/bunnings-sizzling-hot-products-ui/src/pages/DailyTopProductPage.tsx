import { useEffect, useState } from "react"
import { api, formatApiError, formatDateOnly } from "../lib/api"
import type { TopProductResponse } from "../lib/api-client"

const DEFAULT_DATE = '2026-04-23'

export function DailyTopProductPage() {
  const [date, setDate] = useState(DEFAULT_DATE)
  const [data, setData] = useState<TopProductResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)
    api.daily(date)
      .then(d => { if (!cancelled) setData(d) })
      .catch(e => { if (!cancelled) setError(formatApiError(e)) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [date])

  return (
    <section className="page" aria-labelledby="daily-heading">
      <p className="page__meta">Section 01 · Daily</p>
      <h1 id="daily-heading" className="page__title">
        Top sizzling hot product for a single day.
      </h1>

      <div className="form">
        <label className="field">
          <span className="field__label">The selected day</span>
          <input
            type="date"
            value={date}
            max={DEFAULT_DATE}
            onChange={e => setDate(e.target.value)}
            aria-label="Select a date"
          />
        </label>
      </div>

      {loading && (
        <p className="status status--loading">
          Looking up the day's top product…
        </p>
      )}
      {error && <p role="alert" className="error">{error}</p>}
      {data && !loading && !error && (
        <article className="result" data-testid="daily-result">
          <p className="result__meta">On {formatDateOnly(data.from)}</p>
          <h2 className="result__name">
            {data.productName ?? 'No sales recorded'}
          </h2>
          <p className="result__cap">The day's top performer.</p>
        </article>
      )}
    </section>
  )
}
