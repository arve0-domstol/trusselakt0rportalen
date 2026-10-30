import { useEffect, useState, type FormEvent } from 'react'
import { documentsApi } from '../api/documents'
import { ApiError } from '../api/client'
import type { CaseDocument } from '../types/document'

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} kB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export function CaseDocuments({ caseId }: { caseId: number }) {
  const [documents, setDocuments] = useState<CaseDocument[]>([])
  const [importUrl, setImportUrl] = useState('')
  const [importing, setImporting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  function reload() {
    documentsApi.list(caseId).then(setDocuments)
  }

  useEffect(reload, [caseId])

  async function handleImport(e: FormEvent) {
    e.preventDefault()
    if (!importUrl.trim()) return
    setError(null)
    setImporting(true)
    try {
      await documentsApi.importFromUrl(caseId, importUrl)
      setImportUrl('')
      reload()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Import feilet.')
    } finally {
      setImporting(false)
    }
  }

  return (
    <section className="case-documents">
      <h2>Dokumenter</h2>
      <ul className="document-list">
        {documents.map((d) => (
          <li key={d.id} className="document-item">
            <span className="document-name">{d.fileName}</span>
            <span className="document-meta">
              {formatSize(d.sizeBytes)} · {d.uploadedByName} ·{' '}
              {new Date(d.uploadedAt).toLocaleDateString('nb-NO')}
            </span>
            <span className="document-actions">
              <a href={documentsApi.previewUrl(d.id)} target="_blank" rel="noreferrer">
                Forhåndsvis
              </a>
              <a href={documentsApi.downloadUrl(d.id)}>Last ned</a>
            </span>
          </li>
        ))}
        {documents.length === 0 && <li className="document-empty">Ingen dokumenter lastet opp.</li>}
      </ul>
      <form onSubmit={handleImport} className="document-import-form">
        <input
          type="text"
          placeholder="Importer dokument fra URL…"
          value={importUrl}
          onChange={(e) => setImportUrl(e.target.value)}
          disabled={importing}
        />
        <button type="submit" disabled={importing || !importUrl.trim()}>
          {importing ? 'Importerer…' : 'Importer'}
        </button>
      </form>
      {error && <p className="form-error">{error}</p>}
    </section>
  )
}
