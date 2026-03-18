import { Card, Button } from 'react-bootstrap';
import './HistoryPanel.css';

function formatDate(isoDate) {
  if (!isoDate) return '-';
  const date = new Date(isoDate);
  return Number.isNaN(date.getTime()) ? '-' : date.toLocaleString();
}

export default function HistoryPanel({
  entries,
  onLoad,
  onDelete,
  onClear,
  onClose,
  disabled,
}) {
  return (
    <Card className="main-card mt-3">
      <Card.Body className="p-3">
        <div className="history-header">
          <h6 className="mb-0">Generation History</h6>
          <div className="history-header-actions">
            <Button
              variant="outline-danger"
              size="sm"
              onClick={onClear}
              disabled={disabled || entries.length === 0}
            >
              Clear
            </Button>
            <Button variant="outline-secondary" size="sm" onClick={onClose}>
              Close
            </Button>
          </div>
        </div>

        {entries.length === 0 ? (
          <p className="history-empty mb-0">No generations yet.</p>
        ) : (
          <div className="history-list">
            {entries.map((entry) => (
              <div className="history-item" key={entry.id}>
                <div className="history-meta">
                  <div className="history-title" title={entry.jobTitle}>{entry.jobTitle}</div>
                  <div className="history-subtitle">
                    <span>{formatDate(entry.generatedAt)}</span>
                    <span>Template: {entry.template}</span>
                    <span>Match: {entry.matchScore ?? 'N/A'}%</span>
                  </div>
                </div>
                <div className="history-item-actions">
                  <Button
                    variant="outline-primary"
                    size="sm"
                    onClick={() => onLoad(entry)}
                    disabled={disabled}
                  >
                    Regenerate
                  </Button>
                  <Button
                    variant="outline-danger"
                    size="sm"
                    onClick={() => onDelete(entry.id)}
                    disabled={disabled}
                  >
                    Delete
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card.Body>
    </Card>
  );
}
