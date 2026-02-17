import { useState } from 'react';
import { Card, Collapse } from 'react-bootstrap';
import './ATSScoreCard.css';

export default function ATSScoreCard({ data }) {
  const [open, setOpen] = useState(false);
  if (!data) return null;

  const c = data.colorClass || 'yellow';
  const pct = Math.min(Math.max(data.overall, 0), 100);
  const labelMap = { green: 'ATS Ready', yellow: 'Needs Attention', red: 'ATS Risk' };

  return (
    <Card className="ats-card">
      <Card.Body>
        <div className="ats-card-header" onClick={() => setOpen(!open)}>
          <div>
            <h5 className="ats-card-title">🤖 ATS Compatibility</h5>
            <div className="ats-score-row">
              <span className={`ats-percent ${c}`}>{pct}</span>
              <span className="ats-denom">/100</span>
              <span className={`ats-badge ${c}`}>{labelMap[c]}</span>
            </div>
          </div>
          <span className="toggle-icon">{open ? '▲' : '▼'}</span>
        </div>

        <div className="ats-bar-track">
          <div className={`ats-bar-fill ${c}`} style={{ width: `${pct}%` }} />
        </div>

        <Collapse in={open}>
          <div style={{ marginTop: 'var(--spacing-md)' }}>
            {!data.issues?.length ? (
              <p className="ats-all-clear">✅ Your CV is fully optimized for ATS </p>
            ) : (
              <>
                <div className="ats-issues-title">Issues found</div>
                {data.issues.map((issue, i) => (
                  <div key={i} className="ats-issue-row">
                    <span className={`severity-badge ${issue.severity}`}>{issue.severity}</span>
                    <div>
                      <div className="issue-text">{issue.issue}</div>
                      <div className="issue-fix">→ {issue.fix}</div>
                    </div>
                  </div>
                ))}
              </>
            )}
          </div>
        </Collapse>
      </Card.Body>
    </Card>
  );
}
