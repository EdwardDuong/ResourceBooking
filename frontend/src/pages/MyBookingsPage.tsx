import { useEffect, useState } from 'react'
import { cancelBooking, getMyBookings } from '../api/bookings'
import { describeError } from '../api/client'
import type { BookingDto } from '../api/types'

export function MyBookingsPage() {
  const [bookings, setBookings] = useState<BookingDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [cancellingId, setCancellingId] = useState<string | null>(null)

  useEffect(() => {
    load()
  }, [])

  function load() {
    setError(null)
    getMyBookings()
      .then(setBookings)
      .catch((err) => setError(describeError(err)))
  }

  async function handleCancel(id: string) {
    setError(null)
    setCancellingId(id)
    try {
      await cancelBooking(id)
      load()
    } catch (err) {
      setError(describeError(err))
    } finally {
      setCancellingId(null)
    }
  }

  return (
    <div>
      <h1>My Bookings</h1>
      {error && <p className="form-error">{error}</p>}
      {bookings.length === 0 && !error && <p>No bookings yet.</p>}

      {bookings.map((booking) => (
        <div key={booking.id} className="card card-row">
          <div>
            <strong>{formatSlot(booking.slotStart)}</strong>
            <p>Status: {booking.status}</p>
          </div>
          {(booking.status === 'Pending' || booking.status === 'Confirmed') && (
            <button
              type="button"
              disabled={cancellingId === booking.id}
              onClick={() => handleCancel(booking.id)}
            >
              Cancel
            </button>
          )}
        </div>
      ))}
    </div>
  )
}

function formatSlot(isoString: string): string {
  return new Date(isoString).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}
