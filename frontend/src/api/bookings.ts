import { apiRequest } from './client'
import type { BookingDto, ResourceAvailability } from './types'

export function getAvailability(resourceId: string, date: string): Promise<ResourceAvailability> {
  return apiRequest<ResourceAvailability>(
    `/api/bookings/availability?resourceId=${resourceId}&date=${date}`,
  )
}

export function createBooking(resourceId: string, slotStart: string): Promise<string> {
  return apiRequest<string>('/api/bookings', {
    method: 'POST',
    body: { resourceId, slotStart },
  })
}

export function cancelBooking(id: string): Promise<void> {
  return apiRequest<void>(`/api/bookings/${id}`, { method: 'DELETE' })
}

export function getMyBookings(): Promise<BookingDto[]> {
  return apiRequest<BookingDto[]>('/api/bookings/mine')
}
