import { useEffect, useState } from "react"
import { api, formatApiError, formatDateOnly } from "../lib/api"
import type { TopProductResponse } from "../lib/api-client"

export function RollingTopProductPage() {
  const [days, setDays] = useState(3)
  const [data, setData] = useState<TopProductResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)
    api.rolling(days)
      .then(d => { if (!cancelled) setData(d) })
      .catch(e => { if (!cancelled) setError(formatApiError(e)) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [days])

  return (
    <section className="page" aria-labelledby="rolling-heading">
      <p className="page__meta">Section 02 · Rolling</p>
      <h1 id="rolling-heading" className="page__title">
        Top sizzling hot product over the past N days.
      </h1>

      <div className="form">
        <label className="field">
          <span className="field__label">Window length (days)</span>
          <input
            type="number"
            min={1}
            max={365}
            value={days}
            onChange={e => setDays(Number(e.target.value))}
          />
        </label>
      </div>

      {loading && (
        <p className="status status--loading">
          Aggregating the window…
        </p>
      )}
      {error && <p role="alert" className="error">{error}</p>}
      {data && !loading && !error && (
        <article className="result" data-testid="rolling-result">
          <p className="result__meta">
            {formatDateOnly(data.from)} → {formatDateOnly(data.to)}
          </p>
          <h2 className="result__name">
            {data.productName ?? 'No sales recorded'}
          </h2>
          <p className="result__cap">Top performer for the window.</p>
        </article>
      )}
    </section>
  )
}
