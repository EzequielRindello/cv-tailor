import { Card } from 'react-bootstrap';
import './MatchScoreCard.css';

export default function MatchScoreCard({ data }) {
  if (!data) return null;

  const c = data.colorClass || 'yellow';
  const pct = Math.min(Math.max(data.overall, 0), 100);
  const labelMap = { green: 'Strong Match', yellow: 'Moderate Match', red: 'Weak Match' };

  return (
    <Card className="match-score-card">
      <Card.Body>
        <div className="match-card-header">
          <h5 className="match-card-title">
            <i className="bi bi-bullseye match-title-icon" />
            Match Score
          </h5>
          <span className={`match-badge ${c}`}>{labelMap[c]}</span>
        </div>

        <div className="match-percent-row">
          <span className={`match-percent ${c}`}>{pct}%</span>
        </div>

        <div className="match-bar-track">
          <div className={`match-bar-fill ${c}`} style={{ width: `${pct}%` }} />
        </div>

        <div className="match-breakdown">
          <div className="breakdown-item">
            <span className={`breakdown-icon ${data.skillsMatched >= data.totalSkills * 0.6 ? 'ok' : 'warn'}`}>
              <i className={`bi ${data.skillsMatched >= data.totalSkills * 0.6 ? 'bi-check' : 'bi-circle'}`} />
            </span>
            Technical skills: <strong>{data.skillsMatched}/{data.totalSkills}</strong> matched
          </div>
          <div className="breakdown-item">
            <span className={`breakdown-icon ${data.keywordsMatched >= data.totalKeywords * 0.5 ? 'ok' : 'warn'}`}>
              <i className={`bi ${data.keywordsMatched >= data.totalKeywords * 0.5 ? 'bi-check' : 'bi-circle'}`} />
            </span>
            Keywords: <strong>{data.keywordsMatched}/{data.totalKeywords}</strong> present
          </div>
          <div className="breakdown-item">
            <span className={`breakdown-icon ${data.seniorityMatch ? 'ok' : 'warn'}`}>
              <i className={`bi ${data.seniorityMatch ? 'bi-check' : 'bi-circle'}`} />
            </span>
            Seniority level: <strong>{data.seniorityMatch ? 'Match' : 'Mismatch'}</strong>
          </div>
        </div>
      </Card.Body>
    </Card>
  );
}
