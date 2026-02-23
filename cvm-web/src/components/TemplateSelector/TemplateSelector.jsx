import { Form } from 'react-bootstrap';
import { TEMPLATES } from './templateSelector.const'
import './TemplateSelector.css';

export default function TemplateSelector({ value, onChange, disabled }) {
  return (
    <Form.Group className="mb-4">
      <Form.Label className="form-label-custom mb-3">Resume Template</Form.Label>
      <div className="template-options">
        {TEMPLATES.map(t => (
          <div
            key={t.key}
            className={`template-card ${value === t.key ? 'selected' : ''} ${disabled ? 'disabled' : ''}`}
            onClick={() => !disabled && onChange(t.key)}
          >
            <div className="template-preview">{t.preview}</div>
            <div className="template-info">
              <div className="template-name">{t.name}</div>
              <div className="template-desc">{t.desc}</div>
            </div>
            {value === t.key && <div className="template-check">✓</div>}
          </div>
        ))}
      </div>
    </Form.Group>
  );
}
