export type UserRole = 'Member' | 'Admin'

export interface AuthResult {
  token: string
  userId: string
  email: string
  role: UserRole
}

export interface ResourceDto {
  id: string
  name: string
  description: string | null
  isActive: boolean
  createdAtUtc: string
}

export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed'

export interface BookingDto {
  id: string
  resourceId: string
  slotStart: string
  status: BookingStatus
}

export interface AvailabilitySlot {
  slotStart: string
  isAvailable: boolean
}

export interface ResourceAvailability {
  resourceId: string
  date: string
  slots: AvailabilitySlot[]
}

export interface ProblemDetails {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}
