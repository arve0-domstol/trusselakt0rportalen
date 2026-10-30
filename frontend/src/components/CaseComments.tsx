import { useEffect, useState, type FormEvent } from 'react'
import { commentsApi } from '../api/comments'
import type { Comment } from '../types/comment'

export function CaseComments({ caseId }: { caseId: number }) {
  const [comments, setComments] = useState<Comment[]>([])
  const [text, setText] = useState('')
  const [isInternal, setIsInternal] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  function reload() {
    commentsApi.list(caseId).then(setComments)
  }

  useEffect(reload, [caseId])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!text.trim()) return
    setSubmitting(true)
    try {
      await commentsApi.create(caseId, { text, isInternal })
      setText('')
      setIsInternal(false)
      reload()
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="case-comments">
      <h2>Kommentarer</h2>
      <ul className="comment-list">
        {comments.map((c) => (
          <li key={c.id} className={c.isInternal ? 'comment comment-internal' : 'comment'}>
            <div className="comment-meta">
              <strong>{c.authorName}</strong>
              <span>{new Date(c.createdAt).toLocaleString('nb-NO')}</span>
              {c.isInternal && <span className="comment-badge">Intern</span>}
            </div>
            <div className="comment-text" dangerouslySetInnerHTML={{ __html: c.text }} />
          </li>
        ))}
        {comments.length === 0 && <li className="comment-empty">Ingen kommentarer ennå.</li>}
      </ul>
      <form onSubmit={handleSubmit} className="comment-form">
        <textarea
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder="Skriv en kommentar…"
          rows={3}
        />
        <label className="checkbox-label">
          <input type="checkbox" checked={isInternal} onChange={(e) => setIsInternal(e.target.checked)} />
          Intern kommentar
        </label>
        <button type="submit" disabled={submitting || !text.trim()}>
          {submitting ? 'Sender…' : 'Legg til kommentar'}
        </button>
      </form>
    </section>
  )
}
