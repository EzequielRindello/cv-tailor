import { useState } from 'react';
import { Card, Collapse } from 'react-bootstrap';
import { ATS_STATUS_LABEL, DEFAULT_ATS_STATUS, ATS_SCORE_MIN, ATS_SCORE_MAX } from './atsScore.const';

import './ATSScoreCard.css';

export default function ATSScoreCard({ data }) {
  const [open, setOpen] = useState(false);
  if (!data)
    return null;

  const status = data.colorClass || DEFAULT_ATS_STATUS;
  const score = Math.min(Math.max(data.overall, ATS_SCORE_MIN), ATS_SCORE_MAX);

  return (
    <Card className="ats-card">
      <Card.Body>
        <div className="ats-card-header" onClick={() => setOpen(!open)}>
          <div>
            <h5 className="ats-card-title mb-3">
              <i className="bi bi-cpu ats-title-icon" />
              ATS Compatibility
            </h5>

            <div className="ats-score-row">
              <span className={`ats-percent ${status}`}>{score}</span>
              <span className="ats-denom">/100</span>
              <span className={`ats-badge ${status}`}>
                {ATS_STATUS_LABEL[status]}
              </span>
            </div>
          </div>

          <span className="toggle-icon">
            <i className={`bi ${open ? 'bi-chevron-up' : 'bi-chevron-down'}`} />
          </span>
        </div>

        <div className="ats-bar-track">
          <div
            className={`ats-bar-fill ${status}`}
            style={{ width: `${score}%` }}
          />
        </div>

        <Collapse in={open}>
          <div style={{ marginTop: 'var(--spacing-md)' }}>
            {!data.issues?.length ? (
              <p className="ats-all-clear">
                <i className="bi bi-check-circle-fill ats-success-icon" />
                Your CV is fully optimized for ATS
              </p>
            ) : (
              <>
                <div className="ats-issues-title">Issues found</div>
                {data.issues.map((issue, i) => (
                  <div key={i} className="ats-issue-row">
                    <span className={`severity-badge ${issue.severity}`}>
                      {issue.severity}
                    </span>
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
