import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { casesApi, type CaseListParams } from '../api/cases'
import { CASE_PRIORITY_OPTIONS, CASE_STATUS_OPTIONS, type CaseSummary, type PagedResult } from '../types/case'

const PAGE_SIZE = 15

export function CaseListPage() {
  // Supports sharing a search as a link (?search=...) - whoever opens the
  // link sees the same search pre-filled.
  const [searchParams] = useSearchParams()
  const [result, setResult] = useState<PagedResult<CaseSummary> | null>(null)
  const [search, setSearch] = useState(searchParams.get('search') ?? '')
  const [status, setStatus] = useState('')
  const [priority, setPriority] = useState('')
  const [mine, setMine] = useState(false)
  const [sort, setSort] = useState('CreatedAt')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const params: CaseListParams = {
      search: search || undefined,
      status: status || undefined,
      priority: priority || undefined,
      mine: mine || undefined,
      sort,
      sortDirection,
      page,
      pageSize: PAGE_SIZE,
    }
    setLoading(true)
    casesApi
      .list(params)
      .then(setResult)
      .finally(() => setLoading(false))
  }, [search, status, priority, mine, sort, sortDirection, page])

  function toggleSort(column: string) {
    if (sort === column) {
      setSortDirection((d) => (d === 'asc' ? 'desc' : 'asc'))
    } else {
      setSort(column)
      setSortDirection('asc')
    }
    setPage(1)
  }

  function sortIndicator(column: string) {
    if (sort !== column) return ''
    return sortDirection === 'asc' ? ' ▲' : ' ▼'
  }

  const totalPages = result ? Math.max(1, Math.ceil(result.totalCount / PAGE_SIZE)) : 1

  return (
    <div className="case-list-page">
      <div className="case-list-toolbar">
        <input
          placeholder="Søk i saker…"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value)
            setPage(1)
          }}
        />
        <select
          value={status}
          onChange={(e) => {
            setStatus(e.target.value)
            setPage(1)
          }}
        >
          <option value="">Alle statuser</option>
          {CASE_STATUS_OPTIONS.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
        <select
          value={priority}
          onChange={(e) => {
            setPriority(e.target.value)
            setPage(1)
          }}
        >
          <option value="">Alle prioriteter</option>
          {CASE_PRIORITY_OPTIONS.map((p) => (
            <option key={p} value={p}>
              {p}
            </option>
          ))}
        </select>
        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={mine}
            onChange={(e) => {
              setMine(e.target.checked)
              setPage(1)
            }}
          />
          Mine saker
        </label>
        <Link to="/saker/ny" className="button-link">
          Ny sak
        </Link>
      </div>

      {loading && <p>Laster…</p>}

      {result && (
        <>
          <table className="case-table">
            <thead>
              <tr>
                <th onClick={() => toggleSort('caseNumber')}>Saksnummer{sortIndicator('caseNumber')}</th>
                <th onClick={() => toggleSort('title')}>Tittel{sortIndicator('title')}</th>
                <th onClick={() => toggleSort('status')}>Status{sortIndicator('status')}</th>
                <th onClick={() => toggleSort('priority')}>Prioritet{sortIndicator('priority')}</th>
                <th>Kunde</th>
                <th>Avdeling</th>
                <th>Saksbehandler</th>
                <th onClick={() => toggleSort('deadline')}>Frist{sortIndicator('deadline')}</th>
              </tr>
            </thead>
            <tbody>
              {result.items.map((c) => (
                <tr key={c.id}>
                  <td>
                    <Link to={`/saker/${c.id}`}>{c.caseNumber}</Link>
                  </td>
                  <td>{c.title}</td>
                  <td>{c.status}</td>
                  <td>{c.priority}</td>
                  <td>{c.customerName}</td>
                  <td>{c.departmentName}</td>
                  <td>{c.assignedToName ?? '—'}</td>
                  <td>{c.deadline ? new Date(c.deadline).toLocaleDateString('nb-NO') : '—'}</td>
                </tr>
              ))}
              {result.items.length === 0 && (
                <tr>
                  <td
                    colSpan={8}
                    dangerouslySetInnerHTML={{
                      __html: search ? `Ingen saker funnet for <strong>${search}</strong>.` : 'Ingen saker funnet.',
                    }}
                  />
                </tr>
              )}
            </tbody>
          </table>

          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Forrige
            </button>
            <span>
              Side {page} av {totalPages} ({result.totalCount} saker)
            </span>
            <button disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
              Neste
            </button>
          </div>
        </>
      )}
    </div>
  )
}
