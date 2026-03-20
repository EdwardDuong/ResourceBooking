import { useEffect, useState } from 'react'
import { createBooking, getAvailability } from '../api/bookings'
import { describeError } from '../api/client'
import type { ResourceAvailability } from '../api/types'

interface AvailabilityGridProps {
  resourceId: string
  date: string
}

export function AvailabilityGrid({ resourceId, date }: AvailabilityGridProps) {
  const [availability, setAvailability] = useState<ResourceAvailability | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [bookingSlot, setBookingSlot] = useState<string | null>(null)
  const [confirmation, setConfirmation] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setError(null)
    setConfirmation(null)
    setAvailability(null)

    getAvailability(resourceId, date)
      .then((result) => {
        if (!cancelled) setAvailability(result)
      })
      .catch((err) => {
        if (!cancelled) setError(describeError(err))
      })

    return () => {
      cancelled = true
    }
  }, [resourceId, date])

  async function handleBook(slotStart: string) {
    setError(null)
    setConfirmation(null)
    setBookingSlot(slotStart)
    try {
      await createBooking(resourceId, slotStart)
      const refreshed = await getAvailability(resourceId, date)
      setAvailability(refreshed)
      setConfirmation(`Booked ${formatSlotTime(slotStart)}.`)
    } catch (err) {
      setError(describeError(err))
    } finally {
      setBookingSlot(null)
    }
  }

  if (error) {
    return <p className="form-error">{error}</p>
  }

  if (!availability) {
    return <p>Loading availability...</p>
  }

  return (
    <div>
      {confirmation && <p>{confirmation}</p>}
      <div className="slot-grid">
        {availability.slots.map((slot) => (
          <button
            key={slot.slotStart}
            type="button"
            disabled={!slot.isAvailable || bookingSlot === slot.slotStart}
            className={`slot-button${slot.isAvailable ? ' available' : ''}`}
            onClick={() => handleBook(slot.slotStart)}
          >
            {formatSlotTime(slot.slotStart)}
          </button>
        ))}
      </div>
    </div>
  )
}

function formatSlotTime(isoString: string): string {
  return new Date(isoString).toLocaleTimeString(undefined, {
    hour: '2-digit',
    minute: '2-digit',
  })
}
