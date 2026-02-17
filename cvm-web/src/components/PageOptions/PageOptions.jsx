import { Form } from 'react-bootstrap';
import './PageOptions.css';

export default function PageOptions({ allowTwoPages, onChange, disabled }) {
  return (
    <Form.Group className="mb-4">
      <Form.Label className="form-label-custom mb-3">
        Page Options
      </Form.Label>
      <div className="page-options-group">
        <div
          className={`page-option-card ${!allowTwoPages ? 'selected' : ''} ${disabled ? 'disabled' : ''}`}
          onClick={() => !disabled && onChange(false)}
        >
          <div className="page-option-icon">
            <i className="bi bi-file-earmark-text"></i>
          </div>
          <div className="page-option-label">1 Page</div>
          <div className="page-option-desc">Recommended</div>
        </div>
        <div
          className={`page-option-card ${allowTwoPages ? 'selected' : ''} ${disabled ? 'disabled' : ''}`}
          onClick={() => !disabled && onChange(true)}
        >
          <div className="page-option-icon">
            <i className="bi bi-files"></i>
          </div>
          <div className="page-option-label">Up to 2 Pages</div>
          <div className="page-option-desc">8+ years exp.</div>
        </div>
      </div>
    </Form.Group>
  );
}
