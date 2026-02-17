import { useState, useEffect } from 'react';
import { Card, Button } from 'react-bootstrap';
import './BulletEditor.css';

export default function BulletEditor({ generatedCV, onRegenerate, onCancel, loading }) {
  const [selections, setSelections] = useState({});
  const [localCV, setLocalCV] = useState(null);
  const [dragInfo, setDragInfo] = useState(null);

  useEffect(() => {
    if (!generatedCV?.experience) return;
    // Deep copy - to avoid mutating parent state when reordering
    const cvCopy = JSON.parse(JSON.stringify(generatedCV));
    setLocalCV(cvCopy);

    const initial = {};
    cvCopy.experience.forEach((exp, i) => {
      initial[`exp${i}`] = new Array(exp.bullets.length).fill(true);
    });
    setSelections(initial);
  }, [generatedCV]);

  if (!localCV?.experience) return null;

  const toggleBullet = (expKey, bulletIdx) => {
    setSelections(prev => {
      const arr = [...prev[expKey]];
      const selectedCount = arr.filter(Boolean).length;
      if (arr[bulletIdx] && selectedCount <= 1) return prev;
      arr[bulletIdx] = !arr[bulletIdx];
      return { ...prev, [expKey]: arr };
    });
  };

  const handleDragStart = (expKey, idx) => setDragInfo({ expKey, idx });

  const handleDragOver = (e, expKey, idx) => {
    e.preventDefault();
    if (!dragInfo || dragInfo.expKey !== expKey || dragInfo.idx === idx) return;

    // Reorder bullets in localCV
    setLocalCV(prev => {
      const copy = JSON.parse(JSON.stringify(prev));
      const expIdx = parseInt(expKey.replace('exp', ''));
      const bullets = copy.experience[expIdx].bullets;
      const [moved] = bullets.splice(dragInfo.idx, 1);
      bullets.splice(idx, 0, moved);
      return copy;
    });

    // Reorder selections to match new bullet order
    setSelections(prev => {
      const arr = [...prev[expKey]];
      const [moved] = arr.splice(dragInfo.idx, 1);
      arr.splice(idx, 0, moved);
      return { ...prev, [expKey]: arr };
    });

    setDragInfo({ expKey, idx });
  };

  const handleDragEnd = () => setDragInfo(null);

  const handleRegenerate = () => {
    const bulletSelections = {};
    Object.entries(selections).forEach(([key, bools]) => {
      bulletSelections[key] = bools
        .map((selected, i) => (selected ? i : -1))
        .filter(i => i >= 0);
    });
    onRegenerate(bulletSelections);
  };

  const totalSelected = Object.values(selections).flat().filter(Boolean).length;
  const totalBullets = Object.values(selections).flat().length;

  return (
    <Card className="bullet-editor-card">
      <Card.Body>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-start',
            marginBottom: 'var(--spacing-md)',
          }}
        >
          <div>
            <h5 className="bullet-editor-title">
              <i className="bi bi-pencil-square bullet-title-icon" />
              Edit Bullets
            </h5>
            <div className="bullet-editor-subtitle">
              {totalSelected}/{totalBullets} selected · Uncheck to exclude · Drag
              <i className="bi bi-grip-vertical bullet-drag-icon" />
              to reorder
            </div>
          </div>
        </div>


        {localCV.experience.map((exp, expIdx) => {
          const expKey = `exp${expIdx}`;
          const expSelections = selections[expKey] || [];
          const selectedCount = expSelections.filter(Boolean).length;

          return (
            <div key={expKey} className="bullet-exp-block">
              <div className="bullet-exp-header">
                <div className="bullet-exp-role">{exp.role}</div>
                <div className="exp-company-label">{exp.company} · {exp.startDate} – {exp.endDate}</div>
              </div>

              <div className="bullets-list">
                {exp.bullets.map((bullet, bulletIdx) => {
                  const isSelected = expSelections[bulletIdx] !== false;
                  const canDeselect = selectedCount > 1;

                  return (
                    <div
                      key={bulletIdx}
                      className={`bullet-row${!isSelected ? ' deselected' : ''}${dragInfo?.expKey === expKey && dragInfo?.idx === bulletIdx ? ' dragging' : ''}`}
                      draggable
                      onDragStart={() => handleDragStart(expKey, bulletIdx)}
                      onDragOver={(e) => handleDragOver(e, expKey, bulletIdx)}
                      onDragEnd={handleDragEnd}
                    >
                      <span className="drag-handle">⠿</span>
                      <input
                        type="checkbox"
                        className="bullet-checkbox"
                        checked={isSelected}
                        disabled={isSelected && !canDeselect}
                        onChange={() => toggleBullet(expKey, bulletIdx)}
                        title={isSelected && !canDeselect ? 'At least 1 bullet must remain' : ''}
                      />
                      <span className="bullet-text">{bullet}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}

        <div className="bullet-editor-actions">
          <Button variant="outline-secondary" size="sm" onClick={onCancel} disabled={loading}>
            Cancel
          </Button>
          <Button variant="primary" size="sm" onClick={handleRegenerate} disabled={loading}>
            {loading ? 'Regenerating...' : `Regenerate CV (${totalSelected} bullets)`}
          </Button>
        </div>
      </Card.Body>
    </Card>
  );
}
