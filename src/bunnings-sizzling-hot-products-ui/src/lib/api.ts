import { Client, ApiException } from './api-client'

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? ''
export const api = new Client(baseUrl)


// Format a server-side DateOnly in UTC, so the day doesn't drift in
// negative timezones (NSwag treats DateOnly as a Date with UTC-midnight).
export function formatDateOnly(yyyyMmDd: string, locale = 'en-AU'): string {
    return new Intl.DateTimeFormat(locale, {
      day: 'numeric', month: 'short', year: 'numeric', timeZone: 'UTC',
    }).format(new Date(`${yyyyMmDd}T00:00:00Z`))
}

// NSwag throws a ProblemDetails instance directly on 400 (no .message)
// and an ApiException on other non-200 statuses. Both shapes handled here.
export function formatApiError(e: unknown): string {
    if (e instanceof ApiException) return e.message || `HTTP ${e.status}`

    if (e && typeof e === 'object') {
      const p = e as Record<string, unknown>

      if (p.errors && typeof p.errors === 'object') {
        const msgs = Object.values(p.errors as Record<string, unknown>)
          .flat()
          .filter((m): m is string => typeof m === 'string')
        if (msgs.length) return msgs.join('; ')
      }

      if (typeof p.detail === 'string') return p.detail
      if (typeof p.title === 'string') return p.title
    }

    return e instanceof Error ? e.message : String(e)
}