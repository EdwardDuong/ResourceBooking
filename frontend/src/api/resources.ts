import { apiRequest } from './client'
import type { ResourceDto } from './types'

export function getActiveResources(): Promise<ResourceDto[]> {
  return apiRequest<ResourceDto[]>('/api/resources')
}

export function createResource(name: string, description: string | null): Promise<string> {
  return apiRequest<string>('/api/resources', {
    method: 'POST',
    body: { name, description },
  })
}

export function deactivateResource(id: string): Promise<void> {
  return apiRequest<void>(`/api/resources/${id}`, { method: 'DELETE' })
}
