import { useRef, useState } from 'react'
import { casesApi } from '../api/cases'
import { ApiError } from '../api/client'

export function CaseExportImport({ caseId }: { caseId: number }) {
  const [status, setStatus] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  async function handleExport() {
    setStatus(null)
    try {
      const bundle = await casesApi.exportHistory(caseId)
      const blob = new Blob([JSON.stringify(bundle, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `sak-${caseId}-eksport.json`
      link.click()
      URL.revokeObjectURL(url)
    } catch (err) {
      setStatus(err instanceof ApiError ? err.message : 'Kunne ikke eksportere sak.')
    }
  }

  async function handleImportFile(file: File) {
    setStatus(null)
    try {
      const text = await file.text()
      const res = await casesApi.importHistory(caseId, text)
      setStatus(res.message)
    } catch (err) {
      setStatus(err instanceof ApiError ? err.message : 'Kunne ikke importere sak.')
    }
  }

  return (
    <section className="case-export-import">
      <h2>Eksporter / importer historikk</h2>
      <div className="case-export-import-actions">
        <button onClick={handleExport}>Eksporter sak</button>
        <button onClick={() => fileInputRef.current?.click()}>Importer sak</button>
        <input
          ref={fileInputRef}
          type="file"
          accept="application/json"
          style={{ display: 'none' }}
          onChange={(e) => {
            const file = e.target.files?.[0]
            if (file) {
              handleImportFile(file)
            }
            e.target.value = ''
          }}
        />
      </div>
      {status && <p className="form-message">{status}</p>}
    </section>
  )
}
