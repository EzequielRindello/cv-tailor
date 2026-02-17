import React, { useState } from 'react';
import { Card, Collapse } from 'react-bootstrap';
import './KeywordAnalysisCard.css';

function KeywordRow({ item }) {
  return (
    <div className="keyword-row">
      <span className={`kw-status ${item.presentInCV ? 'present' : 'missing'}`}>
        {item.presentInCV ? '✓' : '✗'}
      </span>
      <span className="kw-word">{item.word}</span>
      <span className="kw-freq">×{item.frequency}</span>
    </div>
  );
}

export default function KeywordAnalysisCard({ data }) {
  const [open, setOpen] = useState(false);
  if (!data) return null;

  const densityPct = Math.round(data.keywordDensity * 100);
  const densityColor = densityPct >= 70
    ? 'var(--color-success)'
    : densityPct >= 50
    ? 'var(--color-warning)'
    : 'var(--color-error)';

  return (
    <Card className="keyword-card">
      <Card.Body>
        <div className="keyword-card-header" onClick={() => setOpen(!open)}>
          <h5 className="keyword-card-title">📊 Keyword Analysis</h5>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--spacing-sm)' }}>
            <span className="density-badge" style={{ background: densityColor }}>
              {densityPct}% density
            </span>
            <span className="toggle-icon">{open ? '▲' : '▼'}</span>
          </div>
        </div>

        <Collapse in={open}>
          <div>
            <div className="kw-grid">
              {data.technicalKeywords?.length > 0 && (
                <div className="kw-group">
                  <div className="kw-group-title">⚙️ Technical</div>
                  {data.technicalKeywords.map(item => (
                    <KeywordRow key={item.word} item={item} />
                  ))}
                </div>
              )}

              {data.softSkillKeywords?.length > 0 && (
                <div className="kw-group">
                  <div className="kw-group-title">🤝 Soft Skills</div>
                  {data.softSkillKeywords.map(item => (
                    <KeywordRow key={item.word} item={item} />
                  ))}
                </div>
              )}
            </div>

            {data.suggestions?.length > 0 && (
              <div className="kw-suggestions">
                <div className="kw-group-title">💡 Suggestions</div>
                <ul className="kw-suggestion-list">
                  {data.suggestions.map((s, i) => <li key={i}>{s}</li>)}
                </ul>
              </div>
            )}
          </div>
        </Collapse>
      </Card.Body>
    </Card>
  );
}
