import { Form } from 'react-bootstrap';
import { FORMATS } from './formats.const'
import './FormatSelector.css';

const FormatSelector = ({ selectedFormats, onChange, disabled }) => {

  return (
    <Form.Group className="mb-4">
      <Form.Label className="form-label-custom mb-3">
        Select Format(s)
      </Form.Label>
      <div className="format-options">
        {FORMATS.map(format => (
          <div
            key={format.key}
            className={`format-card ${selectedFormats[format.key] ? 'selected' : ''}`}
            onClick={() => !disabled && onChange(format.key)}
          >
            <div className="format-icon">
              {format.icon}
            </div>
            <div className="format-name">{format.name}</div>
            <Form.Check
              type="checkbox"
              checked={selectedFormats[format.key]}
              onChange={() => onChange(format.key)}
              disabled={disabled}
              className="format-checkbox"
            />
          </div>
        ))}
      </div>
    </Form.Group>
  );
};

export default FormatSelector;