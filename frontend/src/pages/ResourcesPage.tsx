import { useEffect, useState } from 'react'
import { getActiveResources } from '../api/resources'
import { describeError } from '../api/client'
import { AvailabilityGrid } from '../components/AvailabilityGrid'
import type { ResourceDto } from '../api/types'

function todayIsoDate(): string {
  return new Date().toISOString().slice(0, 10)
}

export function ResourcesPage() {
  const [resources, setResources] = useState<ResourceDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [selectedResourceId, setSelectedResourceId] = useState<string | null>(null)
  const [date, setDate] = useState(todayIsoDate())

  useEffect(() => {
    getActiveResources()
      .then(setResources)
      .catch((err) => setError(describeError(err)))
  }, [])

  return (
    <div>
      <h1>Resources</h1>
      {error && <p className="form-error">{error}</p>}
      {resources.length === 0 && !error && <p>No active resources yet.</p>}

      {resources.map((resource) => (
        <div key={resource.id} className="card">
          <div className="card-row">
            <div>
              <strong>{resource.name}</strong>
              {resource.description && <p>{resource.description}</p>}
            </div>
            <button
              type="button"
              onClick={() =>
                setSelectedResourceId(resource.id === selectedResourceId ? null : resource.id)
              }
            >
              {resource.id === selectedResourceId ? 'Close' : 'Book'}
            </button>
          </div>

          {resource.id === selectedResourceId && (
            <div>
              <label>
                Date
                <input
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                  min={todayIsoDate()}
                />
              </label>
              <AvailabilityGrid resourceId={resource.id} date={date} />
            </div>
          )}
        </div>
      ))}
    </div>
  )
}
