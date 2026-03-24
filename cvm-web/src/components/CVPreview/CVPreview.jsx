import { useMemo } from 'react';
import { Card } from 'react-bootstrap';
import './CVPreview.css';

const VIEWPORT_META = '<meta name="viewport" content="width=device-width, initial-scale=1.0">';
const PREVIEW_FIX_STYLE = `
<style id="cv-preview-fix">
  html, body {
    max-width: 100% !important;
    overflow-x: hidden !important;
  }

  body {
    margin: 0 auto !important;
    padding: 12px !important;
  }

  a,
  p,
  span,
  div,
  li {
    overflow-wrap: anywhere;
  }

  /* Modern template uses a 2-column grid that can overflow in iframe preview. */
  .body-grid {
    width: 100% !important;
    grid-template-columns: minmax(0, 38%) minmax(0, 1fr) !important;
    column-gap: 10pt !important;
  }

  .left-col,
  .right-col {
    min-width: 0 !important;
  }

  .exp-header {
    flex-wrap: wrap;
    gap: 2pt 8pt;
  }

  .exp-dates {
    white-space: normal !important;
  }

  @media (max-width: 860px) {
    .body-grid {
      grid-template-columns: 1fr !important;
      row-gap: 8pt !important;
    }

    .left-col {
      border-right: 0 !important;
      padding-right: 0 !important;
    }
  }
</style>`;

function normalizePreviewHtml(rawHtml) {
  let normalized = rawHtml;

  if (!/<meta[^>]*name=["']viewport["'][^>]*>/i.test(normalized)) {
    if (/<head[^>]*>/i.test(normalized)) {
      normalized = normalized.replace(/<head([^>]*)>/i, `<head$1>\n${VIEWPORT_META}`);
    } else {
      normalized = `${VIEWPORT_META}\n${normalized}`;
    }
  }

  if (!/id=["']cv-preview-fix["']/i.test(normalized)) {
    if (/<\/head>/i.test(normalized)) {
      normalized = normalized.replace(/<\/head>/i, `${PREVIEW_FIX_STYLE}\n</head>`);
    } else {
      normalized = `${PREVIEW_FIX_STYLE}\n${normalized}`;
    }
  }

  return normalized;
}

export default function CVPreview({ html }) {
  const normalizedHtml = useMemo(() => normalizePreviewHtml(html ?? ''), [html]);

  if (!html) return null;

  return (
    <Card className="main-card mb-4">
      <Card.Body className="p-3">
        <h5 className="mb-3">CV Preview</h5>
        <div className="cv-preview-frame-wrapper">
          <iframe
            srcDoc={normalizedHtml}
            title="CV Preview"
            className="cv-preview-frame"
            sandbox="allow-same-origin"
          />
        </div>
      </Card.Body>
    </Card>
  );
}
