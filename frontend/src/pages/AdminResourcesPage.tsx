import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { createResource, deactivateResource, getActiveResources } from '../api/resources'
import { describeError } from '../api/client'
import type { ResourceDto } from '../api/types'

export function AdminResourcesPage() {
  const [resources, setResources] = useState<ResourceDto[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [deactivatingId, setDeactivatingId] = useState<string | null>(null)

  useEffect(() => {
    load()
  }, [])

  function load() {
    setError(null)
    getActiveResources()
      .then(setResources)
      .catch((err) => setError(describeError(err)))
  }

  async function handleCreate(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await createResource(name, description || null)
      setName('')
      setDescription('')
      load()
    } catch (err) {
      setError(describeError(err))
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDeactivate(id: string) {
    setError(null)
    setDeactivatingId(id)
    try {
      await deactivateResource(id)
      load()
    } catch (err) {
      setError(describeError(err))
    } finally {
      setDeactivatingId(null)
    }
  }

  return (
    <div>
      <h1>Admin: Resources</h1>
      {error && <p className="form-error">{error}</p>}

      <form onSubmit={handleCreate} className="admin-create-form">
        <label>
          Name
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </label>
        <label>
          Description
          <input value={description} onChange={(e) => setDescription(e.target.value)} />
        </label>
        <button type="submit" disabled={submitting}>
          {submitting ? 'Creating...' : 'Create resource'}
        </button>
      </form>

      {resources.map((resource) => (
        <div key={resource.id} className="card card-row">
          <div>
            <strong>{resource.name}</strong>
            {resource.description && <p>{resource.description}</p>}
          </div>
          <button
            type="button"
            disabled={deactivatingId === resource.id}
            onClick={() => handleDeactivate(resource.id)}
          >
            Deactivate
          </button>
        </div>
      ))}
    </div>
  )
}
