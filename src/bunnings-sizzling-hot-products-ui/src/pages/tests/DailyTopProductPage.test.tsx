import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { api } from '../../lib/api'
import { DailyTopProductPage } from '../DailyTopProductPage'
import type { TopProductResponse } from '../../lib/api-client'

describe('DailyTopProductPage', () => {
  beforeEach(() => {
    vi.spyOn(api, 'daily').mockResolvedValue({
      from: '2026-04-21',
      to: '2026-04-21',
      productName: 'Ezy Storage 37L Flexi Laundry Basket - White',
    } as TopProductResponse)
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  test('renders the top product after the initial fetch', async () => {
    render(<DailyTopProductPage />)

    const result = await screen.findByTestId('daily-result')
    expect(result).toHaveTextContent('Ezy Storage 37L Flexi Laundry Basket - White')
  })

  test('refetches when the date changes', async () => {
    const user = userEvent.setup()
    const spy = vi.spyOn(api, 'daily').mockResolvedValue({
      from: '2026-04-22',
      to: '2026-04-22',
      productName: 'Another product',
    } as TopProductResponse)

    render(<DailyTopProductPage />)
    const dateInput = screen.getByLabelText(/The selected day/i)
    await user.clear(dateInput)
    await user.type(dateInput, '2026-04-22')

    expect(spy).toHaveBeenCalledWith('2026-04-22')
  })

  test('displays the API error message', async () => {
    vi.spyOn(api, 'daily').mockRejectedValueOnce(new Error('Boom'))

    render(<DailyTopProductPage />)

    expect(await screen.findByRole('alert')).toHaveTextContent('Boom')
  })
})