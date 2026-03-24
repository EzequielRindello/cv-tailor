import { Form } from 'react-bootstrap';
import './ProfileSelector.css';

export default function ProfileSelector({ profiles, value, onChange, disabled }) {
  if (!profiles || profiles.length <= 1) return null;

  return (
    <Form.Group className="mb-4">
      <Form.Label className="form-label-custom">CV Profile</Form.Label>
      <Form.Select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
        className="profile-selector"
      >
        {profiles.map((profile) => (
          <option key={profile.id} value={profile.id}>
            {profile.name}
          </option>
        ))}
      </Form.Select>
    </Form.Group>
  );
}
