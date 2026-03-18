import { Card } from 'react-bootstrap';
import './CVPreview.css';

export default function CVPreview({ html }) {
  if (!html) return null;

  return (
    <Card className="main-card mb-4">
      <Card.Body className="p-3">
        <h5 className="mb-3">CV Preview</h5>
        <div className="cv-preview-frame-wrapper">
          <iframe
            srcDoc={html}
            title="CV Preview"
            className="cv-preview-frame"
            sandbox="allow-same-origin"
          />
        </div>
      </Card.Body>
    </Card>
  );
}
