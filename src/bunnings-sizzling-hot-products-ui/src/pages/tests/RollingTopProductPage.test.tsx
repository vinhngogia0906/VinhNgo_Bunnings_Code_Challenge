import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { api } from "../../lib/api"
import type { TopProductResponse } from "../../lib/api-client"
import { RollingTopProductPage } from "../RollingTopProductPage"
import { fireEvent, render, screen, waitFor } from "@testing-library/react"

describe('RollingTopProductPage', () => {
  beforeEach(() => {
    vi.spyOn(api, 'rolling').mockResolvedValue({
      from: '2026-04-21',
      to: '2026-04-21',
      productName: 'Ezy Storage 37L Flexi Laundry Basket - White',
    } as TopProductResponse)
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  test('renders the top product after the initial fetch', async () => {
    render(<RollingTopProductPage />)

    const result = await screen.findByTestId('rolling-result')
    expect(result).toHaveTextContent('Ezy Storage 37L Flexi Laundry Basket - White')
  })

  test('refetches when the days input changes', async () => {
    const spy = vi.spyOn(api, 'rolling').mockResolvedValue({
      from: '2026-04-22',
      to: '2026-04-22',
      productName: 'Another product',
    } as TopProductResponse)

    render(<RollingTopProductPage />)
    await waitFor(() => expect(spy).toHaveBeenCalledWith(3))
    const daysInput = screen.getByLabelText(/Window length/i)
     fireEvent.change(daysInput, { target: { value: '7' } })

    // After the change, the effect refires with the new value.
    await waitFor(() => expect(spy).toHaveBeenCalledWith(7))
  })

  test('displays the API error message', async () => {
    vi.spyOn(api, 'rolling').mockRejectedValueOnce(new Error('Boom'))

    render(<RollingTopProductPage />)

    expect(await screen.findByRole('alert')).toHaveTextContent('Boom')
  })

  test('shows a validation error for days below 1', async () => {
    // Initial mount with the default `days=3` should succeed (via beforeEach).
    // After the user enters 0, the API should reject with a ProblemDetails 400.
    vi.spyOn(api, 'rolling')
      .mockResolvedValueOnce({
        from: '2026-04-21',
        to: '2026-04-21',
        productName: 'Ezy Storage 37L Flexi Laundry Basket - White',
      } as TopProductResponse)
      .mockRejectedValueOnce({
        status: 400,
        title: 'One or more validation errors occurred.',
        errors: { Days: ['Days must be positive.'] },
      })

    render(<RollingTopProductPage />)

    // Wait for the initial fetch to settle before changing the input.
    await screen.findByTestId('rolling-result')

    const daysInput = screen.getByLabelText(/Window length/i)
    fireEvent.change(daysInput, { target: { value: '0' } })

    expect(await screen.findByRole('alert')).toHaveTextContent('Days must be positive.')
  })

  test('shows a validation error for days above 365', async () => {
    vi.spyOn(api, 'rolling')
      .mockResolvedValueOnce({
        from: '2026-04-21',
        to: '2026-04-21',
        productName: 'Ezy Storage 37L Flexi Laundry Basket - White',
      } as TopProductResponse)
      .mockRejectedValueOnce({
        status: 400,
        title: 'One or more validation errors occurred.',
        errors: { Days: ['Days cannot exceed 365.'] },
      })

    render(<RollingTopProductPage />)

    await screen.findByTestId('rolling-result')

    const daysInput = screen.getByLabelText(/Window length/i)
    fireEvent.change(daysInput, { target: { value: '366' } })

    expect(await screen.findByRole('alert')).toHaveTextContent('Days cannot exceed 365.')
  })
})