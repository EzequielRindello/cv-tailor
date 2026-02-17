import { useState } from 'react';
import { Card, Collapse } from 'react-bootstrap';
import './MissingSkillsCard.css';

export default function MissingSkillsCard({ data }) {
  const [open, setOpen] = useState(false);
  if (!data) return null;

  const hasAnything = data.requiredMissing?.length > 0 || data.niceToHaveMissing?.length > 0;

  if (!hasAnything) {
    return (
      <Card className="missing-skills-card">
        <Card.Body>
          <div className="missing-card-header">
            <h5 className="missing-card-title">
              <i className="bi bi-check-circle-fill missing-title-icon ok" />
              Skills Coverage
            </h5>
          </div>
          <p className="all-clear-text">All required skills are present in your CV Master!</p>
        </Card.Body>
      </Card>
    );
  }

  const total = (data.requiredMissing?.length || 0) + (data.niceToHaveMissing?.length || 0);

  return (
    <Card className="missing-skills-card">
      <Card.Body>
        <div className="missing-card-header mb-3" onClick={() => setOpen(!open)}>
          <h5 className="missing-card-title">
            <i className="bi bi-exclamation-triangle-fill missing-title-icon warn" />
            Missing Skills ({total})
          </h5>
          <span className="toggle-icon">
            <i className={`bi ${open ? 'bi-chevron-up' : 'bi-chevron-down'}`} />
          </span>
        </div>

        {data.potentialScoreIncrease > 0 && (
          <p className="tip-text">
            <i className="bi bi-lightbulb-fill missing-tip-icon" />
            Adding missing skills could increase your match score by up to <strong>+{data.potentialScoreIncrease}%</strong>
          </p>
        )}

        <Collapse in={open}>
          <div style={{ marginTop: 'var(--spacing-md)' }}>
            {data.requiredMissing?.length > 0 && (
              <div className="missing-group">
                <div className="group-label required">Required — Missing</div>
                <div className="skill-tags">
                  {data.requiredMissing.map(s => (
                    <span key={s} className="skill-pill required">{s}</span>
                  ))}
                </div>
              </div>
            )}

            {data.niceToHaveMissing?.length > 0 && (
              <div className="missing-group">
                <div className="group-label nice">Nice to Have — Missing</div>
                <div className="skill-tags">
                  {data.niceToHaveMissing.map(s => (
                    <span key={s} className="skill-pill nice">{s}</span>
                  ))}
                </div>
              </div>
            )}

            {data.suggestions?.length > 0 && (
              <div className="suggestions">
                <div className="group-label">Suggestions</div>
                <ul className="suggestion-list">
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
