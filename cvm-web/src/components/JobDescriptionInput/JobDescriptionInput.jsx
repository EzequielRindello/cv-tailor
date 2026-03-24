import { Form } from 'react-bootstrap';
import './JobDescriptionInput.css';

const JobDescriptionInput = ({ value, onChange, disabled, helperText }) => {
  return (
    <Form.Group className="mb-4">
      <Form.Label className="form-label-custom">
        Job Description
      </Form.Label>
      <Form.Control
        as="textarea"
        rows={12}
        value={value}
        onChange={onChange}
        placeholder="Paste the job description here..."
        disabled={disabled}
        className="job-description-input"
        title={helperText || ''}
      />
      {helperText && <div className="input-helper-text">{helperText}</div>}
    </Form.Group>
  );
};

export default JobDescriptionInput;